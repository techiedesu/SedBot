module SedBot.Actors

open Akka.FSharp
open Funogram.Telegram
open Funogram.Telegram.Types
open Funogram.Types

type SendTelegramResponseMail =
    | SendMessage of chatId: int64 * text: string
    | MessageReply of chatId: int64 * text: string * replyToMessageId: int64
    | MarkupMessageReply of chatId: int64 * text: string * replyToMessageId: int64 * parseMode: ParseMode
    | DeleteMessage of chatId: int64 * messageId: int64
    | SetConfig of config: BotConfig

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
        | SetConfig botConfig ->
            printfn "Set config"
            cfg <-
                if System.Object.ReferenceEquals(null, botConfig) then
                    None
                else
                    Some botConfig

        return! loop ()
    }
    loop ()

module [<RequireQualifiedAccess>] TgApi =
    let mutable actor : Akka.Actor.IActorRef = null

    let sendMessage chatId text =
        actor <! SendTelegramResponseMail.SendMessage (chatId, text)

    let sendMessageReply chatId text replyToMessageId =
        actor <! SendTelegramResponseMail.MessageReply (chatId, text, replyToMessageId)

    let sendMarkupMessageReply chatId text replyToMessageId parseMode =
        actor <! SendTelegramResponseMail.MarkupMessageReply (chatId, text, replyToMessageId, parseMode)

    let deleteMessage chatId messageId =
        actor <! SendTelegramResponseMail.DeleteMessage (chatId, messageId)
