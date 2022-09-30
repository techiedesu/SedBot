module SedBot.Api

open System.IO
open System.Net.Http
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types

let mutable hc = new HttpClient()

let sendAnimationReply chatId animation replyToMessageId =
    Req.SendAnimation.Make(ChatId.Int chatId, animation, replyToMessageId = replyToMessageId)

let sendVideoReply chatId videoFile replyToMessageId =
    Req.SendVideo.Make(ChatId.Int chatId, videoFile, replyToMessageId = replyToMessageId)

let sendTextMarkupReply chatId text replyToMessageId parseMode =
    Req.SendMessage.Make(ChatId.Int chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)

let sendTextMarkup chatId text parseMode =
    Req.SendMessage.Make(ChatId.Int chatId, text, parseMode = parseMode)

let sendPhotoReply chatId photo replyToMessageId =
    Req.SendPhoto.Make(ChatId.Int chatId, photo, replyToMessageId = replyToMessageId)

/// Try to get file stream by telegram FileId
let tryGetFileAsStream (ctx: UpdateContext) fileId = task {
    let! file = Api.getFile fileId |> api ctx.Config
    match file with
    | Ok { FilePath = Some path } ->
        try
            let! res = hc.GetStreamAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{path}")
            return res |> ValueSome
        with
        | _ -> return ValueNone
    | _ -> return ValueNone
}

let tryGetFileBytes (ctx: UpdateContext) fileId = task {
    let! stream = fileId |> tryGetFileAsStream ctx
    match stream with
    | ValueSome stream ->
        use ms = new MemoryStream()
        do! stream.CopyToAsync(ms)
        return ms.ToArray() |> ValueSome
    | _ ->
        return ValueNone
}
