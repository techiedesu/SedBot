module SedBot.ChannelProcessors

open System.Threading
open System.Threading.Tasks
open Funogram.Telegram
open Funogram.Telegram.Types
open Microsoft.FSharp.Core
open SedBot.Common
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging

type internal Marker = interface end
let private log = Logger.get ^ typeof<Marker>.DeclaringType.Name

let channelWriter = TgApi.channel.Writer
let channelReader = TgApi.channel.Reader

let private channelWorker() =
    let mutable cfg = ValueNone
    let api request =
        request
        |> Funogram.Api.api cfg.Value
        |> Async.RunSynchronously
        |> fun res ->
            try
                log.LogDebug("Result: {res}", Json.serialize res)
            with ex ->
                log.LogError("Got response: {ex}", ex)

    let apiMapResponse a =
        Funogram.Api.api cfg.Value >> Async.StartAsTask >> TaskOption.ofResult >> TaskOption.map a

    let tryWriteToChannel v a =
        match a with
        | Some a ->
            channelWriter.WriteAsync(v a)
        | _ -> ValueTask.CompletedTask

    let rec loop () = task {
        let! message = channelReader.ReadAsync()
        match message with
        | TgApi.SendMessage(chatId, text) ->
            Api.sendMessage chatId text
            |> api
        | TgApi.MessageReply (chatId, text, replyToMessageId) ->
            Api.sendMessageReply chatId text replyToMessageId
            |> api
        | TgApi.MarkupMessageReply(chatId, text, replyToMessageId, parseMode) ->
            Req.SendMessage.Make(chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)
            |> api
        | TgApi.DeleteMessage(chatId, messageId) ->
            Api.deleteMessage chatId messageId |> api

        | TgApi.SendMessageAndDeleteAfter(chatId, text, ms) ->
            let! messageId = Api.sendMessage chatId text |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SendMarkupMessageAndDeleteAfter(chatId, text, mode, ms) ->
            let! messageId = Api.sendTextMarkup chatId text mode |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SendMessageReplyAndDeleteAfter(chatId, text, ms) ->
            let! messageId = Api.sendMessage chatId text |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SendMarkupMessageReplyAndDeleteAfter(chatId, text, mode, replyToMessageId, ms) ->
            let! messageId = Api.sendTextMarkupReply chatId text replyToMessageId mode |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SetConfig botConfig ->
            &cfg <-? botConfig

        | TgApi.SendAnimationReply (chatId, animation, replyToMessageId) ->
            Api.sendAnimationReply chatId animation replyToMessageId
            |> api

        | TgApi.SendVideoReply (chatId, video, replyToMessageId) ->
            Api.sendVideoReply chatId video replyToMessageId
            |> api

        | TgApi.SendPhotoReply (chatId, animation, replyToMessageId) ->
            Api.sendPhotoReply chatId animation replyToMessageId
            |> api

        | TgApi.SendVoiceReply(chatId, inputFile, replyToMessageId) ->
            Api.sendVoiceReply chatId inputFile replyToMessageId
            |> api

        | TgApi.SendAudioReply(chatId, inputFile, replyToMessageId) ->
            Api.sendAudioReply chatId inputFile replyToMessageId
            |> api

        return! loop()
    }

    loop()

let mutable thread : Thread = null

let runChannel() =
    let worker = channelWorker().GetAwaiter().GetResult

    let thread' = Thread(worker)
    thread'.Start()
    thread <- thread'
