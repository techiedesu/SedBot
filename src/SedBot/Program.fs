open System
open System.IO
open System.Linq
open System.Net.Http

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

let mutable myUserName : string = null

/// Extract telegram bot username and cache
let me ctx = task {
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

let private replyAsFileType fileType chatId ani msgId =
    match fileType with
    | Gif
    | Sticker ->
        TgApi.sendAnimationReply chatId ani msgId
    | Video ->
        TgApi.sendVideoReply chatId ani msgId
    | Picture ->
        TgApi.sendPhotoReply chatId ani msgId

let createInputFile fileType (data: byte[] voption) : InputFile voption =
    match data with
    | ValueSome data ->
        let extension = extension fileType
        let synthName = Utilities.Path.getSynthName extension
        let ms = new MemoryStream(data)
        InputFile.File (synthName, ms) |> ValueSome
    | _ -> ValueNone

let sendFileAsReply data fileType chatId msgId =
    let inputFile = createInputFile fileType data
    match inputFile with
    | ValueSome inputFile ->
        replyAsFileType fileType chatId inputFile msgId
    | _ -> ()

let updateArrived (ctx: UpdateContext) =
    task {
        let! botUsername = me ctx
        match CommandParser.parse ctx.Update.Message botUsername with
        | SedCommand (chatId, replyMsgId, srcMsgId, exp, text) ->
            let! res = Commands.sed text exp
            match res with
            | ValueSome res ->
                TgApi.deleteMessage chatId srcMsgId
                TgApi.sendMessageReply chatId res replyMsgId
            | _ -> ()

        | JqCommand (chatId, msgId, data, expression) ->
            let! res = expression |> Commands.jq data
            match res with
            | ValueSome res ->
                TgApi.sendMarkupMessageReply chatId $"```\n{res}\n```" msgId ParseMode.Markdown
            | _ -> ()

        | ReverseCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                let! res = Commands.reverse srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ -> ()

        | VflipCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                let! res = Commands.vFlip srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ -> ()

        | HflipCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                let! res = Commands.hFlip srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ -> ()

        | DistortCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                let! res = Commands.distort srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ -> ()

        | ClockCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                let! res = Commands.clock srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ -> ()

        | CClockCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                let! res = Commands.cclock srcStream fileType
                sendFileAsReply res fileType chatId msgId
            | _ -> ()

        | ClownCommand(chatId, count) ->
            TgApi.sendMessage chatId (System.String.Concat(Enumerable.Repeat("🤡", count)))

        | InfoCommand(chatId, msgId, reply) ->
            TgApi.deleteMessage chatId msgId
            let res =  $"```\n{reply |> Json.serializeNicely}\n```"
            TgApi.sendMarkupMessageReplyAndDeleteAfter chatId res ParseMode.Markdown reply.MessageId 5000

        | CreateKick(chatId, victimUserId, msgId) ->
            TgApi.deleteMessage chatId msgId
            TgApi.sendMarkupMessageAndDeleteAfter chatId $"`/banid {victimUserId}`" ParseMode.Markdown 5000

        | Nope ->
            ()
    } |> ignore

open Akka.FSharp
open Funogram.Types

[<EntryPoint>]
let main args =
    let logger = Logger.get "EntryPoint"
    if args.Length = 0 then
        logger.LogCritical("Usage: {execName} yourtelegramtoken", AppDomain.CurrentDomain.FriendlyName)
        Environment.Exit(-1)

    let token = args[0]
    let system = Configuration.defaultConfig() |> System.create "system"
    let tgResponseActor = responseTelegramActor |> spawn system "tgResponseActor"
    TgApi.actor <- tgResponseActor

    ProcessingChannels.start()
    while true do
        try
            task {
                let config = { Config.defaultConfig with Token = token }
                tgResponseActor <! SendTelegramResponseMail.SetConfig config
                let! _ = Api.deleteWebhookBase () |> api config
                return! startBot config updateArrived None
            } |> fun x -> x.ConfigureAwait(false).GetAwaiter().GetResult()
        with
         | ex when ex.Message.Contains("Unauthorized") ->
            logger.LogCritical("Wrong token? Error: {error}", ex)
            Environment.Exit(-1)
         | ex ->
            logger.LogError("Something goes wrong: {error}", ex)
            Thread.Sleep(5000)
    0
