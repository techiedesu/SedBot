open System
open System.Net.Http
open System.Threading
open SedBot
open SedBot.ChatCommands.Types
open SedBot.Commands
open SedBot.Common.TypeExtensions
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging

open SedBot.Telegram.Types

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
            let aa =
                Func<HttpRequestMessage, Security.Cryptography.X509Certificates.X509Certificate2, Security.Cryptography.X509Certificates.X509Chain, Net.Security.SslPolicyErrors, bool>(
                    fun _ _ _ _ -> true
                )

            let handler = new HttpClientHandler()
            handler.ClientCertificateOptions <- ClientCertificateOption.Manual
            handler.ServerCertificateCustomValidationCallback <- aa
            let client = new HttpClient(handler)

            let defaultConfig  : SedBot.Telegram.Types.CoreTypes.BotConfig = {
                IsTest = false
                Token = ""
                Offset = Some 0L
                Limit = Some 100
                Timeout = Some 60000
                AllowedUpdates = None
                Client = client
                ApiEndpointUrl = Uri("https://api.telegram.org/bot")
                WebHook = None
                OnError = (fun e -> printfn "%A" e)
            }

            let config : SedBot.Telegram.Types.CoreTypes.BotConfig = {
                defaultConfig with
                    Token = token
                    OnError = fun ex -> logger.LogError("Got Funogram exception: {ex}", ex)
            }
            logger.LogDebug("Config: {config}", config)

            ChannelProcessors.channelWriter.TryWrite(TgApi.TelegramSendingMessage.SetConfig config) |> ignore
            ChannelProcessors.runChannel()

            let help = CommandParser.processInlineHelp ()
            let botCommands : BotCommand list = help |> List.map (fun ici -> { Command = ici.Command; Description = ici.Description })
            let _ = ApiS.sendNewCommands (Array.ofList botCommands) |> SedBot.Telegram.Bot.api config |> Task.runSynchronously

            let _ = ApiS.deleteWebhookBase () |> SedBot.Telegram.Bot.api config |> Task.runSynchronously
            let botInfoResult = ApiS.getMe |> SedBot.Telegram.Bot.api config |> Task.getResult

            let botUsername =
                match botInfoResult with
                | Error err ->
                    raise ^ Exception($"Can't get username: {err}")
                | Ok res ->
                    Option.get res.Username

            SedBot.Telegram.Bot.startLoop config (UpdatesHandler.updateArrived botUsername) None |> Task.runSynchronously
        with
        | ex when ex.Message.Contains("Unauthorized") ->
            logger.LogCritical("Wrong token? Error: {error}", ex)
            Environment.Exit(-1)
        | ex ->
            logger.LogError("Something goes wrong: {error}", ex)
        Thread.Sleep(5000)

    0
