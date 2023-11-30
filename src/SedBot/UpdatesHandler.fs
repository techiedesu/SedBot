module SedBot.UpdatesHandler

open System.IO

open System.Threading.Tasks
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open SedBot
open SedBot.Commands
open SedBot.ChatCommands.Types
open SedBot.Common.TypeExtensions
open SedBot.Common.Utilities

let private log = Logger.get "UpdatesHandler"

let private replyAsFileType fileType chatId inputFile msgId =
    match fileType with
    | Gif
    | Sticker ->
        TgApi.sendAnimationReply chatId inputFile msgId
    | Video ->
        TgApi.sendVideoReply chatId inputFile msgId
    | Picture ->
        TgApi.sendPhotoReply chatId inputFile msgId
    | Voice ->
        TgApi.sendVoiceReply chatId inputFile msgId
    | Audio ->
        TgApi.sendAudioReply chatId inputFile msgId

let private createInputFile fileType (data: byte [] voption) : InputFile voption =
    match data with
    | ValueSome data ->
        let extension = extension fileType
        let synthName = Path.getSynthName extension
        let ms = new MemoryStream(data)
        InputFile.File(synthName, ms) |> ValueSome
    | _ -> ValueNone

let private sendFileAsReply data fileType chatId msgId =
    let inputFile = createInputFile fileType data

    match inputFile with
    | ValueSome inputFile ->
        replyAsFileType fileType chatId inputFile msgId
    | _ ->
        ValueTask.CompletedTask

let private updateArrivedInternal botUsername ctx (message: Message) = task {
    let res = CommandParser.processMessage message botUsername

    match res with
    | Sed {
            TelegramOmniMessageId = chatId, replyMsgId
            Expression = exp
            Text = text
     } ->
        let! res = Handlers.sed text exp

        match res with
        | ValueSome res ->
            do! TgApi.sendMessageReply chatId res replyMsgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "Sed command failed. This message will be deleted after 35 s." 35000

    | Jq {
        TelegramOmniMessageId = chatId, msgId
        Expression = expression
        Text = data
     } ->
        let! res = expression |> Handlers.jq data

        match res with
        | ValueSome res ->
            do! TgApi.sendMarkupMessageReply chatId $"```json\n{res}\n```" msgId ParseMode.Markdown
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "Jq command failed. This message will be deleted after 35 s." 35000

    | Reverse {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! file = fileId |> Api.tryGetFileAsStream ctx

        match file with
        | ValueSome srcStream ->
            let! res = Handlers.reverse srcStream fileType
            do! sendFileAsReply res fileType chatId msgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "Reverse command failed. This message will be deleted after 35 s." 35000

    | VerticalFlip {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! file = fileId |> Api.tryGetFileAsStream ctx

        match file with
        | ValueSome srcStream ->
            let! res = Handlers.vFlip srcStream fileType
            do! sendFileAsReply res fileType chatId msgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "VerticalFlip command failed. This message will be deleted after 35 s." 35000

    | HorizontalFlip {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! file = fileId |> Api.tryGetFileAsStream ctx

        match file with
        | ValueSome srcStream ->
            let! res = Handlers.hFlip srcStream fileType
            do! sendFileAsReply res fileType chatId msgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "HorizontalFlip command failed. This message will be deleted after 35 s." 35000

    | Distortion {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! file = fileId |> Api.tryGetFileAsStream ctx

        match file with
        | ValueSome srcStream ->
            let! res = Handlers.distort srcStream fileType
            do! sendFileAsReply res fileType chatId msgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "Distortion command failed. This message will be deleted after 35 s." 35000

    | ClockwiseRotation {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! file = fileId |> Api.tryGetFileAsStream ctx

        match file with
        | ValueSome srcStream ->
            let! res = Handlers.clock srcStream fileType
            do! sendFileAsReply res fileType chatId msgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "ClockwiseRotation command failed. This message will be deleted after 35 s." 35000

    | CounterClockwiseRotation {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! file = fileId |> Api.tryGetFileAsStream ctx

        match file with
        | ValueSome srcStream ->
            let! res = Handlers.cclock srcStream fileType
            do! sendFileAsReply res fileType chatId msgId
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter chatId "CounterClockwiseRotation command failed. This message will be deleted after 35 s." 35000

    | Clown {
        ChatId = chatId
        Count = count
     } ->
        do! TgApi.sendMessage chatId (seq {
            while true do
                "🤡"
        } |> Seq.take count |> String.concat "")

    | RawMessageInfo {
        ReplyTo = {
            MessageId = messageId
            Chat = {
                Id = replyChatId
            }
        } as replyTo
     } ->
        do! TgApi.sendMarkupMessageReplyAndDeleteAfter replyChatId $"`{(Json.serialize replyTo)}`" ParseMode.Markdown messageId 30000

    | Nope -> ()
}

let updateArrived botUsername (ctx: UpdateContext) =
    ctx.Update.Message |> Option.iterIgnore (updateArrivedInternal botUsername ctx)
