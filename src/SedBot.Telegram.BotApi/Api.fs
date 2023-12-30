[<RequireQualifiedAccess>]
module rec SedBot.Telegram.BotApi.Api

open SedBot.Telegram.BotApi.Types
open SedBot.Telegram.BotApi.Types.CoreTypes


let deleteWebhookBase () =
    Req.GetWebhookInfo()

let getMe = Req.GetMe()

type GetFile = {
    FileId: string
}
with
    static member Make(fileId: string) = {
        FileId = fileId
    }

    interface IRequestBase<File> with
        member _.MethodName = "getFile"
        member this.Type = typeof<GetFile>

let getFile fileId = {
    Req.GetFile.FileId = fileId
}

let sendMessageReply chatId text replyToMessageId =
    Req.SendMessage.Make(ChatId.Int chatId, text, replyToMessageId = replyToMessageId)

let sendMessage chatId text =
    Req.SendMessage.Make(ChatId.Int chatId, text)

let private deleteMessageBase chatId messageId =
  ({ ChatId = chatId; MessageId = messageId }: Req.DeleteMessage)

let deleteMessage chatId messageId =
    deleteMessageBase (ChatId.Int chatId) messageId

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
let tryGetFileAsStream (ctx: UpdateContext) fileId = task {
    let! file = Api.getFile fileId |> Core.api ctx.Config
    match file with
    | Ok { FilePath = Some path } ->
        try
            let! res = ctx.Config.Client.GetStreamAsync($"https://api.telegram.org/file/bot{ctx.Config.Token}/{path}")
            return res |> ValueSome
        with
        | _ -> return ValueNone
    | _ -> return ValueNone
}

let sendNewCommands commands =
    Req.SetMyCommands.Make(commands)

let startLoop (config: BotConfig) updateArrived updatesArrived = task {
    let! me = Api.getMe |> Core.api config

    return!
        me
        |> function
            | Error error -> failwith error.Description
            | Ok me -> Core.runBot config me updateArrived updatesArrived
}

