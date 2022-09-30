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
let asyncIgnore (a: _ Async) = a |> Async.logAndIgnore log

let updateArrived (ctx: UpdateContext) =
    task {
        let! botUsername = me ctx
        match CommandParser.parse ctx.Update.Message botUsername with
        | SedCommand (chatId, replyMsgId, srcMsgId, exp, text) ->
            let! res = Commands.sed text exp
            match res with
            | Some res ->
                do! Api.deleteMessage chatId srcMsgId |> api ctx.Config |> Async.Ignore
                do! Api.sendMessageReply chatId res replyMsgId |> api ctx.Config |> Async.Ignore
            | _ ->
                ()
        | JqCommand (chatId, msgId, data, expression) ->
            let! res = expression |> Commands.jq data
            match res with
            | Some res ->
                let res = $"```\n{res}\n```"
                do! Api.sendTextMarkupReply chatId res msgId ParseMode.Markdown |> api ctx.Config |> asyncIgnore
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
                            |> asyncIgnore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
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
                            |> asyncIgnore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
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
                            |> asyncIgnore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
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
                            |> asyncIgnore
                    | Video ->
                        do! Api.sendVideoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
                    | _ ->
                        do! Api.sendPhotoReply chatId ani msgId
                            |> api ctx.Config
                            |> asyncIgnore
                | _ -> ()
            | _ -> ()
            ()
        | ClownCommand(chatId, count) ->
            do! Api.sendMessage chatId (System.String.Concat(Enumerable.Repeat("🤡", count)))
                |> api ctx.Config
                |> asyncIgnore
        | InfoCommand(chatId, msgId, reply) ->
            // do! Api.deleteMessage chatId msgId |> api ctx.Config |> asyncIgnore
            let res =  $"```\n{reply |> Json.serializeNicely}\n```"
            let! x = Api.sendTextMarkupReply chatId res reply.MessageId ParseMode.Markdown |> api ctx.Config
            match x with
            | Ok { MessageId = msgId } ->
                do! Task.Delay(30000)
                do! Api.deleteMessage chatId msgId |> api ctx.Config |> asyncIgnore
            | _ -> ()
        | CreateKick(chatId, victimUserId, msgId) ->
            do! Api.deleteMessage chatId msgId
                |> api ctx.Config
                |> asyncIgnore
            let! apiResponse =
                Api.sendTextMarkup chatId $"`/banid {victimUserId}`" ParseMode.Markdown
                |> api ctx.Config
            match apiResponse with
            | Ok { MessageId = msgId } ->
                do! Task.Delay(10000)
                do! Api.deleteMessage chatId msgId
                    |> api ctx.Config
                    |> asyncIgnore
            | _ -> ()
        | Nope ->
            ()
    } |> ignore

[<EntryPoint>]
let main args =
    let logger = Logger.get "EntryPoint"
    if args.Length = 0 then
        logger.LogCritical("Usage: {execName} yourtelegramtoken", AppDomain.CurrentDomain.FriendlyName)
        Environment.Exit(-1)
    let token = args[0]

    ProcessingChannels.start()
    while true do
        try
            task {
                let config = { Config.defaultConfig with Token = token }
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
