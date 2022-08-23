open System
open System.IO
open System.Net.Http

open System.Threading
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot

module Api =
    let sendAnimationReply chatId animation replyToMessageId = Req.SendAnimation.Make(ChatId.Int chatId, animation, replyToMessageId = replyToMessageId)
    let sendTextMarkupReply chatId text replyToMessageId parseMode = Req.SendMessage.Make(ChatId.Int chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)

open SedBot.ChatCommands.ActivePatterns

let updateArrived (ctx: UpdateContext) =
    task {
        match ctx.Update.Message with
        | SedCommand (chatId, replyMsgId, exp, text) ->
            let! res = Commands.sed text exp
            match res with
            | Some res ->
                do! Api.sendMessageReply chatId res replyMsgId |> api ctx.Config |> Async.Ignore
            | _ ->
                ()
        | JqCommand (chatId, msgId, data, exp) ->
            let! res = Commands.jq data exp
            match res with
            | Some res ->
                let res = $"```\n{res}\n```"
                do! Api.sendTextMarkupReply chatId res msgId ParseMode.Markdown |> api ctx.Config |> Async.Ignore
            | _ ->
                ()
        | ReverseCommand (chatId, msgId, fileId) ->
            let! file = Api.getFile fileId |> api ctx.Config
            match file with
            | Ok { FilePath = Some filePath } ->
                use hc = new HttpClient()
                let! srcStream = hc.GetStreamAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{filePath}")
                match! Commands.reverse srcStream with
                | ValueSome resStream ->
                    let synthName = Utilities.Path.getSynthName ".mp4"
                    let ms = new MemoryStream(resStream)
                    ms.Position <- 0
                    let ani = InputFile.File (synthName, ms)
                    do! Api.sendAnimationReply chatId ani msgId |> api ctx.Config |> Async.Ignore
                | _ -> ()
                ()
            | _ -> ()
            ()
        | DistortCommand (chatId, msgId, fileId) ->
            let! file = Api.getFile fileId |> api ctx.Config
            match file with
            | Ok { FilePath = Some filePath } ->
                use hc = new HttpClient()
                let! srcStream = hc.GetStreamAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{filePath}")
                match! Commands.distort srcStream with
                | ValueSome resStream ->
                    let synthName = Guid.NewGuid().ToString().Replace("-", "") + ".mp4"
                    let ms = new MemoryStream(resStream)
                    ms.Position <- 0
                    let ani = InputFile.File (synthName, ms)
                    do! Api.sendAnimationReply chatId ani msgId |> api ctx.Config |> Async.Ignore
                | _ -> ()
                ()
            | _ -> ()
            ()
        | _ -> ()
    } |> fun x -> x.ConfigureAwait(false).GetAwaiter().GetResult()

[<EntryPoint>]
let main args =
    if args.Length = 0 then
        printfn "Usage: %s yourtelegramtoken" AppDomain.CurrentDomain.FriendlyName
        Environment.Exit(-1)
    let token = args[0]

    while true do
        try
            task {
                let config = { Config.defaultConfig with Token = token }
                return! startBot config updateArrived None
            } |> fun x -> x.ConfigureAwait(false).GetAwaiter().GetResult()
        with
        | e ->
            printfn "Something goes wrong: %s" (e.ToString())
            Thread.Sleep(5000)
    0
