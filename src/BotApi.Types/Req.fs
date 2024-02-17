[<RequireQualifiedAccess>]
module SedBot.Telegram.BotApi.Types.Req

open SedBot.Telegram.BotApi.Types.CoreTypes

type SendVideo = {
    ChatId: ChatId
    MessageThreadId: int64 option
    Video: InputFile
    Duration: int64 option
    Width: int64 option
    Height: int64 option
    Thumbnail: InputFile option
    Caption: string option
    ParseMode: ParseMode option
    CaptionEntities: MessageEntity[] option
    HasSpoiler: bool option
    SupportsStreaming: bool option
    DisableNotification: bool option
    ProtectContent: bool option
    ReplyToMessageId: int64 option
    AllowSendingWithoutReply: bool option
    ReplyMarkup: Markup option
}
with
    static member Make(chatId: ChatId,
                       video: InputFile,
                       ?replyToMessageId: int64,
                       ?protectContent: bool,
                       ?disableNotification: bool,
                       ?supportsStreaming: bool,
                       ?hasSpoiler: bool,
                       ?captionEntities: MessageEntity[],
                       ?parseMode: ParseMode,
                       ?caption: string,
                       ?thumbnail: InputFile,
                       ?height: int64,
                       ?width: int64,
                       ?duration: int64,
                       ?messageThreadId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) = {
          ChatId = chatId
          MessageThreadId = messageThreadId
          Video = video
          Duration = duration
          Width = width
          Height = height
          Thumbnail = thumbnail
          Caption = caption
          ParseMode = parseMode
          CaptionEntities = captionEntities
          HasSpoiler = hasSpoiler
          SupportsStreaming = supportsStreaming
          DisableNotification = disableNotification
          ProtectContent = protectContent
          ReplyToMessageId = replyToMessageId
          AllowSendingWithoutReply = allowSendingWithoutReply
          ReplyMarkup = replyMarkup
    }

    static member Make(chatId: int64,
                       video: InputFile,
                       ?replyToMessageId: int64,
                       ?protectContent: bool,
                       ?disableNotification: bool,
                       ?supportsStreaming: bool,
                       ?hasSpoiler: bool,
                       ?captionEntities: MessageEntity[],
                       ?parseMode: ParseMode,
                       ?caption: string,
                       ?thumbnail: InputFile,
                       ?height: int64,
                       ?width: int64,
                       ?duration: int64,
                       ?messageThreadId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendVideo.Make(ChatId.Int chatId,
                       video,
                       ?replyToMessageId = replyToMessageId,
                       ?protectContent = protectContent,
                       ?disableNotification = disableNotification,
                       ?supportsStreaming = supportsStreaming,
                       ?hasSpoiler = hasSpoiler,
                       ?captionEntities = captionEntities,
                       ?parseMode = parseMode,
                       ?caption = caption,
                       ?thumbnail = thumbnail,
                       ?height = height,
                       ?width = width,
                       ?duration = duration,
                       ?messageThreadId = messageThreadId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)

    static member Make(chatId: string,
                       video: InputFile,
                       ?replyToMessageId: int64,
                       ?protectContent: bool,
                       ?disableNotification: bool,
                       ?supportsStreaming: bool,
                       ?hasSpoiler: bool,
                       ?captionEntities: MessageEntity[],
                       ?parseMode: ParseMode,
                       ?caption: string,
                       ?thumbnail: InputFile,
                       ?height: int64,
                       ?width: int64,
                       ?duration: int64,
                       ?messageThreadId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendVideo.Make(ChatId.String chatId,
                       video,
                       ?replyToMessageId = replyToMessageId,
                       ?protectContent = protectContent,
                       ?disableNotification = disableNotification,
                       ?supportsStreaming = supportsStreaming,
                       ?hasSpoiler = hasSpoiler,
                       ?captionEntities = captionEntities,
                       ?parseMode = parseMode,
                       ?caption = caption,
                       ?thumbnail = thumbnail,
                       ?height = height, ?width = width,
                       ?duration = duration,
                       ?messageThreadId = messageThreadId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)

    interface IRequestBase<Message> with
        member _.MethodName = "sendVideo"
        member this.Type = typeof<SendVideo>

type GetWebhookInfo() =
    static member Make() = GetWebhookInfo()
    interface IRequestBase<WebhookInfo> with
        member _.MethodName = "getWebhookInfo"
        member this.Type = typeof<unit>

type GetMe() =
    static member Make() = GetMe()
    interface IRequestBase<User> with
        member _.MethodName = "getMe"
        member this.Type = typeof<unit>

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

type SendMessage = {
    ChatId: ChatId
    MessageThreadId: int64 option
    Text: string
    ParseMode: ParseMode option
    Entities: MessageEntity[] option
    DisableWebPagePreview: bool option
    DisableNotification: bool option
    ProtectContent: bool option
    ReplyToMessageId: int64 option
    AllowSendingWithoutReply: bool option
    ReplyMarkup: Markup option
}
with
    static member Make(chatId: ChatId,
                       text: string,
                       ?messageThreadId: int64,
                       ?parseMode: ParseMode,
                       ?entities: MessageEntity[],
                       ?disableWebPagePreview: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) = {
        ChatId = chatId
        MessageThreadId = messageThreadId
        Text = text
        ParseMode = parseMode
        Entities = entities
        DisableWebPagePreview = disableWebPagePreview
        DisableNotification = disableNotification
        ProtectContent = protectContent
        ReplyToMessageId = replyToMessageId
        AllowSendingWithoutReply = allowSendingWithoutReply
        ReplyMarkup = replyMarkup
    }

    static member Make(chatId: int64,
                       text: string,
                       ?messageThreadId: int64,
                       ?parseMode: ParseMode,
                       ?entities: MessageEntity[],
                       ?disableWebPagePreview: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendMessage.Make(ChatId.Int chatId,
                         text,
                         ?messageThreadId = messageThreadId,
                         ?parseMode = parseMode,
                         ?entities = entities,
                         ?disableWebPagePreview = disableWebPagePreview,
                         ?disableNotification = disableNotification,
                         ?protectContent = protectContent,
                         ?replyToMessageId = replyToMessageId,
                         ?allowSendingWithoutReply = allowSendingWithoutReply,
                         ?replyMarkup = replyMarkup)

    static member Make(chatId: string,
                       text: string,
                       ?messageThreadId: int64,
                       ?parseMode: ParseMode,
                       ?entities: MessageEntity[],
                       ?disableWebPagePreview: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendMessage.Make(ChatId.String chatId,
                         text,
                         ?messageThreadId = messageThreadId,
                         ?parseMode = parseMode,
                         ?entities = entities,
                         ?disableWebPagePreview = disableWebPagePreview,
                         ?disableNotification = disableNotification,
                         ?protectContent = protectContent,
                         ?replyToMessageId = replyToMessageId,
                         ?allowSendingWithoutReply = allowSendingWithoutReply,
                         ?replyMarkup = replyMarkup)

    interface IRequestBase<Message> with
        member _.MethodName = "sendMessage"
        member this.Type = typeof<SendMessage>

type GetUpdates = {
    Offset: int64 option
    Limit: int64 option
    Timeout: int64 option
    AllowedUpdates: string[] option
}
with
    static member Make(?offset: int64, ?limit: int64, ?timeout: int64, ?allowedUpdates: string[]) = {
        Offset = offset
        Limit = limit
        Timeout = timeout
        AllowedUpdates = allowedUpdates
    }

    interface IRequestBase<Update[]> with
        member _.MethodName = "getUpdates"
        member this.Type = typeof<GetUpdates>

type SendAnimation = {
    ChatId: ChatId
    MessageThreadId: int64 option
    Animation: InputFile
    Duration: int64 option
    Width: int64 option
    Height: int64 option
    Thumbnail: InputFile option
    Caption: string option
    ParseMode: ParseMode option
    CaptionEntities: MessageEntity[] option
    HasSpoiler: bool option
    DisableNotification: bool option
    ProtectContent: bool option
    ReplyToMessageId: int64 option
    AllowSendingWithoutReply: bool option
    ReplyMarkup: Markup option
}
with
    static member Make(chatId: ChatId,
                       animation: InputFile,
                       ?messageThreadId: int64,
                       ?duration: int64,
                       ?width: int64,
                       ?height: int64,
                       ?thumbnail: InputFile,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?hasSpoiler: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) = {
        ChatId = chatId
        MessageThreadId = messageThreadId
        Animation = animation
        Duration = duration
        Width = width
        Height = height
        Thumbnail = thumbnail
        Caption = caption
        ParseMode = parseMode
        CaptionEntities = captionEntities
        HasSpoiler = hasSpoiler
        DisableNotification = disableNotification
        ProtectContent = protectContent
        ReplyToMessageId = replyToMessageId
        AllowSendingWithoutReply = allowSendingWithoutReply
        ReplyMarkup = replyMarkup
    }
    static member Make(chatId: int64,
                       animation: InputFile,
                       ?messageThreadId: int64,
                       ?duration: int64,
                       ?width: int64,
                       ?height: int64,
                       ?thumbnail: InputFile,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?hasSpoiler: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendAnimation.Make(ChatId.Int chatId,
                           animation,
                           ?messageThreadId = messageThreadId,
                           ?duration = duration,
                           ?width = width,
                           ?height = height,
                           ?thumbnail = thumbnail,
                           ?caption = caption,
                           ?parseMode = parseMode,
                           ?captionEntities = captionEntities,
                           ?hasSpoiler = hasSpoiler,
                           ?disableNotification = disableNotification,
                           ?protectContent = protectContent,
                           ?replyToMessageId = replyToMessageId,
                           ?allowSendingWithoutReply = allowSendingWithoutReply,
                           ?replyMarkup = replyMarkup)

    static member Make(chatId: string,
                       animation: InputFile,
                       ?messageThreadId: int64,
                       ?duration: int64,
                       ?width: int64,
                       ?height: int64,
                       ?thumbnail: InputFile,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?hasSpoiler: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendAnimation.Make(ChatId.String chatId,
                           animation,
                           ?messageThreadId = messageThreadId,
                           ?duration = duration,
                           ?width = width,
                           ?height = height,
                           ?thumbnail = thumbnail,
                           ?caption = caption,
                           ?parseMode = parseMode,
                           ?captionEntities = captionEntities,
                           ?hasSpoiler = hasSpoiler,
                           ?disableNotification = disableNotification,
                           ?protectContent = protectContent,
                           ?replyToMessageId = replyToMessageId,
                           ?allowSendingWithoutReply = allowSendingWithoutReply,
                           ?replyMarkup = replyMarkup)
    interface IRequestBase<Message> with
        member _.MethodName = "sendAnimation"
        member this.Type = typeof<SendAnimation>

type SendAudio = {
    ChatId: ChatId
    MessageThreadId: int64 option
    Audio: InputFile
    Caption: string option
    ParseMode: ParseMode option
    CaptionEntities: MessageEntity[] option
    Duration: int64 option
    Performer: string option
    Title: string option
    Thumbnail: InputFile option
    DisableNotification: bool option
    ProtectContent: bool option
    ReplyToMessageId: int64 option
    AllowSendingWithoutReply: bool option
    ReplyMarkup: Markup option
}
with
    static member Make(chatId: ChatId,
                       audio: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?duration: int64,
                       ?performer: string,
                       ?title: string,
                       ?thumbnail: InputFile,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) = {
        ChatId = chatId
        MessageThreadId = messageThreadId
        Audio = audio
        Caption = caption
        ParseMode = parseMode
        CaptionEntities = captionEntities
        Duration = duration
        Performer = performer
        Title = title
        Thumbnail = thumbnail
        DisableNotification = disableNotification
        ProtectContent = protectContent
        ReplyToMessageId = replyToMessageId
        AllowSendingWithoutReply = allowSendingWithoutReply
        ReplyMarkup = replyMarkup
    }
    static member Make(chatId: int64,
                       audio: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?duration: int64,
                       ?performer: string,
                       ?title: string,
                       ?thumbnail: InputFile,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendAudio.Make(ChatId.Int chatId,
                       audio,
                       ?messageThreadId = messageThreadId,
                       ?caption = caption,
                       ?parseMode = parseMode,
                       ?captionEntities = captionEntities,
                       ?duration = duration,
                       ?performer = performer,
                       ?title = title,
                       ?thumbnail = thumbnail,
                       ?disableNotification = disableNotification,
                       ?protectContent = protectContent,
                       ?replyToMessageId = replyToMessageId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)

    static member Make(chatId: string,
                       audio: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?duration: int64,
                       ?performer: string,
                       ?title: string,
                       ?thumbnail: InputFile,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendAudio.Make(ChatId.String chatId,
                       audio,
                       ?messageThreadId = messageThreadId,
                       ?caption = caption,
                       ?parseMode = parseMode,
                       ?captionEntities = captionEntities,
                       ?duration = duration,
                       ?performer = performer,
                       ?title = title,
                       ?thumbnail = thumbnail,
                       ?disableNotification = disableNotification,
                       ?protectContent = protectContent,
                       ?replyToMessageId = replyToMessageId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)

    interface IRequestBase<Message> with
        member _.MethodName = "sendAudio"
        member this.Type = typeof<SendAudio>

type SendPhoto = {
    ChatId: ChatId
    MessageThreadId: int64 option
    Photo: InputFile
    Caption: string option
    ParseMode: ParseMode option
    CaptionEntities: MessageEntity[] option
    HasSpoiler: bool option
    DisableNotification: bool option
    ProtectContent: bool option
    ReplyToMessageId: int64 option
    AllowSendingWithoutReply: bool option
    ReplyMarkup: Markup option
}
with
    static member Make(chatId: ChatId,
                       photo: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?hasSpoiler: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) = {
        ChatId = chatId
        MessageThreadId = messageThreadId
        Photo = photo
        Caption = caption
        ParseMode = parseMode
        CaptionEntities = captionEntities
        HasSpoiler = hasSpoiler
        DisableNotification = disableNotification
        ProtectContent = protectContent
        ReplyToMessageId = replyToMessageId
        AllowSendingWithoutReply = allowSendingWithoutReply
        ReplyMarkup = replyMarkup
    }
    static member Make(chatId: int64,
                       photo: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?hasSpoiler: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendPhoto.Make(ChatId.Int chatId,
                       photo,
                       ?messageThreadId = messageThreadId,
                       ?caption = caption,
                       ?parseMode = parseMode,
                       ?captionEntities = captionEntities,
                       ?hasSpoiler = hasSpoiler,
                       ?disableNotification = disableNotification,
                       ?protectContent = protectContent,
                       ?replyToMessageId = replyToMessageId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)

    static member Make(chatId: string,
                       photo: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?hasSpoiler: bool,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendPhoto.Make(ChatId.String chatId, photo,
                       ?messageThreadId = messageThreadId,
                       ?caption = caption,
                       ?parseMode = parseMode,
                       ?captionEntities = captionEntities,
                       ?hasSpoiler = hasSpoiler,
                       ?disableNotification = disableNotification,
                       ?protectContent = protectContent,
                       ?replyToMessageId = replyToMessageId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)
    interface IRequestBase<Message> with
        member _.MethodName = "sendPhoto"
        member this.Type = typeof<SendPhoto>

type SendVoice = {
    ChatId: ChatId
    MessageThreadId: int64 option
    Voice: InputFile
    Caption: string option
    ParseMode: ParseMode option
    CaptionEntities: MessageEntity[] option
    Duration: int64 option
    DisableNotification: bool option
    ProtectContent: bool option
    ReplyToMessageId: int64 option
    AllowSendingWithoutReply: bool option
    ReplyMarkup: Markup option
}
with
    static member Make(chatId: ChatId,
                       voice: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?duration: int64,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) = {
        ChatId = chatId
        MessageThreadId = messageThreadId
        Voice = voice
        Caption = caption
        ParseMode = parseMode
        CaptionEntities = captionEntities
        Duration = duration
        DisableNotification = disableNotification
        ProtectContent = protectContent
        ReplyToMessageId = replyToMessageId
        AllowSendingWithoutReply = allowSendingWithoutReply
        ReplyMarkup = replyMarkup
    }
    static member Make(chatId: int64,
                       voice: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?duration: int64,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendVoice.Make(ChatId.Int chatId,
                       voice,
                       ?messageThreadId = messageThreadId,
                       ?caption = caption,
                       ?parseMode = parseMode,
                       ?captionEntities = captionEntities,
                       ?duration = duration,
                       ?disableNotification = disableNotification,
                       ?protectContent = protectContent,
                       ?replyToMessageId = replyToMessageId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)

    static member Make(chatId: string,
                       voice: InputFile,
                       ?messageThreadId: int64,
                       ?caption: string,
                       ?parseMode: ParseMode,
                       ?captionEntities: MessageEntity[],
                       ?duration: int64,
                       ?disableNotification: bool,
                       ?protectContent: bool,
                       ?replyToMessageId: int64,
                       ?allowSendingWithoutReply: bool,
                       ?replyMarkup: Markup) =
        SendVoice.Make(ChatId.String chatId,
                       voice,
                       ?messageThreadId = messageThreadId,
                       ?caption = caption,
                       ?parseMode = parseMode,
                       ?captionEntities = captionEntities,
                       ?duration = duration,
                       ?disableNotification = disableNotification,
                       ?protectContent = protectContent,
                       ?replyToMessageId = replyToMessageId,
                       ?allowSendingWithoutReply = allowSendingWithoutReply,
                       ?replyMarkup = replyMarkup)
    interface IRequestBase<Message> with
        member _.MethodName = "sendVoice"
        member this.Type = typeof<SendVoice>

type SetMyCommands = {
    Commands: BotCommand[]
    Scope: BotCommandScope option
    LanguageCode: string option
}
with
    static member Make(commands: BotCommand[],
                       ?scope: BotCommandScope,
                       ?languageCode: string) = {
        Commands = commands
        Scope = scope
        LanguageCode = languageCode
    }
    interface IRequestBase<bool> with
        member _.MethodName = "setMyCommands"
        member this.Type = typeof<SetMyCommands>

type DeleteMessage = {
    ChatId: ChatId
    MessageId: int64
}
with
    static member Make(chatId: ChatId, messageId: int64) = {
        ChatId = chatId
        MessageId = messageId
    }

    static member Make(chatId: int64, messageId: int64) =
        DeleteMessage.Make(ChatId.Int chatId, messageId)

    static member Make(chatId: string, messageId: int64) =
        DeleteMessage.Make(ChatId.String chatId, messageId)

    interface IRequestBase<bool> with
        member _.MethodName = "deleteMessage"
        member this.Type = typeof<DeleteMessage>
