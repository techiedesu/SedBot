module rec SedBot.Telegram.BotApi.Core

open System
open System.Net
open System.Net.Http
open System.Net.Sockets
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open SedBot.Common
open SedBot.Json
open SedBot.Telegram
open SedBot.Telegram.BotApi.Types
open SedBot.Telegram.BotApi.Types.CoreTypes

let private log = Utilities.Logger.get ^ nameof makeRequest

let private getUrlAux (config: BotConfig) methodName =
    let botToken = $"{config.ApiEndpointUrl |> string}{config.Token}"

    if config.IsTest then
        $"{botToken}/test/{methodName}"
    else
        $"{botToken}/{methodName}"

/// Code functionality for Telegram Bot api. Makes requests
let makeRequest<'a> (config: BotConfig) (request: IRequestBase<'a>) = task {
    let client = config.Client
    let url = getUrlAux config request.MethodName

    let! result =
        let dataContent = new MultipartFormDataContent()
        let kindOfJson, _streams = RequestBuilder.builderDynamic (request.GetType()) dataContent request
        log.LogDebug("Sent command {method} -> {request}", request.MethodName, kindOfJson)

        if Seq.isEmpty dataContent then
            client.GetAsync(url)
        else
            client.PostAsync(url, dataContent)

    if result.StatusCode = HttpStatusCode.OK then
        let! str = result.Content.ReadAsStringAsync()
        let result = SedJsonDeserializer.deserializeStatic<ApiResponse<'a>> str
        return result.Result.Value |> Ok
    else
        log.LogDebug("Request fail {method} -> {response}", request.MethodName, result.Content.ReadAsStringAsync() |> Task.getResult)
        return Error {
            Description = "HTTP_ERROR"
            ErrorCode = int result.StatusCode
        }
}

let api (config: BotConfig) (request: IRequestBase<'a>) = makeRequest config request


/// Starts bot
let runBot config (me: User) (updateArrived: UpdateContext -> unit) updatesArrived =
    let bot data = api config data

    let log = Utilities.Logger.get ^ nameof runBot

    let processUpdates updates =
        if updates |> Seq.isEmpty |> not then
            updates |> Seq.iter (fun f -> updateArrived { Update = f; Config = config; Me = me })

            updatesArrived |> Option.iter (fun x -> x updates)

    let rec loop offset = task {
        try
            let! updatesResult = bot <| Req.GetUpdates.Make(offset, ?limit = config.Limit, ?timeout = config.Timeout)

            match updatesResult with
            | Ok [||] ->
                return! loop offset
            | Ok updates ->
                let offset = updates |> Seq.map _.UpdateId |> Seq.max |> (fun x -> x + 1L) // TODO: Save to persist storage?
                processUpdates updates

                return! loop offset
            | Error e ->
                config.OnError(e.AsException())

                if e.Description = "HTTP_ERROR" then
                    log.LogWarning("Got HTTP_ERROR. Delaying...")
                    do! Task.Delay 1000

                return! loop offset
        with
        | :? HttpRequestException as e ->
            config.OnError e
            do! Task.Delay 1000
            return! loop offset

        | :? AggregateException as e
            when e.InnerExceptions |> NTSeq.exists (fun x -> (x :? HttpRequestException) || (x :? SocketException)) ->
            config.OnError e
            do! Task.Delay 1000
            return! loop offset

        | ex ->
            config.OnError ex
            // in case of "general" error we should increment offset to skip problematic update
            return! loop (offset + 1L)

        return! loop offset
    }

    loop (config.Offset)
