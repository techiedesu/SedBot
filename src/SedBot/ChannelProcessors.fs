module SedBot.ChannelProcessors

open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core
open SedBot.Common
open SedBot.Common.Utilities
open SedBot.Telegram.BotApi
open SedBot.Telegram.BotApi.Types

type internal Marker = interface end
let private log = Logger.get ^ typeof<Marker>.DeclaringType.Name

let channelWriter = TgApi.channel.Writer
let channelReader = TgApi.channel.Reader

let private channelWorker () =
    let mutable cfg = ValueNone
    let api request = request |> Core.api cfg.Value :> Task

    let apiMapResponse a =
        Core.api cfg.Value >> TaskOption.ofResult >> TaskOption.map a

    let tryWriteToChannel v a =
        match a with
        | Some a -> channelWriter.WriteAsync(v a)
        | _ -> ValueTask.CompletedTask

    let rec loop () =
        task {
            let! message = channelReader.ReadAsync()

            match message with
            | TgApi.SendMessage(chatId, text) -> do! Api.sendMessage chatId text |> api

            | TgApi.MessageReply(chatId, text, replyToMessageId) ->
                do! Api.sendMessageReply chatId text replyToMessageId |> api

            | TgApi.MarkupMessageReply(chatId, text, replyToMessageId, parseMode) ->
                do!
                    Req.SendMessage.Make(chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)
                    |> api

            | TgApi.DeleteMessage(chatId, messageId) -> do! Api.deleteMessage chatId messageId |> api

            | TgApi.SendMessageAndDeleteAfter(chatId, text, ms) ->
                let! messageId = Api.sendMessage chatId text |> apiMapResponse _.MessageId

                %Task
                    .Delay(ms)
                    .ContinueWith(fun _ ->
                        tryWriteToChannel
                            (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId))
                            messageId)

            | TgApi.SendMarkupMessageAndDeleteAfter(chatId, text, mode, ms) ->
                let! messageId = Api.sendTextMarkup chatId text mode |> apiMapResponse _.MessageId

                %Task
                    .Delay(ms)
                    .ContinueWith(fun _ ->
                        tryWriteToChannel
                            (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId))
                            messageId)

            | TgApi.SendMessageReplyAndDeleteAfter(chatId, text, ms) ->
                let! messageId = Api.sendMessage chatId text |> apiMapResponse _.MessageId

                %Task
                    .Delay(ms)
                    .ContinueWith(fun _ ->
                        tryWriteToChannel
                            (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId))
                            messageId)

            | TgApi.SendMarkupMessageReplyAndDeleteAfter(chatId, text, mode, replyToMessageId, ms) ->
                let! messageId =
                    Api.sendTextMarkupReply chatId text replyToMessageId mode
                    |> apiMapResponse _.MessageId

                %Task
                    .Delay(ms)
                    .ContinueWith(fun _ ->
                        tryWriteToChannel
                            (fun msgId -> TgApi.TelegramSendingMessage.DeleteMessage(chatId, msgId))
                            messageId)

            | TgApi.SetConfig botConfig -> &cfg <-? botConfig

            | TgApi.SendAnimationReply(chatId, animation, replyToMessageId) ->
                do! Api.sendAnimationReply chatId animation replyToMessageId |> api

            | TgApi.SendVideoReply(chatId, video, replyToMessageId) ->
                do! Api.sendVideoReply chatId video replyToMessageId |> api

            | TgApi.SendPhotoReply(chatId, animation, replyToMessageId) ->
                do! Api.sendPhotoReply chatId animation replyToMessageId |> api

            | TgApi.SendVoiceReply(chatId, inputFile, replyToMessageId) ->
                do! Api.sendVoiceReply chatId inputFile replyToMessageId |> api

            | TgApi.SendAudioReply(chatId, inputFile, replyToMessageId) ->
                do! Api.sendAudioReply chatId inputFile replyToMessageId |> api

            return! loop ()
        }

    loop ()

let mutable thread: Thread = null

let runChannel () =
    let worker = channelWorker().GetAwaiter().GetResult

    let thread' = Thread(worker)
    thread'.Start()
    thread <- thread'
