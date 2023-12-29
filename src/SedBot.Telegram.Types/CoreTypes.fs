namespace SedBot.Telegram.Types.CoreTypes

open System
open System.Net
open System.Net.Http
open SedBot.Telegram.Types

type UpdateContext =
  { Update: Update
    Config: BotConfig
    Me: User }

and BotConfig =
  { IsTest: bool
    Token: string
    Offset: int64 option
    Limit: int64 option
    Timeout: int64 option
    AllowedUpdates: string seq option
    OnError: Exception -> unit
    ApiEndpointUrl: Uri
    Client: HttpClient
    WebHook: BotWebHook option }
and BotWebHook = { Listener: HttpListener; ValidateRequest: HttpListenerRequest -> bool }
/// Bot Api Response Error
and ApiResponseError =
  { Description: string
    ErrorCode: int }
  member x.AsException() =
    ApiResponseException(x)
and ApiResponseException(error: ApiResponseError) =
  inherit Exception()

  member _.Error = error
  override _.ToString() =
    sprintf "ApiResponseException: %s. Code: %A" error.Description error.ErrorCode
