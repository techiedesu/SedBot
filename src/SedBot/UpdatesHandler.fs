module SedBot.UpdatesHandler

open System.IO

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

let private createInputFile fileType (data: byte[]) : InputFile =
    let extension = extension fileType
    let synthName = Path.getSynthName extension
    let ms = new MemoryStream(data)
    InputFile.File(synthName, ms)

let private sendFileAsReply data fileType chatId msgId =
    let inputFile = createInputFile fileType data
    replyAsFileType fileType chatId inputFile msgId

let private updateArrivedInternal botUsername ctx (message: Message) = task {
    let res = CommandParser.processMessage message botUsername

    let placeholder = sprintf "%s command failed. This message will be deleted after 35 s.\n\n You can send file to @tdesu for investigation."

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
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "Sed") 35000

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
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "Jq") 35000

    | Reverse { TelegramOmniMessageId = chatId, msgId; File = fileId, fileType } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.reverse fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType chatId msgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "Reverse") 35000

    | VerticalFlip {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.vFlip fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType chatId msgId
        |ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "VerticalFlip") 35000

    | HorizontalFlip {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.hFlip fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType chatId msgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "HorizontalFlip") 35000

    | Distortion {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.distort fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType chatId msgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "Distortion") 35000

    | ClockwiseRotation {
        TelegramOmniMessageId = chatId, msgId; File = fileId, fileType } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.clock fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType chatId msgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "ClockwiseRotation") 35000

    | CounterClockwiseRotation {
        TelegramOmniMessageId = chatId, msgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.cclock fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType chatId msgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter chatId (placeholder "CounterClockwiseRotation") 35000

    | Clown {
        ChatId = chatId
        Count = count
     } ->
        do! TgApi.sendMessage chatId (seq {
            while true do
                "🤡"
        } |> Seq.take count |> String.concat "")

    | RawMessageInfo { ReplyTo = { MessageId = msgId; Chat = { Id = chatId } } as replyTo } ->
        do! TgApi.sendMarkupMessageReplyAndDeleteAfter chatId $"`{(Json.serialize replyTo)}`" ParseMode.Markdown msgId 30000

    | Nope -> ()
}

let updateArrived botUsername (ctx: UpdateContext) =
    ctx.Update.Message |> Option.iterIgnore (updateArrivedInternal botUsername ctx)
