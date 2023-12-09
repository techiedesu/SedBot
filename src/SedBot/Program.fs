open System
open System.Threading
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot
open SedBot.ChatCommands.Types
open SedBot.Commands
open SedBot.Common.TypeExtensions
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging

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
            let config = {
                Config.defaultConfig with
                    Token = token
                    OnError = fun ex -> logger.LogError("Got Funogram exception: {ex}", ex)
            }
            ChannelProcessors.channelWriter.TryWrite(TgApi.TelegramSendingMessage.SetConfig config) |> ignore
            ChannelProcessors.runChannel()

            task {

                let help = CommandParser.processInlineHelp ()

                let botCommands : BotCommand list = help |> List.map (fun ici -> { Command = ici.Command; Description = ici.Description })
                let! _ = Api.sendNewCommands (Array.ofList botCommands) |> api config

                let! _ = Api.deleteWebhookBase () |> api config
                let! botInfoResult = Api.getMe |> api config

                let botUsername =
                    match botInfoResult with
                    | Error err ->
                        raise ^ Exception($"Can't get username: {err}")
                    | Ok res ->
                        Option.get res.Username

                return! startBot config (UpdatesHandler.updateArrived botUsername) None
            } |> Task.runSynchronously
        with
        | ex when ex.Message.Contains("Unauthorized") ->
            logger.LogCritical("Wrong token? Error: {error}", ex)
            Environment.Exit(-1)
        | ex ->
            logger.LogError("Something goes wrong: {error}", ex)
            Thread.Sleep(5000)

    0
