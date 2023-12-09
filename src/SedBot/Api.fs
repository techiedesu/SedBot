module SedBot.Api

open System.Net.Http
open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open Funogram.Telegram.Types

let private hc = new HttpClient()

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

let sendVoiceReply chatId voice replyToMessageId =
    Req.SendVoice.Make(ChatId.Int chatId, voice, replyToMessageId = replyToMessageId)

let sendAudioReply chatId audio replyToMessageId =
    Req.SendAudio.Make(ChatId.Int chatId, audio, replyToMessageId = replyToMessageId)

/// Try to get file stream by telegram FileId
let tryGetFileAsStream ctx fileId = task {
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

/// Try to get file bytes by telegram FileId
let tryGetFileAsBytes ctx fileId = task {
    let! file = Api.getFile fileId |> api ctx.Config
    match file with
    | Ok { FilePath = Some path } ->
        try
            let! res = hc.GetByteArrayAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{path}")
            return res |> ValueSome
        with
        | _ ->
            return ValueNone
    | _ ->
        return ValueNone
}

let sendNewCommands commands =
    Req.SetMyCommands.Make(commands)
