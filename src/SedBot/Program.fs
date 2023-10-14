open System
open System.IO

open System.Threading
open System.Threading.Tasks
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot
open SedBot.Common.TypeExtensions
open SedBot.ChatCommands
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging

let mutable myUserName: string = null

/// Extract telegram bot username and cache
let me ctx =
    task {
        if myUserName = null then
            let! gotMe = Api.getMe |> api ctx.Config

            match gotMe with
            | Ok res ->
                myUserName <- $"@{res.Username.Value}"
                return myUserName
            | _ -> return raise <| Exception("Can't get username")
        else
            return myUserName
    }

let log = Logger.get "updateArrived"

let private replyAsFileType fileType chatId inputFile msgId =
    match fileType with
    | Gif
    | Sticker ->
        TgApi.sendAnimationReply chatId inputFile msgId
    | Video ->
        TgApi.sendVideoReply chatId inputFile msgId
    | Picture ->
        TgApi.sendPhotoReply chatId inputFile msgId
    | Voice ->
        TgApi.sendVoiceReply chatId inputFile msgId
    | Audio ->
        TgApi.sendAudioReply chatId inputFile msgId

let createInputFile fileType (data: byte [] voption) : InputFile voption =
    match data with
    | ValueSome data ->
        let extension = extension fileType
        let synthName = Path.getSynthName extension
        let ms = new MemoryStream(data)
        InputFile.File(synthName, ms) |> ValueSome
    | _ -> ValueNone

let sendFileAsReply data fileType chatId msgId =
    let inputFile = createInputFile fileType data

    match inputFile with
    | ValueSome inputFile ->
        replyAsFileType fileType chatId inputFile msgId
    | _ ->
        ValueTask.CompletedTask

let updateArrivedInternal ctx (message: Message) =
    task {
        let! botUsername = me ctx

        let res = CommandParser.processMessage message botUsername

        match res with
        | Sed {
                TelegramOmniMessageId = chatId, replyMsgId
                Expression = exp
                Text = text
         } ->
            let! res = Commands.sed text exp

            match res with
            | ValueSome res ->
                // do! TgApi.deleteMessage chatId srcMsgId
                do! TgApi.sendMessageReply chatId res replyMsgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "Sed command failed. This message will be deleted after 35 s." 35000

        | Jq {
            TelegramOmniMessageId = chatId, msgId
            Expression = expression
            Text = data
         } ->
            let! res = expression |> Commands.jq data

            match res with
            | ValueSome res ->
                do! TgApi.sendMarkupMessageReply chatId $"```\n{res}\n```" msgId ParseMode.Markdown
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "Jq command failed. This message will be deleted after 35 s." 35000

        | Reverse {
            TelegramOmniMessageId = chatId, msgId
            File = fileId, fileType
         } ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.reverse srcStream fileType
                do! sendFileAsReply res fileType chatId msgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "Reverse command failed. This message will be deleted after 35 s." 35000

        | VerticalFlip {
            TelegramOmniMessageId = chatId, msgId
            File = fileId, fileType
         } ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.vFlip srcStream fileType
                do! sendFileAsReply res fileType chatId msgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "VerticalFlip command failed. This message will be deleted after 35 s." 35000

        | HorizontalFlip {
            TelegramOmniMessageId = chatId, msgId
            File = fileId, fileType
         } ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.hFlip srcStream fileType
                do! sendFileAsReply res fileType chatId msgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "HorizontalFlip command failed. This message will be deleted after 35 s." 35000

        | Distortion {
            TelegramOmniMessageId = chatId, msgId
            File = fileId, fileType
         } ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.distort srcStream fileType
                do! sendFileAsReply res fileType chatId msgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "Distortion command failed. This message will be deleted after 35 s." 35000

        | ClockwiseRotation {
            TelegramOmniMessageId = chatId, msgId
            File = fileId, fileType
         } ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.clock srcStream fileType
                do! sendFileAsReply res fileType chatId msgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "ClockwiseRotation command failed. This message will be deleted after 35 s." 35000

        | CounterClockwiseRotation {
            TelegramOmniMessageId = chatId, msgId
            File = fileId, fileType
         } ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.cclock srcStream fileType
                do! sendFileAsReply res fileType chatId msgId
            | _ ->
                do! TgApi.sendMessageAndDeleteAfter chatId "CounterClockwiseRotation command failed. This message will be deleted after 35 s." 35000

        | Clown {
            ChatId = chatId
            Count = count
         } ->
            do! TgApi.sendMessage chatId (seq {
                while true do
                    "🤡"
            } |> Seq.take count |> String.concat "")

        | RawMessageInfo {
            ReplyTo = {
                MessageId = messageId
                Chat = {
                    Id = replyChatId
                }
            } as replyTo
         } ->
            do! TgApi.sendMarkupMessageReplyAndDeleteAfter replyChatId $"`{(Json.serialize replyTo)}`" ParseMode.Markdown messageId 30000

        | Nope -> ()
    }
    |> ignore

let updateArrived (ctx: UpdateContext) =
    ctx.Update.Message |> Option.iter (updateArrivedInternal ctx)

open Funogram.Types

[<EntryPoint>]
let rec entryPoint args =
    let logger = Logger.get (nameof entryPoint)

    let token =
        match List.ofArray args with
        | [] ->
            logger.LogCritical("Usage: {execName} yourtelegramtoken", AppDomain.CurrentDomain.FriendlyName)
            Environment.Exit(-1)
            null
        | token :: _ ->
            token

    ProcessingChannels.start ()

    while true do
        try
            task {
                let config = { Config.defaultConfig with Token = token }

                OldActors.channelWriter.TryWrite(TgApi.TelegramSendingMessage.SetConfig config) |> ignore
                OldActors.runChannel()

                let! _ = Api.deleteWebhookBase () |> api config
                return! startBot config updateArrived None
            }
            |> fun x -> x.ConfigureAwait(false).GetAwaiter().GetResult()
        with
        | ex when ex.Message.Contains("Unauthorized") ->
            logger.LogCritical("Wrong token? Error: {error}", ex)
            Environment.Exit(-1)
        | ex ->
            logger.LogError("Something goes wrong: {error}", ex)
            Thread.Sleep(5000)

    0
