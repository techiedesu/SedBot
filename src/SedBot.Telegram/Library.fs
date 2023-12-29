module rec SedBot.Telegram.Bot

open System
open System.Net
open System.Net.Http
open System.Net.Sockets
open System.Text
open System.Text.Json
open System.Threading.Tasks
open SedBot.Common
open SedBot.Telegram.Types
open SedBot.Telegram.Types.CoreTypes
open SedBot.Telegram.Types.Generated

let private getUrl (config: BotConfig) methodName =
    let botToken = $"{config.ApiEndpointUrl |> string}{config.Token}"

    if config.IsTest then
        $"{botToken}/test/{methodName}"
    else
        $"{botToken}/{methodName}"

let private log = Utilities.Logger.get ^ nameof makeRequestAsync

let makeRequestAsync<'a> (config: BotConfig) (request: IRequestBase<'a>) = task {
    let client = config.Client
    let url = getUrl config request.MethodName

    let hasData = request.MethodName.StartsWith("get", StringComparison.InvariantCultureIgnoreCase) |> not

    let! result =
        // if request.MethodName.StartsWith("send") then
            // let form = new MultipartFormDataContent()
            // let stream =
            //     match request with
            //     | :? IRequestBase<Message> as v when v.MethodName = "sendVoice" ->
            //         let x = Unsafe.As<SendVoice> v
            //         x.Voice
            //     | _ -> ()

            // form.Add()

            // client.PostAsync(url, form)
        if hasData then
            let serialized = JsonSerializer.Serialize(request, request.Type, options = ReqJsonSerializerContext.CreateDefaultOptions())
            let content = new StringContent(serialized, Encoding.UTF8, "application/json")
            client.PostAsync(url, content)
        else
            let url =
                let ts = request.ToString()
                if ts.StartsWith("?") then
                    let url = url + ts
                    url
                else
                    url

            client.GetAsync(url)

    if result.StatusCode = HttpStatusCode.OK then
        let! stream = result.Content.ReadAsStreamAsync()

        let result =
            JsonSerializer.Deserialize<ApiResponse<'a>>(
                stream,
                options = ReqJsonSerializerContext.CreateDefaultOptions()
            )

        return  result.Result.Value |> Ok
    else
        return
            Error
                { Description = "HTTP_ERROR"
                  ErrorCode = int result.StatusCode }
}

let api config (request: IRequestBase<'a>) = makeRequestAsync config request

let private runBot config (me: User) (updateArrived: UpdateContext -> _) updatesArrived =
    let bot data = api config data

    let processUpdates updates =
        if updates |> Seq.isEmpty |> not then
            updates
            |> Seq.iter (fun f -> updateArrived { Update = f; Config = config; Me = me })

            updatesArrived |> Option.iter (fun x -> x updates)

    let rec loop offset = task {
        do! Task.Delay(3000)
        try
            let! updatesResult =
                ReqS.GetUpdates.Make(offset, ?limit = config.Limit, ?timeout = config.Timeout)
                |> bot

            match updatesResult with
            | Ok updates when updates |> Seq.isEmpty |> not ->
                let offset = updates |> Seq.map _.UpdateId |> Seq.max |> (fun x -> x + 1L)
                processUpdates updates
                return! loop offset // send new offset
            | Error e ->
                config.OnError(e.AsException() :> Exception)

                // add delay in case of HTTP error
                // for example: the server may be "busy"
                if e.Description = "HTTP_ERROR" then
                    do! Task.Delay 1000

                return! loop offset
            | _ -> return! loop offset
        with
        | :? HttpRequestException as e ->
            // in case of HTTP error we should not increment offset
            config.OnError e
            do! Async.Sleep 1000
            return! loop offset

        | :? AggregateException as e when
            e.InnerExceptions <> null
            && e.InnerExceptions
               |> Seq.exists (fun x -> (x :? HttpRequestException) || (x :? SocketException))
            ->
            config.OnError e
            do! Async.Sleep 1000
            return! loop offset

        | ex ->
            config.OnError ex
            // in case of "general" error we should increment offset to skip problematic update
            return! loop (offset + 1L)

        return! loop offset
    }

    loop (config.Offset |> Option.defaultValue 0L)

let startLoop (config: BotConfig) updateArrived updatesArrived = task {
    let! me = ApiS.getMe |> api config

    return!
        me
        |> function
            | Error error -> failwith error.Description
            | Ok me -> runBot config me updateArrived updatesArrived
}
