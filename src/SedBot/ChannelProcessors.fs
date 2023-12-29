module SedBot.ChannelProcessors

open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core
open SedBot.Common
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging
open SedBot.Telegram.Types

type internal Marker = interface end
let private log = Logger.get ^ typeof<Marker>.DeclaringType.Name

let channelWriter = TgApi.channel.Writer
let channelReader = TgApi.channel.Reader

let private channelWorker() =
    let mutable cfg = ValueNone
    let api (request: IRequestBase<_>) = task {
        log.LogDebug("Request: {request}", request)
        let! result = request |> SedBot.Telegram.Bot.api cfg.Value

        try
            log.LogDebug("Result: {res}", result)
        with ex ->
            log.LogError("Got response: {ex}", ex)
    }

    let apiMapResponse a =
        SedBot.Telegram.Bot.api cfg.Value >> TaskOption.ofResult >> TaskOption.map a

    let tryWriteToChannel v a =
        match a with
        | Some a ->
            channelWriter.WriteAsync(v a)
        | _ -> ValueTask.CompletedTask

    let rec loop () = task {
        let! message = channelReader.ReadAsync()
        match message with
        | TgApi.SendMessage(chatId, text) ->
            do! ApiS.sendMessage chatId text |> api
        | TgApi.MessageReply (chatId, text, replyToMessageId) ->
            do! ApiS.sendMessageReply chatId text replyToMessageId |> api
        | TgApi.MarkupMessageReply(chatId, text, replyToMessageId, parseMode) ->
            do! ReqS.SendMessage.Make(chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode) |> api
        | TgApi.DeleteMessage(chatId, messageId) ->
            do! ApiS.deleteMessage chatId messageId |> api

        | TgApi.SendMessageAndDeleteAfter(chatId, text, ms) ->
            let! messageId = ApiS.sendMessage chatId text |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SendMarkupMessageAndDeleteAfter(chatId, text, mode, ms) ->
            let! messageId = ApiS.sendTextMarkup chatId text mode |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SendMessageReplyAndDeleteAfter(chatId, text, ms) ->
            let! messageId = ApiS.sendMessage chatId text |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SendMarkupMessageReplyAndDeleteAfter(chatId, text, mode, replyToMessageId, ms) ->
            let! messageId = ApiS.sendTextMarkupReply chatId text replyToMessageId mode |> apiMapResponse _.MessageId
            do! Task.Delay(ms)
            do! tryWriteToChannel (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId)) messageId

        | TgApi.SetConfig botConfig ->
            &cfg <-? botConfig

        | TgApi.SendAnimationReply (chatId, animation, replyToMessageId) ->
            do! ApiS.sendAnimationReply chatId animation replyToMessageId |> api

        | TgApi.SendVideoReply (chatId, video, replyToMessageId) ->
            do! ApiS.sendVideoReply chatId video replyToMessageId |> api

        | TgApi.SendPhotoReply (chatId, animation, replyToMessageId) ->
            do! ApiS.sendPhotoReply chatId animation replyToMessageId |> api

        | TgApi.SendVoiceReply(chatId, inputFile, replyToMessageId) ->
            do! ApiS.sendVoiceReply chatId inputFile replyToMessageId |> api

        | TgApi.SendAudioReply(chatId, inputFile, replyToMessageId) ->
            do! ApiS.sendAudioReply chatId inputFile replyToMessageId |> api

        return! loop()
    }

    loop()

let mutable thread : Thread = null

let runChannel() =
    let worker = channelWorker().GetAwaiter().GetResult

    let thread' = Thread(worker)
    thread'.Start()
    thread <- thread'
