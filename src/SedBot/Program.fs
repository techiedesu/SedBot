open System
open System.Net
open System.Net.Http
open System.Threading
open SedBot
open SedBot.Commands
open SedBot.Common.TypeExtensions
open SedBot.Common.Utilities
open Microsoft.Extensions.Logging

open SedBot.Json
open SedBot.Telegram.BotApi
open SedBot.Telegram.BotApi.Types
open SedBot.Telegram.BotApi.Types.CoreTypes

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
            let _aa =
                Func<HttpRequestMessage, Security.Cryptography.X509Certificates.X509Certificate2, Security.Cryptography.X509Certificates.X509Chain, Net.Security.SslPolicyErrors, bool>(
                    fun _ _ _ _ -> true
                )

            let handler = new HttpClientHandler()
            handler.ClientCertificateOptions <- ClientCertificateOption.Manual
            handler.ServerCertificateCustomValidationCallback <- _aa // for debug
            handler.Proxy <- WebProxy(Uri("http://127.0.0.1:8888"))
            let client = new HttpClient(handler)
            client.Timeout <- TimeSpan.FromMinutes 1

            let config = {
                BotConfig.Empty with
                    Token = token
                    Client = client
                    OnError = fun ex -> logger.LogError("Got Funogram exception: {ex}", ex)
            }

            %ChannelProcessors.channelWriter.TryWrite(TgApi.TelegramSendingMessage.SetConfig config)
            ChannelProcessors.runChannel()

            let help = CommandParser.processInlineHelp ()
            let botCommands : BotCommand list = help |> List.map (fun ici -> { Command = ici.Command; Description = ici.Description })
            Api.sendNewCommands (Array.ofList botCommands) |> Core.api config |> Task.runSynchronously

            Api.deleteWebhookBase () |> Core.api config |> Task.runSynchronously
            let botInfoResult = Api.getMe |> Core.api config |> Task.getResult

            let botUsername =
                match botInfoResult with
                | Error err ->
                    raise ^ Exception($"Can't get username: {err}")
                | Ok res ->
                    Option.get res.Username
            logger.LogDebug("Config {config}", SedJsonSerializer.serialize {config with Token = $"<redacted:{botUsername}>"})

            Api.startLoop config (UpdatesHandler.updateArrived botUsername) None |> Task.runSynchronously
        with
        | ex when ex.Message.Contains("Unauthorized") ->
            logger.LogCritical("Wrong token? Error: {error}", ex)
            Environment.Exit(-1)
        | ex ->
            logger.LogError("Something goes wrong: {error}", ex)
            Thread.Sleep(5000)

    0
