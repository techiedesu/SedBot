open System
open System.Threading
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot
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
            task {
                let config = { Config.defaultConfig with Token = token }

                ChannelProcessors.channelWriter.TryWrite(TgApi.TelegramSendingMessage.SetConfig config) |> ignore
                ChannelProcessors.runChannel()

                let! _ = Api.deleteWebhookBase () |> api config
                let! botInfoResult = Api.getMe |> api config
                let me =
                    match botInfoResult with
                    | Error err ->
                        raise ^ Exception($"Can't get username: {err}")
                    | Ok res ->
                        Option.get res.Username

                return! startBot config (UpdatesHandler.updateArrived me) None
            } |> fun x -> x.ConfigureAwait(false).GetAwaiter().GetResult()
        with
        | ex when ex.Message.Contains("Unauthorized") ->
            logger.LogCritical("Wrong token? Error: {error}", ex)
            Environment.Exit(-1)
        | ex ->
            logger.LogError("Something goes wrong: {error}", ex)
            Thread.Sleep(5000)

    0
