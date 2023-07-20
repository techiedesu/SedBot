open System
open System.IO
open System.Linq

open System.Threading
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot
open SedBot.Actors
open SedBot.ChatCommands
open SedBot.Utilities
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
        let synthName = Utilities.Path.getSynthName extension
        let ms = new MemoryStream(data)
        InputFile.File(synthName, ms) |> ValueSome
    | _ -> ValueNone

let sendFileAsReply data fileType chatId msgId =
    let inputFile = createInputFile fileType data

    match inputFile with
    | ValueSome inputFile -> replyAsFileType fileType chatId inputFile msgId
    | _ -> ()

let updateArrivedInternal ctx (message: Message) =
    task {
        let! botUsername = me ctx

        match CommandParser.processMessage message botUsername with
        | Sed ((chatId, replyMsgId), srcMsgId, exp, text) ->
            let! res = Commands.sed text exp

            match res with
            | ValueSome res ->
                TgApi.deleteMessage chatId srcMsgId
                TgApi.sendMessageReply chatId res replyMsgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "Sed command failed. This message will be deleted after 35 s." 35000

        | Jq ((chatId, msgId), data, expression) ->
            let! res = expression |> Commands.jq data

            match res with
            | ValueSome res -> TgApi.sendMarkupMessageReply chatId $"```\n{res}\n```" msgId ParseMode.Markdown
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "Jq command failed. This message will be deleted after 35 s." 35000

        | Reverse ((chatId, msgId), (fileId, fileType)) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.reverse srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "Reverse command failed. This message will be deleted after 35 s." 35000

        | VerticalFlip ((chatId, msgId), (fileId, fileType)) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.vFlip srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "VerticalFlip command failed. This message will be deleted after 35 s." 35000

        | HorizontalFlip ((chatId, msgId), (fileId, fileType)) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.hFlip srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "HorizontalFlip command failed. This message will be deleted after 35 s." 35000

        | Distortion ((chatId, msgId), (fileId, fileType)) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.distort srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "Distortion command failed. This message will be deleted after 35 s." 35000

        | ClockwiseRotation ((chatId, msgId), (fileId, fileType)) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.clock srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "ClockwiseRotation command failed. This message will be deleted after 35 s." 35000

        | CounterClockwiseRotation ((chatId, msgId), (fileId, fileType)) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx

            match file with
            | ValueSome srcStream ->
                let! res = Commands.cclock srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ ->
                TgApi.sendMessageAndDeleteAfter chatId "CounterClockwiseRotation command failed. This message will be deleted after 35 s." 35000

        | Clown (chatId, count) -> TgApi.sendMessage chatId (System.String.Concat(Enumerable.Repeat("🤡", count)))

        | RawMessageInfo(_, replyTo) ->
            TgApi.sendMarkupMessageReplyAndDeleteAfter replyTo.Chat.Id $"`{(Json.serializeWithIndentationsIgnoreEmptyFields replyTo)}`" ParseMode.Markdown replyTo.MessageId 30000

        | UserId((chatId, msgId), victimUserId) ->
            TgApi.deleteMessage chatId msgId
            TgApi.sendMarkupMessageAndDeleteAfter chatId $"`{victimUserId}`" ParseMode.Markdown 5000

        | Nope -> ()
    }
    |> ignore

let updateArrived (ctx: UpdateContext) =
    ctx.Update.Message |> Option.iter (updateArrivedInternal ctx)

open Akka.FSharp
open Funogram.Types

[<EntryPoint>]
let main args =
    let logger = Logger.get "EntryPoint"

    let token =
        match List.ofArray args with
        | token :: _ ->
            token
        | [] ->
            logger.LogCritical("Usage: {execName} yourtelegramtoken", AppDomain.CurrentDomain.FriendlyName)
            Environment.Exit(-1)
            null

    let system =
        Configuration.defaultConfig ()
        |> System.create "system"

    let tgResponseActor =
        responseTelegramActor
        |> spawn system "tgResponseActor"

    TgApi.actor <- tgResponseActor

    ProcessingChannels.start ()

    while true do
        try
            task {
                let config = { Config.defaultConfig with Token = token }

                tgResponseActor
                <! SendTelegramResponseMail.SetConfig config

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
