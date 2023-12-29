module SedBot.ApiS

open SedBot.Telegram.Types

let sendAnimationReply chatId animation replyToMessageId =
    ReqS.SendAnimation.Make(ChatId.Int chatId, animation, replyToMessageId = replyToMessageId)

let sendVideoReply chatId videoFile replyToMessageId =
    ReqS.SendVideo.Make(ChatId.Int chatId, videoFile, replyToMessageId = replyToMessageId)

let sendTextMarkupReply chatId text replyToMessageId parseMode =
    ReqS.SendMessage.Make(ChatId.Int chatId, text, replyToMessageId = replyToMessageId, parseMode = parseMode)

let sendTextMarkup chatId text parseMode =
    ReqS.SendMessage.Make(ChatId.Int chatId, text, parseMode = parseMode)

let sendPhotoReply chatId photo replyToMessageId =
    ReqS.SendPhoto.Make(ChatId.Int chatId, photo, replyToMessageId = replyToMessageId)

let sendVoiceReply chatId voice replyToMessageId =
    ReqS.SendVoice.Make(ChatId.Int chatId, voice, replyToMessageId = replyToMessageId)

let sendAudioReply chatId audio replyToMessageId =
    ReqS.SendAudio.Make(ChatId.Int chatId, audio, replyToMessageId = replyToMessageId)

/// Try to get file stream by telegram FileId
let tryGetFileAsStream (ctx: SedBot.Telegram.Types.CoreTypes.UpdateContext) fileId = task {
    let! file = ApiS.getFile fileId |> SedBot.Telegram.Bot.api ctx.Config
    match file with
    | Ok { FilePath = Some path } ->
        try
            let res = ctx.Config.Client.GetStreamAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{path}").Result
            let res = res
            return res |> ValueSome
        with
        | _ -> return ValueNone
    | _ -> return ValueNone
}

let sendNewCommands commands =
    ReqS.SetMyCommands.Make(commands)
