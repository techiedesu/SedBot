[<RequireQualifiedAccess>]
module SedBot.TgApi

open System.Threading.Channels
open SedBot.Telegram.Types
open SedBot.Telegram.Types.CoreTypes

type TelegramSendingMessage =
    | SendMessage of chatId: int64 * text: string
    | SendMessageAndDeleteAfter of chatId: int64 * text: string * ms: int
    | SendMessageReplyAndDeleteAfter of chatId: int64 * text: string * ms: int
    | SendMarkupMessageAndDeleteAfter of chatId: int64 * text: string * parseMode: ParseMode * ms: int
    | SendMarkupMessageReplyAndDeleteAfter of chatId: int64 * text: string * parseMode: ParseMode * replyToMessageId: int64 * ms: int
    | MessageReply of chatId: int64 * text: string * replyToMessageId: int64
    | MarkupMessageReply of chatId: int64 * text: string * replyToMessageId: int64 * parseMode: ParseMode
    | DeleteMessage of chatId: int64 * messageId: int64
    | SetConfig of config: BotConfig
    | SendAnimationReply of chatId: int64 * animation: InputFile * replyToMessageId: int64
    | SendVideoReply of chatId: int64 * video: InputFile * replyToMessageId: int64
    | SendPhotoReply of chatId: int64 * photo: InputFile * replyToMessageId: int64
    | SendVoiceReply of chatId: int64 * voice: InputFile * replyToMessageId: int64
    | SendAudioReply of chatId: int64 * audio: InputFile * replyToMessageId: int64

let channel = Channel.CreateUnbounded<TelegramSendingMessage>()

/// Send message to chat
let sendMessage chatId text =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendMessage (chatId, text))

/// Send message to chat and delete after some milliseconds
let sendMessageAndDeleteAfter chatId text ms =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendMessageAndDeleteAfter (chatId, text, ms))

/// Send message as reply to chat
let sendMessageReply (chatId, replyToMessageId) text =
    channel.Writer.WriteAsync(TelegramSendingMessage.MessageReply (chatId, text, replyToMessageId))

/// Send message as reply to chat with parse mode (Markdown or Html)
let sendMarkupMessageReply (chatId, replyToMessageId) text parseMode =
    channel.Writer.WriteAsync(TelegramSendingMessage.MarkupMessageReply (chatId, text, replyToMessageId, parseMode))

/// Send message to chat with parse mode (Markdown or Html) and delete after some milliseconds
let sendMarkupMessageAndDeleteAfter chatId text parseMode ms =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendMarkupMessageAndDeleteAfter (chatId, text, parseMode, ms))

/// Send message reply to chat with parse mode (Markdown or Html) and delete after some milliseconds
let sendMarkupMessageReplyAndDeleteAfter (chatId, replyToMessageId) text parseMode ms =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendMarkupMessageReplyAndDeleteAfter (chatId, text, parseMode, replyToMessageId, ms))

/// Delete message in chat
let deleteMessage (chatId, messageId) =
    channel.Writer.WriteAsync(TelegramSendingMessage.DeleteMessage (chatId, messageId))

/// Send animation as reply
let sendAnimationReply (chatId, replyToMessageId) animation =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendAnimationReply (chatId, animation, replyToMessageId))

/// Send video as reply
let sendVideoReply (chatId, replyToMessageId) video =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendVideoReply (chatId, video, replyToMessageId))

/// Send photo as reply
let sendPhotoReply (chatId, replyToMessageId) photo =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendPhotoReply (chatId, photo, replyToMessageId))

/// Send photo as reply
let sendVoiceReply (chatId, replyToMessageId) voice =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendVoiceReply (chatId, voice, replyToMessageId))

/// Send audio as reply
let sendAudioReply (chatId, replyToMessageId) photo =
    channel.Writer.WriteAsync(TelegramSendingMessage.SendAudioReply (chatId, photo, replyToMessageId))
