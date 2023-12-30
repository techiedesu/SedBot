module SedBot.UpdatesHandler

open System.IO

open SedBot
open SedBot.Commands
open SedBot.ChatCommands.Types
open SedBot.Common.TypeExtensions
open SedBot.Common.Utilities
open SedBot.Telegram.BotApi
open SedBot.Telegram.BotApi.Types
open SedBot.Telegram.BotApi.Types.CoreTypes

type internal Marker = interface end
let log = Logger.get ^ typeof<Marker>.DeclaringType.Name

let private replyAsFileType fileType omniMsgId inputFile =
    match fileType with
    | Gif
    | Sticker ->
        TgApi.sendAnimationReply omniMsgId inputFile
    | Video ->
        TgApi.sendVideoReply omniMsgId inputFile
    | Picture ->
        TgApi.sendPhotoReply omniMsgId inputFile
    | Voice ->
        TgApi.sendVoiceReply omniMsgId inputFile
    | Audio ->
        TgApi.sendAudioReply omniMsgId inputFile

let private createInputFile fileType (data: byte[]) : InputFile =
    let extension = extension fileType
    let synthName = Path.getSynthName extension
    let ms = new MemoryStream(data)
    InputFile.File(synthName, ms)

let private sendFileAsReply data fileType omniMsgId =
    let inputFile = createInputFile fileType data
    replyAsFileType fileType omniMsgId inputFile

let private updateArrivedInternal botUsername (ctx: UpdateContext) (message: Message) = task {
    let res = CommandParser.processMessage message botUsername
    let placeholder = sprintf "%s command failed. This message will be deleted after 35 s.\n\n You can send file to @tdesu for investigation."

    match res with
    | Sed {
            TelegramOmniMessageId = omniMsgId
            Expression = exp
            Text = text
     } ->
        let! res = Handlers.sed text exp

        match res with
        | ValueSome res ->
            do! TgApi.sendMessageReply omniMsgId res
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "Sed") 35000

    | Jq {
        TelegramOmniMessageId = omniMsgId
        Expression = expression
        Text = data
     } ->
        let! res = expression |> Handlers.jq data

        match res with
        | ValueSome res ->
            do! TgApi.sendMarkupMessageReply omniMsgId $"```json\n{res}\n```" ParseMode.Markdown
        | _ ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "Jq") 35000

    | Reverse { TelegramOmniMessageId = omniMsgId; File = fileId, fileType } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.reverse fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType omniMsgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "Reverse") 35000

    | VerticalFlip {
        TelegramOmniMessageId = omniMsgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.vFlip fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType omniMsgId
        |ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "VerticalFlip") 35000

    | HorizontalFlip {
        TelegramOmniMessageId = omniMsgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.hFlip fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType omniMsgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "HorizontalFlip") 35000

    | Distortion {
        TelegramOmniMessageId = omniMsgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.distort fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType omniMsgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "Distortion") 35000

    | ClockwiseRotation {
        TelegramOmniMessageId = omniMsgId; File = fileId, fileType } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.clock fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType omniMsgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "ClockwiseRotation") 35000

    | CounterClockwiseRotation {
        TelegramOmniMessageId = omniMsgId
        File = fileId, fileType
     } ->
        let! res = fileId |> Api.tryGetFileAsStream ctx |> TaskVOption.taskBind (Handlers.cclock fileType)

        match res with
        | ValueSome res ->
            do! sendFileAsReply res fileType omniMsgId
        | ValueNone ->
            do! TgApi.sendMessageAndDeleteAfter (fst omniMsgId) (placeholder "CounterClockwiseRotation") 35000

    | Clown {
        ChatId = chatId
        Count = count
     } ->
        do! TgApi.sendMessage chatId (seq { while true do "🤡" } |> Seq.take count |> String.concat "")

    | RawMessageInfo { ReplyTo = { MessageId = msgId; Chat = { Id = chatId } } as replyTo } ->
        do! TgApi.sendMarkupMessageReplyAndDeleteAfter (chatId, msgId) $"`{Json.serialize replyTo}`" ParseMode.Markdown 30000

    | Nope -> ()
}

let updateArrived botUsername (ctx: UpdateContext) =
    ctx.Update.Message |> Option.iterIgnore (updateArrivedInternal botUsername ctx)
