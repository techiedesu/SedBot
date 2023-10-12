module [<RequireQualifiedAccess>] SedBot.Actors

open System.Threading
open System.Threading.Tasks
open Funogram.Telegram
open Funogram.Telegram.Types
open Microsoft.FSharp.Core
open SedBot.Common
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging

let private log = Logger.get "responseTelegramActor"

let channelWriter = TgApi.channel.Writer
let channelReader = TgApi.channel.Reader

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
            Api.deleteMessage chatId messageId
            |> api
        | TgApi.SendMessageAndDeleteAfter(chatId, text, ms) ->
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
                    do! channelWriter.WriteAsync(TgApi.TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | None ->
                    ()
            | _ -> ()
        | TgApi.SendMarkupMessageAndDeleteAfter(chatId, text, mode, ms) ->
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
                    do! channelWriter.WriteAsync(TgApi.TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | _ -> ()
            | _ -> ()
        | TgApi.SendMessageReplyAndDeleteAfter(chatId, text, ms) ->
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
                    do! channelWriter.WriteAsync(TgApi.TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | _ -> ()
            | _ -> ()
        | TgApi.SendMarkupMessageReplyAndDeleteAfter(chatId, text, mode, replyToMessageId, ms) ->
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
                    do! channelWriter.WriteAsync(TgApi.TelegramSendingMessage.DeleteMessage(chatId, messageId))
                | _ -> ()
            | _ -> ()
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
