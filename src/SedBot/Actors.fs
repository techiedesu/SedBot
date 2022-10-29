module SedBot.Actors

open System.Threading.Tasks
open Akka.FSharp
open Funogram.Telegram
open Funogram.Telegram.Types
open Funogram.Types
open Microsoft.FSharp.Core

type SendTelegramResponseMail =
    | SendMessage of chatId: int64 * text: string
    | SendMessageAndDeleteAfter of chatId: int64 * text: string * ms: int
    | SendMarkupMessageAndDeleteAfter of chatId: int64 * text: string * parseMode: ParseMode * ms: int
    | SendMarkupMessageReplyAndDeleteAfter of chatId: int64 * text: string * parseMode: ParseMode * replyToMessageId: int64 * ms: int
    | MessageReply of chatId: int64 * text: string * replyToMessageId: int64
    | MarkupMessageReply of chatId: int64 * text: string * replyToMessageId: int64 * parseMode: ParseMode
    | DeleteMessage of chatId: int64 * messageId: int64
    | SetConfig of config: BotConfig
    | SendAnimationReply of chatId: int64 * animation: InputFile * replyToMessageId: int64
    | SendVideoReply of chatId: int64 * video: InputFile * replyToMessageId: int64
    | SendPhotoReply of chatId: int64 * photo: InputFile * replyToMessageId: int64

module [<RequireQualifiedAccess>] TgApi =
    let mutable actor : Akka.Actor.IActorRef = null

    /// Send message to chat
    let sendMessage chatId text =
        actor <! SendTelegramResponseMail.SendMessage (chatId, text)

    /// Send message to chat and delete after some milliseconds
    let sendMessageAndDeleteAfter chatId text ms =
        actor <! SendTelegramResponseMail.SendMessageAndDeleteAfter (chatId, text, ms)

    /// Send message as reply to chat
    let sendMessageReply chatId text replyToMessageId =
        actor <! SendTelegramResponseMail.MessageReply (chatId, text, replyToMessageId)

    /// Send message as reply to chat with parse mode (Markdown or Html)
    let sendMarkupMessageReply chatId text replyToMessageId parseMode =
        actor <! SendTelegramResponseMail.MarkupMessageReply (chatId, text, replyToMessageId, parseMode)

    /// Send message to chat with parse mode (Markdown or Html) and delete after some milliseconds
    let sendMarkupMessageAndDeleteAfter chatId text parseMode ms =
        actor <! SendTelegramResponseMail.SendMarkupMessageAndDeleteAfter (chatId, text, parseMode, ms)

    /// Send message reply to chat with parse mode (Markdown or Html) and delete after some milliseconds
    let sendMarkupMessageReplyAndDeleteAfter chatId text parseMode replyToMessageId ms =
        actor <! SendTelegramResponseMail.SendMarkupMessageReplyAndDeleteAfter (chatId, text, parseMode, replyToMessageId, ms)

    /// Delete message in chat
    let deleteMessage chatId messageId =
        actor <! SendTelegramResponseMail.DeleteMessage (chatId, messageId)

    /// Send animation as reply
    let sendAnimationReply chatId animation replyToMessageId =
        actor <! SendTelegramResponseMail.SendAnimationReply (chatId, animation, replyToMessageId)

    /// Send video as reply
    let sendVideoReply chatId video replyToMessageId =
        actor <! SendTelegramResponseMail.SendVideoReply (chatId, video, replyToMessageId)

    /// Send photo as reply
    let sendPhotoReply chatId photo replyToMessageId =
        actor <! SendTelegramResponseMail.SendPhotoReply (chatId, photo, replyToMessageId)

let rec responseTelegramActor (mailbox: Actor<SendTelegramResponseMail>) =
    let mutable cfg : BotConfig option = None
    let api request =
        match cfg with
        | Some botConfig ->
            request
            |> Funogram.Api.api botConfig
            |> Async.RunSynchronously
            |> ignore
        | _ -> ()

    let rec loop () = actor {
        let! message = mailbox.Receive ()

        match message with
        | SendMessage(chatId, text) ->
            Api.sendMessage chatId text
            |> api
        | MessageReply (chatId, text, replyToMessageId) ->
            Api.sendMessageReply chatId text replyToMessageId
            |> api
        | MarkupMessageReply(chatId, text, replyToMessageId, parseMode) ->
            Req.SendMessage.Make(chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)
            |> api
        | DeleteMessage(chatId, messageId) ->
            Api.deleteMessage chatId messageId
            |> api
        | SendMessageAndDeleteAfter(chatId, text, ms) ->
            match cfg with
            | Some cfg ->
                let messageId =
                    Api.sendMessage chatId text
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                task {
                    do! Task.Delay(ms)
                    messageId |> Option.iter (TgApi.deleteMessage chatId)
                } |> ignore
            | _ -> ()
        | SendMarkupMessageAndDeleteAfter(chatId, text, mode, ms) ->
            match cfg with
            | Some cfg ->
                let messageId =
                    Api.sendTextMarkup chatId text mode
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                task {
                    do! Task.Delay(ms)
                    messageId |> Option.iter (TgApi.deleteMessage chatId)
                } |> ignore
            | _ -> ()
        | SendMarkupMessageReplyAndDeleteAfter(chatId, text, mode, replyToMessageId, ms) ->
            match cfg with
            | Some cfg ->
                let messageId =
                    Api.sendTextMarkupReply chatId text replyToMessageId mode
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                task {
                    do! Task.Delay(ms)
                    messageId |> Option.iter (TgApi.deleteMessage chatId)
                } |> ignore
            | _ -> ()
        | SetConfig botConfig ->
            cfg <-
                if System.Object.ReferenceEquals(null, botConfig) then
                    None
                else
                    Some botConfig
        | SendAnimationReply (chatId, animation, replyToMessageId) ->
            Api.sendAnimationReply chatId animation replyToMessageId
            |> api
        | SendVideoReply (chatId, video, replyToMessageId) ->
            Api.sendVideoReply chatId video replyToMessageId
            |> api
        | SendPhotoReply (chatId, animation, replyToMessageId) ->
            Api.sendPhotoReply chatId animation replyToMessageId
            |> api

        return! loop ()
    }
    loop ()
