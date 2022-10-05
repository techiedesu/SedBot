open System
open System.IO
open System.Linq
open System.Net.Http

open System.Threading
open System.Threading.Tasks
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot
open SedBot.Actors
open SedBot.ChatCommands
open SedBot.Utilities
open Microsoft.Extensions.Logging

module Api =
    let mutable hc = new HttpClient()

    let sendAnimationReply chatId animation replyToMessageId =
        Req.SendAnimation.Make(ChatId.Int chatId, animation, replyToMessageId = replyToMessageId)

    let sendVideoReply chatId videoFile replyToMessageId =
        Req.SendVideo.Make(ChatId.Int chatId, videoFile, replyToMessageId = replyToMessageId)

    let sendTextMarkupReply chatId text replyToMessageId parseMode =
        Req.SendMessage.Make(ChatId.Int chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)

    let sendTextMarkup chatId text parseMode =
        Req.SendMessage.Make(ChatId.Int chatId, text, parseMode = parseMode)

    let sendPhotoReply chatId photo replyToMessageId =
        Req.SendPhoto.Make(ChatId.Int chatId, photo, replyToMessageId = replyToMessageId)

    /// Try to get file stream by telegram FileId
    let tryGetFileAsStream ctx fileId = task {
        let! file = Api.getFile fileId |> api ctx.Config
        match file with
        | Ok { FilePath = Some path } ->
            try
                let! res = hc.GetStreamAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{path}")
                return res |> ValueSome
            with
            | _ -> return ValueNone
        | _ -> return ValueNone
    }

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

let updateArrived (ctx: UpdateContext) =
    task {
        let! botUsername = me ctx
        match CommandParser.parse ctx.Update.Message botUsername with
        | SedCommand (chatId, replyMsgId, srcMsgId, exp, text) ->
            let! res = Commands.sed text exp
            match res with
            | Some res ->
                TgApi.deleteMessage chatId srcMsgId
                TgApi.sendMessageReply chatId res replyMsgId
            | _ ->
                ()
        | JqCommand (chatId, msgId, data, expression) ->
            let! res = expression |> Commands.jq data
            match res with
            | Some res ->
                let res = $"```\n{res}\n```"
                TgApi.sendMarkupMessageReply chatId res msgId ParseMode.Markdown
            | _ ->
                ()
        | ReverseCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                match! Commands.reverse srcStream fileType with
                | ValueSome resStream ->
                    let extension = extension fileType
                    let synthName = Utilities.Path.getSynthName extension
                    let ms = new MemoryStream(resStream)
                    ms.Position <- 0
                    let ani = InputFile.File (synthName, ms)
                    match fileType with
                    | Gif | Sticker ->
                        do! Api.sendAnimationReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                | _ -> ()
            | ValueNone -> ()
        | VflipCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                match! Commands.vFlip srcStream fileType with
                | ValueSome resStream ->
                    let extension = extension fileType
                    let synthName = Utilities.Path.getSynthName extension
                    let ms = new MemoryStream(resStream)
                    ms.Position <- 0
                    let ani = InputFile.File (synthName, ms)
                    match fileType with
                    | Gif
                    | Sticker ->
                        do! Api.sendAnimationReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                | ValueNone -> ()
            | _ -> ()
        | HflipCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                match! Commands.hFlip srcStream fileType with
                | ValueSome resStream ->
                    let synthName =
                        extension fileType
                        |> Utilities.Path.getSynthName
                    let ms = new MemoryStream(resStream)
                    ms.Position <- 0
                    let ani = InputFile.File (synthName, ms)
                    match fileType with
                    | Gif
                    | Sticker ->
                        do! Api.sendAnimationReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                | _ -> ()
            | _ -> ()
        | DistortCommand (chatId, msgId, fileId, fileType) ->
            let! file = fileId |> Api.tryGetFileAsStream ctx
            match file with
            | ValueSome srcStream ->
                match! Commands.distort srcStream fileType with
                | ValueSome resStream ->
                    let extension = extension fileType
                    let synthName = Utilities.Path.getSynthName extension
                    let ms = new MemoryStream(resStream)
                    ms.Position <- 0
                    let ani = InputFile.File (synthName, ms)
                    match fileType with
                    | Gif | Sticker ->
                        do! Api.sendAnimationReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> Async.Ignore
                | _ -> ()
            | _ -> ()
            ()
        | ClownCommand(chatId, count) ->
            TgApi.sendMessage chatId (System.String.Concat(Enumerable.Repeat("🤡", count)))
        | InfoCommand(chatId, msgId, reply) ->
            do! Api.deleteMessage chatId msgId |> api ctx.Config |> Async.Ignore
            let res =  $"```\n{reply |> Json.serializeNicely}\n```"
            let! x = Api.sendTextMarkupReply chatId res reply.MessageId ParseMode.Markdown |> api ctx.Config
            match x with
            | Ok { MessageId = msgId } ->
                do! Task.Delay(5000)
                do! Api.deleteMessage chatId msgId |> api ctx.Config |> Async.Ignore
            | _ -> ()
        | CreateKick(chatId, victimUserId, msgId) ->
            TgApi.deleteMessage chatId msgId
            let! apiResponse =
                Api.sendTextMarkup chatId $"`/banid {victimUserId}`" ParseMode.Markdown
                |> api ctx.Config
            match apiResponse with
            | Ok { MessageId = msgId } ->
                do! Task.Delay(10000)
                do! Api.deleteMessage chatId msgId
                    |> api ctx.Config
                    |> Async.Ignore
            | _ -> ()
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
