﻿namespace SedBot.Telegram.BotApi.Types.CoreTypes

open System
open System.Net.Http
open System.Text.Json.Serialization
open SedBot.Telegram.BotApi.Types

type ApiResponse<'a> = {
    Ok: bool
    Result: 'a option
    Description: string option
    ErrorCode: int option
}

type IBotRequest =
    [<JsonIgnore>]
    abstract MethodName: string

    [<JsonIgnore>]
    abstract Type: Type

type IRequestBase<'a> =
    inherit IBotRequest

and BotConfig = {
    IsTest: bool
    Token: string
    Offset: int64 option
    Limit: int64 option
    Timeout: int64 option
    AllowedUpdates: string seq option
    OnError: Exception -> unit
    ApiEndpointUrl: Uri
    Client: HttpClient
}
with
    static member Empty = {
        IsTest = false
        Token = ""
        Offset = Some 0L
        Limit = Some 100
        Timeout = Some 60000
        AllowedUpdates = None
        Client = new HttpClient()
        ApiEndpointUrl = Uri("https://api.telegram.org/bot")
        OnError = printfn "%A"
    }

/// Bot Api Response Error
and ApiResponseError ={
    Description: string
    ErrorCode: int
}
with member x.AsException() = ApiResponseException(x)

and UpdateContext = {
    Update: Update
    Config: BotConfig
    Me: User
}

and ApiResponseException(error: ApiResponseError) =
    inherit Exception()

    member _.Error = error
    override _.ToString() = $"ApiResponseException: {error.Description}. Code: {error.ErrorCode}"
