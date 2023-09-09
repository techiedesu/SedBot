module SedBot.Actors

open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open Funogram.Telegram
open Funogram.Telegram.Types
open Funogram.Types
open Microsoft.FSharp.Core
open SedBot.Common
open SedBot.Utilities
open Microsoft.Extensions.Logging

type TelegramSendingMessage =
    | SendMessage of chatId: int64 * text: string
    | SendMessageAndDeleteAfter of chatId: int64 * text: string * ms: int
    | SendMessageReplyAndDeleteAfter of chatId: int64 * text: string * ms: int
    | SendMarkupMessageAndDeleteAfter of chatId: int64 * text: string * parseMode: ParseMode * ms: int
    | SendMarkupMessageReplyAndDeleteAfter of chatId: int64 * text: string * parseMode: ParseMode * replyToMessageId: int64 * ms: int
    | MessageReply of chatId: int64 * text: string * replyToMessageId: int64
    | MarkupMessageReply of chatId: int64 * text: string * replyToMessageId: int64 * parseMode: ParseMode
    | DeleteMessage of chatId: int64 * messageId: int64
    | SetConfig of config: BotConfig
    | SendAnimationReply of chatId: int64 * animation: InputFile * replyToMessageId: int64
    | SendVideoReply of chatId: int64 * video: InputFile * replyToMessageId: int64
    | SendPhotoReply of chatId: int64 * photo: InputFile * replyToMessageId: int64
    | SendVoiceReply of chatId: int64 * voice: InputFile * replyToMessageId: int64
    | SendAudioReply of chatId: int64 * audio: InputFile * replyToMessageId: int64

let private log = Logger.get "responseTelegramActor"

let channel = Channel.CreateUnbounded<TelegramSendingMessage>()
let channelWriter = channel.Writer
let channelReader = channel.Reader

let channelWorker() = task {
    let mutable cfg = ValueNone
    let api request =
        match cfg with
        | ValueSome botConfig ->
            request
            |> Funogram.Api.api botConfig
            |> Async.RunSynchronously
            |> fun res ->
                try
                    log.LogDebug("Result: {res}", Json.serialize res) with
                | ex ->
                    log.LogError("Got response: {ex}", ex)
        | _ -> ()

    while true do
        let! message = channel.Reader.ReadAsync()
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
            | ValueSome cfg ->
                let messageId =
                    Api.sendMessage chatId text
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                do! Task.Delay(ms)

                match messageId with
                | Some messageId ->
                    do! channelWriter.WriteAsync(TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | None ->
                    ()
            | _ -> ()
        | SendMarkupMessageAndDeleteAfter(chatId, text, mode, ms) ->
            match cfg with
            | ValueSome cfg ->
                let messageId =
                    Api.sendTextMarkup chatId text mode
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                do! Task.Delay(ms)

                match messageId with
                | Some messageId ->
                    do! channelWriter.WriteAsync(TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | _ -> ()
            | _ -> ()
        | SendMessageReplyAndDeleteAfter(chatId, text, ms) ->
            match cfg with
            | ValueSome cfg ->
                let messageId =
                    Api.sendMessage chatId text
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                do! Task.Delay(ms)
                match messageId with
                | Some messageId ->
                    do! channelWriter.WriteAsync(TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | _ -> ()
            | _ -> ()
        | SendMarkupMessageReplyAndDeleteAfter(chatId, text, mode, replyToMessageId, ms) ->
            match cfg with
            | ValueSome cfg ->
                let messageId =
                    Api.sendTextMarkupReply chatId text replyToMessageId mode
                    |> Funogram.Api.api cfg
                    |> Async.RunSynchronously
                    |> Result.toOption
                    |> Option.map (fun m -> m.MessageId)
                do! Task.Delay(ms)
                match messageId with
                | Some messageId ->
                    do! channelWriter.WriteAsync(TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | _ -> ()
            | _ -> ()
        | SetConfig botConfig ->
            &cfg <-? botConfig
        | SendAnimationReply (chatId, animation, replyToMessageId) ->
            Api.sendAnimationReply chatId animation replyToMessageId
            |> api
        | SendVideoReply (chatId, video, replyToMessageId) ->
            Api.sendVideoReply chatId video replyToMessageId
            |> api
        | SendPhotoReply (chatId, animation, replyToMessageId) ->
            Api.sendPhotoReply chatId animation replyToMessageId
            |> api
        | SendVoiceReply(chatId, inputFile, replyToMessageId) ->
            Api.sendVoiceReply chatId inputFile replyToMessageId
            |> api
        | SendAudioReply(chatId, inputFile, replyToMessageId) ->
            Api.sendAudioReply chatId inputFile replyToMessageId
            |> api
}

let mutable thread : Thread = null

let runChannel() =
    match Option.ofObj thread with
    | None ->
        let worker = channelWorker().GetAwaiter().GetResult

        let thread' = Thread(worker)
        thread'.Start()
        thread <- thread'
    | _ ->
        ()

module [<RequireQualifiedAccess>] TgApi =
    /// Send message to chat
    let sendMessage chatId text =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendMessage (chatId, text))

    /// Send message to chat and delete after some milliseconds
    let sendMessageAndDeleteAfter chatId text ms =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendMessageAndDeleteAfter (chatId, text, ms))

    /// Send message as reply to chat
    let sendMessageReply chatId text replyToMessageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.MessageReply (chatId, text, replyToMessageId))

    /// Send message as reply to chat with parse mode (Markdown or Html)
    let sendMarkupMessageReply chatId text replyToMessageId parseMode =
        channel.Writer.WriteAsync(TelegramSendingMessage.MarkupMessageReply (chatId, text, replyToMessageId, parseMode))

    /// Send message to chat with parse mode (Markdown or Html) and delete after some milliseconds
    let sendMarkupMessageAndDeleteAfter chatId text parseMode ms =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendMarkupMessageAndDeleteAfter (chatId, text, parseMode, ms))

    /// Send message reply to chat with parse mode (Markdown or Html) and delete after some milliseconds
    let sendMarkupMessageReplyAndDeleteAfter chatId text parseMode replyToMessageId ms =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendMarkupMessageReplyAndDeleteAfter (chatId, text, parseMode, replyToMessageId, ms))

    /// Delete message in chat
    let deleteMessage chatId messageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.DeleteMessage (chatId, messageId))

    /// Send animation as reply
    let sendAnimationReply chatId animation replyToMessageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendAnimationReply (chatId, animation, replyToMessageId))

    /// Send video as reply
    let sendVideoReply chatId video replyToMessageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendVideoReply (chatId, video, replyToMessageId))

    /// Send photo as reply
    let sendPhotoReply chatId photo replyToMessageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendPhotoReply (chatId, photo, replyToMessageId))

    /// Send photo as reply
    let sendVoiceReply chatId voice replyToMessageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendVoiceReply (chatId, voice, replyToMessageId))

    /// Send audio as reply
    let sendAudioReply chatId photo replyToMessageId =
        channel.Writer.WriteAsync(TelegramSendingMessage.SendAudioReply (chatId, photo, replyToMessageId))
