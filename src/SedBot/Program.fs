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
            res |> Option.iter (fun res -> TgApi.sendMarkupMessageReply chatId $"```\n{res}\n```" msgId ParseMode.Markdown)
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
