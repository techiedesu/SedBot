namespace SedBot.Telegram.BotApi.Types

open System
open System.IO

type InputFile =
    | Url of Uri
    | File of string * Stream
    | FileId of string

type MaskPoint =
    | Forehead
    | Eyes
    | Mouth
    | Chin

/// This object represents a file ready to be downloaded. The file can be downloaded via the link
/// https://api.telegram.org/file/bot<token>/<file_path>. It is guaranteed that the link will be valid for at least
/// 1 hour. When the link expires, a new one can be requested by calling getFile.
and File = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent defects
    /// in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer or
    /// double-precision float type are safe for storing this value.
    FileSize: int64 option

    /// File path. Use https://api.telegram.org/file/bot<token>/<file_path> to get the file.
    FilePath: string option
} with
    static member Create(fileId: string, fileUniqueId: string, ?fileSize: int64, ?filePath: string) = {
          FileId = fileId
          FileUniqueId = fileUniqueId
          FileSize = fileSize
          FilePath = filePath
    }


and WebhookInfo = {
    /// Webhook URL, may be empty if webhook is not set up
    Url: string

    /// True, if a custom certificate was provided for webhook certificate checks
    HasCustomCertificate: bool

    /// Number of updates awaiting delivery
    PendingUpdateCount: int64

    /// Currently used webhook IP address
    IpAddress: string option

    /// Unix time for the most recent error that happened when trying to deliver an update via webhook
    LastErrorDate: int64 option

    /// Error message in human-readable format for the most recent error that happened when trying
    /// to deliver an update via webhook
    LastErrorMessage: string option

    /// Unix time of the most recent error that happened when trying to synchronize available
    /// updates with Telegram datacenters
    LastSynchronizationErrorDate: int64 option

    /// The maximum allowed number of simultaneous HTTPS connections to the webhook for update delivery
    MaxConnections: int64 option

    /// A list of update types the bot is subscribed to. Defaults to all update types except chat_member
    AllowedUpdates: string[] option
}

and User = {
    /// Unique identifier for this user or bot. This number may have more than 32 significant bits and some programming
    /// languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits, so a
    /// 64-bit integer or double-precision float type are safe for storing this identifier.
    Id: int64

    /// True, if this user is a bot
    IsBot: bool

    /// User's or bot's first name
    FirstName: string

    /// User's or bot's last name
    LastName: string option

    /// User's or bot's username
    Username: string option

    /// IETF language tag of the user's language
    LanguageCode: string option

    /// True, if this user is a Telegram Premium user
    IsPremium: bool option

    /// True, if this user added the bot to the attachment menu
    AddedToAttachmentMenu: bool option

    /// True, if the bot can be invited to groups. Returned only in getMe.
    CanJoinGroups: bool option

    /// True, if privacy mode is disabled for the bot. Returned only in getMe.
    CanReadAllGroupMessages: bool option

    /// True, if the bot supports inline queries. Returned only in getMe.
    SupportsInlineQueries: bool option
}

/// This object represents one special entity in a text message. For example, hashtags, usernames, URLs, etc.
and MessageEntity = {
    /// Type of the entity. Currently, can be “mention" (@username), “hashtag" (#hashtag), “cashtag" ($USD),
    /// “bot_command" (/start@jobs_bot), “url" (https://telegram.org), “email" (do-not-reply@telegram.org),
    /// “phone_number" (+1-212-555-0123), “bold" (bold text), “italic" (italic text),
    /// “underline" (underlined text), “strikethrough" (strikethrough text),
    /// “spoiler" (spoiler message), “code" (monowidth string),
    /// “pre" (monowidth block), “text_link" (for clickable text URLs),
    /// “text_mention" (for users without usernames),
    /// “custom_emoji" (for inline custom emoji stickers)
    Type: string

    /// Offset in UTF-16 code units to the start of the entity
    Offset: int64

    /// Length of the entity in UTF-16 code units
    Length: int64

    /// For “text_link" only, URL that will be opened after user taps on the text
    Url: string option

    /// For “text_mention" only, the mentioned user
    User: User option

    /// For “pre" only, the programming language of the entity text
    Language: string option

    /// For “custom_emoji" only, unique identifier of the custom emoji.
    /// Use getCustomEmojiStickers to get full information about the sticker
    CustomEmojiId: string option
}

and InlineKeyboardButton = {
    /// Label text on the button
    Text: string

    /// HTTP or tg:// URL to be opened when the button is pressed. Links tg://user?id=<user_id> can
    /// be used to mention a user by their ID without using a username, if this is allowed by their privacy settings.
    Url: string option

    /// Data to be sent in a callback query to the bot when button is pressed, 1-64 bytes
    CallbackData: string option

    /// Description of the Web App that will be launched when the user presses the button. The Web App will be able
    /// to send an arbitrary message on behalf of the user using the method answerWebAppQuery. Available
    /// only in private chats between a user and the bot.
    WebApp: WebAppInfo option

    /// An HTTPS URL used to automatically authorize the user. Can be used as a replacement for the Telegram
    /// Login Widget.
    LoginUrl: LoginUrl option

    /// If set, pressing the button will prompt the user to select one of their chats, open that chat and insert the
    /// bot's username and the specified inline query in the input field. May be empty, in which case just
    /// the bot's username will be inserted.
    SwitchInlineQuery: string option

    /// If set, pressing the button will insert the bot's username and the specified inline query
    /// in the current chat's input field. May be empty, in which case only the bot's username will be inserted.
    ///
    /// This offers a quick way for the user to open your bot in inline mode in the same chat -
    /// good for selecting something from multiple options.
    SwitchInlineQueryCurrentChat: string option

    /// If set, pressing the button will prompt the user to select one of their chats of the specified type,
    /// open that chat and insert the bot's username and the specified inline query in the input field
    SwitchInlineQueryChosenChat: SwitchInlineQueryChosenChat option

    /// Description of the game that will be launched when the user presses the button.
    ///
    /// NOTE: This type of button must always be the first button in the first row.
    CallbackGame: CallbackGame option

    /// Specify True, to send a Pay button.
    ///
    /// NOTE: This type of button must always be the first button in the first row and can only be used in invoice messages.
    Pay: bool option
}

/// This object represents a service message about a change in auto-delete timer settings.
and MessageAutoDeleteTimerChanged = {
    /// New auto-delete time for messages in the chat; in seconds
    MessageAutoDeleteTime: int64
} with
    static member Create(messageAutoDeleteTime: int64) = {
        MessageAutoDeleteTime = messageAutoDeleteTime
    }

and WebAppInfo = {
    /// An HTTPS URL of a Web App to be opened with additional data as specified in Initializing Web Apps
    Url: string
}

/// This object contains basic information about a successful payment.
and SuccessfulPayment = {
    /// Three-letter ISO 4217 currency code
    Currency: string

    /// Total price in the smallest units of the currency (integer, not float/double). For example,
    /// for a price of US$ 1.45 pass amount = 145. See the exp parameter in currencies.json,
    /// it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).
    TotalAmount: int64

    /// Bot specified invoice payload
    InvoicePayload: string

    /// Identifier of the shipping option chosen by the user
    ShippingOptionId: string option

    /// Order information provided by the user
    OrderInfo: OrderInfo option

    /// Telegram payment identifier
    TelegramPaymentChargeId: string

    /// Provider payment identifier
    ProviderPaymentChargeId: string
} with
    static member Create(currency: string,
                         totalAmount: int64,
                         invoicePayload: string,
                         telegramPaymentChargeId: string,
                         providerPaymentChargeId: string,
                         ?shippingOptionId: string,
                         ?orderInfo: OrderInfo) = {
        Currency = currency
        TotalAmount = totalAmount
        InvoicePayload = invoicePayload
        TelegramPaymentChargeId = telegramPaymentChargeId
        ProviderPaymentChargeId = providerPaymentChargeId
        ShippingOptionId = shippingOptionId
        OrderInfo = orderInfo
    }

/// This object contains information about the user whose identifier was shared with the bot using a KeyboardButtonRequestUser button.
and UserShared = {
    /// Identifier of the request
    RequestId: int64

    /// Identifier of the shared user.
    /// This number may have more than 32 significant bits and some programming languages may have difficulty/silent
    /// defects in interpreting it. But it has at most 52 significant bits, so a 64-bit integer or double-precision
    /// float type are safe for storing this identifier. The bot may not have access to the user and could be unable
    /// to use this identifier, unless the user is already known to the bot by some other means.
    UserId: int64
} with
    static member Create(requestId: int64, userId: int64) = {
        RequestId = requestId
        UserId = userId
    }

/// This object contains information about the chat whose identifier was shared with the bot using
/// a KeyboardButtonRequestChat button.
and ChatShared = {
    /// Identifier of the request
    RequestId: int64

    /// Identifier of the shared chat. This number may have more than 32 significant bits and some programming
    /// languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits,
    /// so a 64-bit integer or double-precision float type are safe for storing this identifier.
    /// The bot may not have access to the chat and could be unable to use this identifier, unless the chat
    /// is already known to the bot by some other means.
    ChatId: int64
} with
    static member Create(requestId: int64, chatId: int64) = {
        RequestId = requestId
        ChatId = chatId
    }

/// Describes documents or other Telegram Passport elements shared with the bot by the user.
and EncryptedPassportElement = {
    /// Element type. One of “personal_details”, “passport”, “driver_license”, “identity_card”, “internal_passport”,
    /// “address”, “utility_bill”, “bank_statement”, “rental_agreement”, “passport_registration”,
    /// “temporary_registration”, “phone_number”, “email”.
    Type: string

    /// Base64-encoded encrypted Telegram Passport element data provided by the user, available for
    /// “personal_details”, “passport”, “driver_license”, “identity_card”, “internal_passport” and “address” types.
    /// Can be decrypted and verified using the accompanying EncryptedCredentials.
    Data: string option

    /// User's verified phone number, available only for “phone_number” type
    PhoneNumber: string option

    /// User's verified email address, available only for “email” type
    Email: string option

    /// Array of encrypted files with documents provided by the user, available for “utility_bill”,
    /// “bank_statement”, “rental_agreement”, “passport_registration” and “temporary_registration” types.
    /// Files can be decrypted and verified using the accompanying EncryptedCredentials.
    Files: PassportFile[] option

    /// Encrypted file with the front side of the document, provided by the user. Available for “passport”,
    /// “driver_license”, “identity_card” and “internal_passport”. The file can be decrypted and verified using
    /// the accompanying EncryptedCredentials.
    FrontSide: PassportFile option

    /// Encrypted file with the reverse side of the document, provided by the user. Available for “driver_license”
    /// and “identity_card”. The file can be decrypted and verified using the accompanying
    /// EncryptedCredentials.
    ReverseSide: PassportFile option

    /// Encrypted file with the selfie of the user holding a document, provided by the user; available for
    /// “passport”, “driver_license”, “identity_card” and “internal_passport”. The file can be decrypted
    /// and verified using the accompanying EncryptedCredentials.
    Selfie: PassportFile option

    /// Array of encrypted files with translated versions of documents provided by the user.
    /// Available if requested for “passport”, “driver_license”, “identity_card”, “internal_passport”,
    /// “utility_bill”, “bank_statement”, “rental_agreement”, “passport_registration” and “temporary_registration”
    /// types. Files can be decrypted and verified using the accompanying EncryptedCredentials.
    Translation: PassportFile[] option

    /// Base64-encoded element hash for using in PassportElementErrorUnspecified
    Hash: string
} with
    static member Create(``type``: string,
                       hash: string,
                       ?data: string,
                       ?phoneNumber: string,
                       ?email: string,
                       ?files: PassportFile[],
                       ?frontSide: PassportFile,
                       ?reverseSide: PassportFile,
                       ?selfie: PassportFile,
                       ?translation: PassportFile[]) = {
        Type = ``type``
        Hash = hash
        Data = data
        PhoneNumber = phoneNumber
        Email = email
        Files = files
        FrontSide = frontSide
        ReverseSide = reverseSide
        Selfie = selfie
        Translation = translation
    }

/// This object represents a file uploaded to Telegram Passport. Currently all Telegram Passport files are in
/// JPEG format when decrypted and don't exceed 10MB.
and PassportFile = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// File size in bytes
    FileSize: int64

    /// Unix time when the file was uploaded
    FileDate: int64
} with
    static member Create(fileId: string, fileUniqueId: string, fileSize: int64, fileDate: int64) = {
        FileId = fileId
        FileUniqueId = fileUniqueId
        FileSize = fileSize
        FileDate = fileDate
    }

/// This object represents a game. Use BotFather to create and edit games, their short names will act as unique identifiers.
and Game = {
    /// Title of the game
    Title: string

    /// Description of the game
    Description: string

    /// Photo that will be displayed in the game message in chats.
    Photo: PhotoSize[]

    /// Brief description of the game or high scores included in the game message.
    /// Can be automatically edited to include current high scores for the game when the bot calls setGameScore,
    /// or manually edited using editMessageText. 0-4096 characters.
    Text: string option

    /// Special entities that appear in text, such as usernames, URLs, bot commands, etc.
    TextEntities: MessageEntity[] option

    /// Animation that will be displayed in the game message in chats. Upload via BotFather
    Animation: Animation option
} with
    static member Create(title: string,
                         description: string,
                         photo: PhotoSize[],
                         ?text: string,
                         ?textEntities: MessageEntity[],
                         ?animation: Animation) = {
        Title = title
        Description = description
        Photo = photo
        Text = text
        TextEntities = textEntities
        Animation = animation
    }

/// This object represents a service message about a new forum topic created in the chat.
and ForumTopicCreated = {
    /// Name of the topic
    Name: string

    /// Color of the topic icon in RGB format
    IconColor: int64

    /// Unique identifier of the custom emoji shown as the topic icon
    IconCustomEmojiId: string option
} with
    static member Create(name: string, iconColor: int64, ?iconCustomEmojiId: string) = {
        Name = name
        IconColor = iconColor
        IconCustomEmojiId = iconCustomEmojiId
    }

/// This object represents a service message about a forum topic closed in the chat. Currently holds no information.
and ForumTopicClosed =
  new() = {}

/// This object represents a service message about an edited forum topic.
and ForumTopicEdited = {
    /// New name of the topic, if it was edited
    Name: string option

    /// New identifier of the custom emoji shown as the topic icon, if it was edited; an empty string if the icon was removed
    IconCustomEmojiId: string option
} with
    static member Create(?name: string, ?iconCustomEmojiId: string) = {
        Name = name
        IconCustomEmojiId = iconCustomEmojiId
    }

/// This object represents a service message about a forum topic reopened in the chat. Currently holds no information.
and ForumTopicReopened =
  new() = {}

/// This object represents a service message about General forum topic hidden in the chat. Currently holds no information.
and GeneralForumTopicHidden =
  new() = {}

/// This object represents a service message about General forum topic unhidden in the chat. Currently holds no information.
and GeneralForumTopicUnhidden =
  new() = {}

/// This object represents the content of a service message, sent whenever a user in the chat triggers a proximity alert set by another user.
and ProximityAlertTriggered = {
/// User that triggered the alert
    Traveler: User

    /// User that set the alert
    Watcher: User

    /// The distance between the users
    Distance: int64
} with
    static member Create(traveler: User, watcher: User, distance: int64) = {
        Traveler = traveler
        Watcher = watcher
        Distance = distance
    }

/// Describes Telegram Passport data shared with the bot by the user.
and PassportData = {
    /// Array with information about documents and other Telegram Passport elements that was shared with the bot
    Data: EncryptedPassportElement[]

    /// Encrypted credentials required to decrypt the data
    Credentials: EncryptedCredentials
} with
    static member Create(data: EncryptedPassportElement[], credentials: EncryptedCredentials) = {
        Data = data
        Credentials = credentials
    }

/// Describes data required for decrypting and authenticating EncryptedPassportElement. See the Telegram Passport Documentation for a complete description of the data decryption and authentication processes.
and EncryptedCredentials = {
    /// Base64-encoded encrypted JSON-serialized data with unique user's payload,
    /// data hashes and secrets required for EncryptedPassportElement
    /// decryption and authentication
    Data: string

    /// Base64-encoded data hash for data authentication
    Hash: string

    /// Base64-encoded secret, encrypted with the bot's public RSA key, required for data decryption
    Secret: string
} with
    static member Create(data: string, hash: string, secret: string) = {
        Data = data
        Hash = hash
        Secret = secret
    }

/// This object represents a service message about a user allowing a bot to write messages after adding it to
/// the attachment menu, launching a Web App from a link, or accepting an explicit request from a
/// Web App sent by the method requestWriteAccess.
and WriteAccessAllowed = {
    /// True, if the access was granted after the user accepted an explicit request from a Web App sent by the method requestWriteAccess
    FromRequest: bool option

    /// Name of the Web App, if the access was granted when the Web App was launched from a link
    WebAppName: string option

    /// True, if the access was granted when the bot was added to the attachment or side menu
    FromAttachmentMenu: bool option
} with
    static member Create(?fromRequest: bool, ?webAppName: string, ?fromAttachmentMenu: bool) = {
        FromRequest = fromRequest
        WebAppName = webAppName
        FromAttachmentMenu = fromAttachmentMenu
    }

/// This object contains basic information about an invoice.
and Invoice = {
    /// Product name
    Title: string

    /// Product description
    Description: string

    /// Unique bot deep-linking parameter that can be used to generate this invoice
    StartParameter: string

    /// Three-letter ISO 4217 currency code
    Currency: string

    /// Total price in the smallest units of the currency (integer, not float/double).
    /// For example, for a price of US$ 1.45 pass amount = 145. See the exp parameter
    /// in currencies.json, it shows the number of digits past the decimal point
    /// for each currency (2 for the majority of currencies).
    TotalAmount: int64
} with
    static member Create(title: string,
                         description: string,
                         startParameter: string,
                         currency: string,
                         totalAmount: int64) = {
        Title = title
        Description = description
        StartParameter = startParameter
        Currency = currency
        TotalAmount = totalAmount
    }

/// This object represents a parameter of the inline keyboard button used to automatically authorize a user.
/// Serves as a great replacement for the Telegram Login Widget when the user is coming from Telegram.
/// All the user needs to do is tap/click a button and confirm that they want to log in:
/// Telegram apps support these buttons as of version 5.7.
and LoginUrl = {
    /// An HTTPS URL to be opened with user authorization data added to the query string
    /// when the button is pressed. If the user refuses to provide authorization data, the original
    /// URL without information about the user will be opened.
    /// The data added is the same as described in Receiving authorization data.
    ///
    /// NOTE: You must always check the hash of the received data to verify
    /// the authentication and the integrity of the data as described in Checking authorization.
    Url: string

    /// New text of the button in forwarded messages.
    ForwardText: string option

    /// Username of a bot, which will be used for user authorization.
    /// See Setting up a bot for more details. If not specified, the current
    /// bot's username will be assumed. The url's domain must be the same
    /// as the domain linked with the bot. See Linking your domain to the bot for more details.
    BotUsername: string option

    /// Pass True to request the permission for your bot to send messages to the user.
    RequestWriteAccess: bool option
}

/// A placeholder, currently holds no information. Use BotFather to set up your game.
and CallbackGame =
    new() = { }

/// This object represents an inline button that switches the current user to
/// inline mode in a chosen chat, with an optional default inline query.
and SwitchInlineQueryChosenChat = {
    /// The default inline query to be inserted in the input field. If left empty,
    /// only the bot's username will be inserted
    Query: string option

    /// True, if private chats with users can be chosen
    AllowUserChats: bool option

    /// True, if private chats with bots can be chosen
    AllowBotChats: bool option

    /// True, if group and supergroup chats can be chosen
    AllowGroupChats: bool option

    /// True, if channel chats can be chosen
    AllowChannelChats: bool option
}

and ChatId =
    | Int of int64
    | String of string

/// Message text parsing mode
and ParseMode =
    /// Markdown parse syntax
    | Markdown
    /// Html parse syntax
    | HTML

/// This object represents an inline keyboard that appears right next to the message it belongs to.
/// Note: This will only work in Telegram versions released after 9 April, 2016.
/// Older clients will display unsupported message.
and InlineKeyboardMarkup =  {
    /// Array of button rows, each represented by an Array of InlineKeyboardButton objects
    InlineKeyboard: InlineKeyboardButton[][]
} with
    static member Create(inlineKeyboard: InlineKeyboardButton[][]) = {
        InlineKeyboard = inlineKeyboard
    }

/// This object represents a message about a forwarded story in the chat. Currently holds no information.
and Story =
    new() = {}

/// This object represents a custom keyboard with reply options
/// (see Introduction to bots for details and examples).
and ReplyKeyboardMarkup = {
    /// Array of button rows, each represented by an Array of KeyboardButton objects
    Keyboard: KeyboardButton[][]

    /// Requests clients to always show the keyboard when the regular keyboard is hidden.
    /// Defaults to false, in which case the custom keyboard can be hidden and opened with a keyboard icon.
    IsPersistent: bool option

    /// Requests clients to resize the keyboard vertically for optimal fit (e.g., make the keyboard smaller
    /// if there are just two rows of buttons). Defaults to false, in which case the custom keyboard
    /// is always of the same height as the app's standard keyboard.
    ResizeKeyboard: bool option

    /// Requests clients to hide the keyboard as soon as it's been used.
    /// The keyboard will still be available, but clients will automatically display
    /// the usual letter-keyboard in the chat - the user can press a special button
    /// in the input field to see the custom keyboard again. Defaults to false.
    OneTimeKeyboard: bool option

    /// The placeholder to be shown in the input field when the keyboard is active; 1-64 characters
    InputFieldPlaceholder: string option

    /// Use this parameter if you want to show the keyboard to specific users only.
    /// Targets:
    /// 1) users that are @mentioned in the text of the Message object;
    /// 2) if the bot's message is a reply (has reply_to_message_id), sender of the original message.
    ///
    /// Example: A user requests to change the bot's language,
    /// bot replies to the request with a keyboard to select the new language. Other users
    /// in the group don't see the keyboard.
    Selective: bool option
}

/// This object represents one button of the reply keyboard. For simple text buttons,
/// String can be used instead of this object to specify the button text.
/// The optional fields web_app, request_user, request_chat, request_contact, request_location, and request_poll
/// are mutually exclusive.
/// Note:request_contact and request_location options will only work in Telegram versions
/// released after 9 April, 2016. Older clients will display unsupported message.
/// Note:request_poll option will only work in Telegram versions released after 23 January, 2020.
/// Older clients will display unsupported message.
/// Note:web_app option will only work in Telegram versions released after 16 April, 2022.
/// Older clients will display unsupported message.
/// Note:request_user and request_chat options will only work in Telegram versions released
/// after 3 February, 2023. Older clients will display unsupported message.
and KeyboardButton = {
    /// Text of the button. If none of the optional fields are used, it will be sent as a message when the button is pressed
    Text: string

    /// If specified, pressing the button will open a list of suitable users. Tapping on any user will send their
    /// identifier to the bot in a “user_shared" service message. Available in private chats only.
    RequestUser: KeyboardButtonRequestUser option

    /// If specified, pressing the button will open a list of suitable chats. Tapping on a chat will send its
    /// identifier to the bot in a “chat_shared" service message. Available in private chats only.
    RequestChat: KeyboardButtonRequestChat option

    /// If True, the user's phone number will be sent as a contact when the button is pressed. Available in private chats only.
    RequestContact: bool option

    /// If True, the user's current location will be sent when the button is pressed. Available in private chats only.
    RequestLocation: bool option

    /// If specified, the user will be asked to create a poll and send it to the bot when the button is pressed.
    /// Available in private chats only.
    RequestPoll: KeyboardButtonPollType option

    /// If specified, the described Web App will be launched when the button is pressed. The Web App will be able to
    /// send a “web_app_data" service message. Available in private chats only.
    WebApp: WebAppInfo option
}

/// This object represents a voice note.
and Voice = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Duration of the audio in seconds as defined by sender
    Duration: int64

    /// MIME type of the file as defined by sender
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent
    /// defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer
    /// or double-precision float type are safe for storing this value.
    FileSize: int64 option
}

and ChatType =
    | Private
    | Group
    | SuperGroup
    | Channel
    | Sender
    | Unknown

/// This object represents a chat.
and Chat = {
    /// Unique identifier for this chat. This number may have more than 32 significant bits and some
    /// programming languages may have difficulty/silent defects in interpreting it. But it has at most
    /// 52 significant bits, so a signed 64-bit integer or double-precision float type are safe
    /// for storing this identifier.
    Id: int64

    /// Type of chat, can be either “private", “group", “supergroup" or “channel"
    Type: ChatType

    /// Title, for supergroups, channels and group chats
    Title: string option

    /// Username, for private chats, supergroups and channels if available
    Username: string option

    /// First name of the other party in a private chat
    FirstName: string option

    /// Last name of the other party in a private chat
    LastName: string option

    /// True, if the supergroup chat is a forum (has topics enabled)
    IsForum: bool option

    /// Chat photo. Returned only in getChat.
    Photo: ChatPhoto option

    /// If non-empty, the list of all active chat usernames; for private chats, supergroups and channels.
    /// Returned only in getChat.
    ActiveUsernames: string[] option

    /// Custom emoji identifier of emoji status of the other party in a private chat.
    /// Returned only in getChat.
    EmojiStatusCustomEmojiId: string option

    /// Expiration date of the emoji status of the other party in a private chat in Unix time, if any.
    /// Returned only in getChat.
    EmojiStatusExpirationDate: int64 option

    /// Bio of the other party in a private chat. Returned only in getChat.
    Bio: string option

    /// True, if privacy settings of the other party in the private chat allows
    /// to use tg://user?id=<user_id> links only in chats with the user. Returned only in getChat.
    HasPrivateForwards: bool option

    /// True, if the privacy settings of the other party restrict sending voice and video note messages
    /// in the private chat. Returned only in getChat.
    HasRestrictedVoiceAndVideoMessages: bool option

    /// True, if users need to join the supergroup before they can send messages. Returned only in getChat.
    JoinToSendMessages: bool option

    /// True, if all users directly joining the supergroup need to be approved by supergroup
    /// administrators. Returned only in getChat.
    JoinByRequest: bool option

    /// Description, for groups, supergroups and channel chats. Returned only in getChat.
    Description: string option

    /// Primary invite link, for groups, supergroups and channel chats. Returned only in getChat.
    InviteLink: string option

    /// The most recent pinned message (by sending date). Returned only in getChat.
    PinnedMessage: Message option

    // /// Default chat member permissions, for groups and supergroups. Returned only in getChat.
    Permissions: ChatPermissions option

    /// For supergroups, the minimum allowed delay between consecutive messages sent by each unprivileged user;
    /// in seconds. Returned only in getChat.
    SlowModeDelay: int64 option

    /// The time after which all messages sent to the chat will be automatically deleted; in seconds.
    /// Returned only in getChat.
    MessageAutoDeleteTime: int64 option

    /// True, if aggressive anti-spam checks are enabled in the supergroup. The field is only
    /// available to chat administrators. Returned only in getChat.
    HasAggressiveAntiSpamEnabled: bool option

    /// True, if non-administrators can only get the list of bots and administrators in the chat.
    /// Returned only in getChat.
    HasHiddenMembers: bool option

    /// True, if messages from the chat can't be forwarded to other chats.
    /// Returned only in getChat.
    HasProtectedContent: bool option

    /// For supergroups, name of group sticker set. Returned only in getChat.
    StickerSetName: string option

    /// True, if the bot can change the group sticker set. Returned only in getChat.
    CanSetStickerSet: bool option

    /// Unique identifier for the linked chat, i.e. the discussion group identifier for a channel and vice versa;
    /// for supergroups and channel chats. This identifier may be greater than 32 bits and some programming languages
    /// may have difficulty/silent defects in interpreting it. But it is smaller than 52 bits, so a signed 64 bit
    /// integer or double-precision float type are safe for storing this identifier. Returned only in getChat.
    LinkedChatId: int64 option

    /// For supergroups, the location to which the supergroup is connected. Returned only in getChat.
    Location: ChatLocation option
}

/// Represents a location to which a chat is connected.
and ChatLocation = {
    /// The location to which the supergroup is connected. Can't be a live location.
    Location: Location

    /// Location address; 1-64 characters, as defined by the chat owner
    Address: string
} with
    static member Create(location: Location, address: string) = {
        Location = location
        Address = address
    }

/// Describes actions that a non-administrator user is allowed to take in a chat.
and ChatPermissions = {
    /// True, if the user is allowed to send text messages, contacts, invoices, locations and venues
    CanSendMessages: bool option

    /// True, if the user is allowed to send audios
    CanSendAudios: bool option

    /// True, if the user is allowed to send documents
    CanSendDocuments: bool option

    /// True, if the user is allowed to send photos
    CanSendPhotos: bool option

    /// True, if the user is allowed to send videos
    CanSendVideos: bool option

    /// True, if the user is allowed to send video notes
    CanSendVideoNotes: bool option

    /// True, if the user is allowed to send voice notes
    CanSendVoiceNotes: bool option

    /// True, if the user is allowed to send polls
    CanSendPolls: bool option

    /// True, if the user is allowed to send animations, games, stickers and use inline bots
    CanSendOtherMessages: bool option

    /// True, if the user is allowed to add web page previews to their messages
    CanAddWebPagePreviews: bool option

    /// True, if the user is allowed to change the chat title, photo and other settings. Ignored in public supergroups
    CanChangeInfo: bool option

    /// True, if the user is allowed to invite new users to the chat
    CanInviteUsers: bool option

    /// True, if the user is allowed to pin messages. Ignored in public supergroups
    CanPinMessages: bool option

    /// True, if the user is allowed to create forum topics. If omitted defaults to the value of can_pin_messages
    CanManageTopics: bool option
} with
    static member Create(?canSendMessages: bool, ?canSendAudios: bool, ?canSendDocuments: bool, ?canSendPhotos: bool,
                         ?canSendVideos: bool, ?canSendVideoNotes: bool, ?canSendVoiceNotes: bool, ?canSendPolls: bool,
                         ?canSendOtherMessages: bool, ?canAddWebPagePreviews: bool, ?canChangeInfo: bool,
                         ?canInviteUsers: bool, ?canPinMessages: bool, ?canManageTopics: bool) = {
        CanSendMessages = canSendMessages
        CanSendAudios = canSendAudios
        CanSendDocuments = canSendDocuments
        CanSendPhotos = canSendPhotos
        CanSendVideos = canSendVideos
        CanSendVideoNotes = canSendVideoNotes
        CanSendVoiceNotes = canSendVoiceNotes
        CanSendPolls = canSendPolls
        CanSendOtherMessages = canSendOtherMessages
        CanAddWebPagePreviews = canAddWebPagePreviews
        CanChangeInfo = canChangeInfo
        CanInviteUsers = canInviteUsers
        CanPinMessages = canPinMessages
        CanManageTopics = canManageTopics
    }

and PhotoSize = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time
    /// and for different bots. Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Photo width
    Width: int64

    /// Photo height
    Height: int64

    /// File size in bytes
    FileSize: int64 option
}

/// This object represents an animation file (GIF or H.264/MPEG-4 AVC video without sound).
and Animation = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Video width as defined by sender
    Width: int64

    /// Video height as defined by sender
    Height: int64

    /// Duration of the video in seconds as defined by sender
    Duration: int64

    /// Animation thumbnail as defined by sender
    Thumbnail: PhotoSize option

    /// Original animation filename as defined by sender
    FileName: string option

    /// MIME type of the file as defined by sender
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have
    /// difficulty/silent defects in interpreting it. But it has at most 52 significant bits,
    /// so a signed 64-bit integer or double-precision float type are safe for storing this value.
    FileSize: int64 option
}

/// This object represents a video file.
and Video = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Video width as defined by sender
    Width: int64

    /// Video height as defined by sender
    Height: int64

    /// Duration of the video in seconds as defined by sender
    Duration: int64

    /// Video thumbnail
    Thumbnail: PhotoSize option

    /// Original filename as defined by sender
    FileName: string option

    /// MIME type of the file as defined by sender
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent
    /// defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer
    /// or double-precision float type are safe for storing this value.
    FileSize: int64 option
}

/// This object represents a general file (as opposed to photos, voice messages and audio files).
and Document = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Document thumbnail as defined by sender
    Thumbnail: PhotoSize option

    /// Original filename as defined by sender
    FileName: string option

    /// MIME type of the file as defined by sender
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have
    /// difficulty/silent defects in interpreting it. But it has at most 52 significant bits,
    /// so a signed 64-bit integer or double-precision float type are safe for storing this value.
    FileSize: int64 option
}

/// This object describes the position on faces where a mask should be placed by default.
and MaskPosition = {
    /// The part of the face relative to which the mask should be placed. One of “forehead”, “eyes”, “mouth”, or “chin”.
    Point: MaskPoint

    /// Shift by X-axis measured in widths of the mask scaled to the face size, from left to right. For example, choosing -1.0 will place mask just to the left of the default mask position.
    XShift: float

    /// Shift by Y-axis measured in heights of the mask scaled to the face size, from top to bottom. For example, 1.0 will place the mask just below the default mask position.
    YShift: float

    /// Mask scaling coefficient. For example, 2.0 means double size.
    Scale: float
} with
    static member Create(point: MaskPoint, xShift: float, yShift: float, scale: float) = {
        Point = point
        XShift = xShift
        YShift = yShift
        Scale = scale
    }

/// This object represents a sticker.
and Sticker = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Type of the sticker, currently one of “regular", “mask", “custom_emoji". The type of the sticker is
    /// independent from its format, which is determined by the fields is_animated and is_video.
    Type: string

    /// Sticker width
    Width: int64

    /// Sticker height
    Height: int64

    /// True, if the sticker is animated
    IsAnimated: bool

    /// True, if the sticker is a video sticker
    IsVideo: bool

    /// Sticker thumbnail in the .WEBP or .JPG format
    Thumbnail: PhotoSize option

    /// Emoji associated with the sticker
    Emoji: string option

    /// Name of the sticker set to which the sticker belongs
    SetName: string option

    /// For premium regular stickers, premium animation for the sticker
    PremiumAnimation: File option

    /// For mask stickers, the position where the mask should be placed
    MaskPosition: MaskPosition option

    /// For custom emoji stickers, unique identifier of the custom emoji
    CustomEmojiId: string option

    /// True, if the sticker must be repainted to a text color in messages, the color of the
    /// Telegram Premium badge in emoji status, white color on chat photos,
    /// or another appropriate color in other places
    NeedsRepainting: bool option

    /// File size in bytes
    FileSize: int64 option
}

/// This object represents a phone contact.
and Contact = {
    /// Contact's phone number
    PhoneNumber: string

    /// Contact's first name
    FirstName: string

    /// Contact's last name
    LastName: string option

    /// Contact's user identifier in Telegram. This number may have more than 32 significant bits and some
    /// programming languages may have difficulty/silent defects in interpreting it. But it has at most
    /// 52 significant bits, so a 64-bit integer or double-precision float type are safe for
    /// storing this identifier.
    UserId: int64 option

    /// Additional data about the contact in the form of a vCard
    Vcard: string option
} with
    static member Create(phoneNumber: string,
                         firstName: string,
                         ?lastName: string,
                         ?userId: int64,
                         ?vcard: string) = {
        PhoneNumber = phoneNumber
        FirstName = firstName
        LastName = lastName
        UserId = userId
        Vcard = vcard
    }

/// Describes data sent from a Web App to the bot.
and WebAppData = {
    /// The data. Be aware that a bad client can send arbitrary data in this field.
    Data: string

    /// Text of the web_app keyboard button from which the Web App was opened.
    /// Be aware that a bad client can send arbitrary data in this field.
    ButtonText: string
} with
    static member Create(data: string, buttonText: string) = {
        Data = data
        ButtonText = buttonText
    }

/// This object represents a service message about new members invited to a video chat.
and VideoChatParticipantsInvited = {
    /// New members that were invited to the video chat
    Users: User[]
} with
    static member Create(users: User[]) =  {
        Users = users
    }

/// This object represents a video message (available in Telegram apps as of v.4.0).
and VideoNote =
  {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Video width and height (diameter of the video message) as defined by sender
    Length: int64

    /// Duration of the video in seconds as defined by sender
    Duration: int64

    /// Video thumbnail
    Thumbnail: PhotoSize option

    /// File size in bytes
    FileSize: int64 option
  } with
    static member Create(fileId: string, fileUniqueId: string, length: int64, duration: int64, ?thumbnail: PhotoSize, ?fileSize: int64) = {
        FileId = fileId
        FileUniqueId = fileUniqueId
        Length = length
        Duration = duration
        Thumbnail = thumbnail
        FileSize = fileSize
    }

/// This object represents a venue.
and Venue = {
    /// Venue location. Can't be a live location
    Location: Location

    /// Name of the venue
    Title: string

    /// Address of the venue
    Address: string

    /// Foursquare identifier of the venue
    FoursquareId: string option

    /// Foursquare type of the venue. (For example, “arts_entertainment/default”,
    /// “arts_entertainment/aquarium” or “food/icecream”.)
    FoursquareType: string option

    /// Google Places identifier of the venue
    GooglePlaceId: string option

    /// Google Places type of the venue. (See supported types.)
    GooglePlaceType: string option
} with
    static member Create(location: Location,
                         title: string,
                         address: string,
                         ?foursquareId: string,
                         ?foursquareType: string,
                         ?googlePlaceId: string,
                         ?googlePlaceType: string) = {
        Location = location
        Title = title
        Address = address
        FoursquareId = foursquareId
        FoursquareType = foursquareType
        GooglePlaceId = googlePlaceId
        GooglePlaceType = googlePlaceType
    }

/// This object represents a service message about a video chat ended in the chat.
and VideoChatEnded = {
    /// Video chat duration in seconds
    Duration: int64
} with
    static member Create(duration: int64) = {
        Duration = duration
    }

/// This object represents a chat photo.
and ChatPhoto = {
    /// File identifier of small (160x160) chat photo. This file_id can be used only for photo download and only
    /// for as long as the photo is not changed.
    SmallFileId: string

    /// Unique file identifier of small (160x160) chat photo, which is supposed to be the
    /// same over time and for different bots. Can't be used to download or reuse the file.
    SmallFileUniqueId: string

    /// File identifier of big (640x640) chat photo. This file_id can be used only for photo download and only
    /// for as long as the photo is not changed.
    BigFileId: string

    /// Unique file identifier of big (640x640) chat photo, which is supposed to be the same over time and
    /// for different bots. Can't be used to download or reuse the file.
    BigFileUniqueId: string
} with
    static member Create(smallFileId: string, smallFileUniqueId: string, bigFileId: string, bigFileUniqueId: string) = {
        SmallFileId = smallFileId
        SmallFileUniqueId = smallFileUniqueId
        BigFileId = bigFileId
        BigFileUniqueId = bigFileUniqueId
    }

/// This object represents a service message about a video chat started in the chat. Currently holds no information.
and VideoChatStarted =
  new() = {}

/// This object represents a service message about a video chat scheduled in the chat.
and VideoChatScheduled = {
    /// Point in time (Unix timestamp) when the video chat is supposed to be started by a chat administrator
    StartDate: int64
} with
    static member Create(startDate: int64) = {
        StartDate = startDate
    }

/// This object represents an animated emoji that displays a random value.
and Dice = {
    /// Emoji on which the dice throw animation is based
    Emoji: string

    /// Value of the dice, 1-6 for “”, “” and “” base emoji, 1-5 for “” and “” base emoji, 1-64 for “” base emoji
    Value: int64
} with
    static member Create(emoji: string, value: int64) = {
        Emoji = emoji
        Value = value
    }

/// This object represents a message.
and Message = {
    /// Unique message identifier inside this chat
    MessageId: int64

    /// Unique identifier of a message thread to which the message belongs; for supergroups only
    MessageThreadId: int64 option

    /// Sender of the message; empty for messages sent to channels. For backward compatibility,
    /// the field contains a fake sender user in non-channel chats, if the message was sent on behalf of a chat.
    From: User option

    /// Sender of the message, sent on behalf of a chat. For example, the channel itself for channel posts,
    /// the supergroup itself for messages from anonymous group administrators, the linked channel for messages
    /// automatically forwarded to the discussion group. For backward compatibility, the field from contains a fake
    /// sender user in non-channel chats, if the message was sent on behalf of a chat.
    SenderChat: Chat option

    /// Date the message was sent in Unix time
    Date: int64

    /// Conversation the message belongs to
    Chat: Chat

    /// For forwarded messages, sender of the original message
    ForwardFrom: User option

    /// For messages forwarded from channels or from anonymous administrators, information about the original sender chat
    ForwardFromChat: Chat option

    /// For messages forwarded from channels, identifier of the original message in the channel
    ForwardFromMessageId: int64 option

    /// For forwarded messages that were originally sent in channels or by an anonymous chat administrator, signature of the message sender if present
    ForwardSignature: string option

    /// Sender's name for messages forwarded from users who disallow adding a link to their account in forwarded messages
    ForwardSenderName: string option

    /// For forwarded messages, date the original message was sent in Unix time
    ForwardDate: int64 option

    /// True, if the message is sent to a forum topic
    IsTopicMessage: bool option

    /// True, if the message is a channel post that was automatically forwarded to the connected discussion group
    IsAutomaticForward: bool option

    /// For replies, the original message. Note that the Message object in this field will not contain
    /// further reply_to_message fields even if it itself is a reply.
    ReplyToMessage: Message option

    /// Bot through which the message was sent
    ViaBot: User option

    /// Date the message was last edited in Unix time
    EditDate: int64 option

    /// True, if the message can't be forwarded
    HasProtectedContent: bool option

    /// The unique identifier of a media message group this message belongs to
    MediaGroupId: string option

    /// Signature of the post author for messages in channels,
    /// or the custom title of an anonymous group administrator
    AuthorSignature: string option

    /// For text messages, the actual UTF-8 text of the message
    Text: string option

    /// For text messages, special entities like usernames, URLs, bot commands, etc. that appear in the text
    Entities: MessageEntity[] option

    /// Message is an animation, information about the animation. For backward compatibility,
    /// when this field is set, the document field will also be set
    Animation: Animation option

    /// Message is an audio file, information about the file
    Audio: Audio option

    /// Message is a general file, information about the file
    Document: Document option

    /// Message is a photo, available sizes of the photo
    Photo: PhotoSize[] option

    /// Message is a sticker, information about the sticker
    Sticker: Sticker option

    /// Message is a forwarded story
    Story: Story option

    /// Message is a video, information about the video
    Video: Video option

    /// Message is a video note, information about the video message
    VideoNote: VideoNote option

    /// Message is a voice message, information about the file
    Voice: Voice option

    /// Caption for the animation, audio, document, photo, video or voice
    Caption: string option

    /// For messages with a caption, special entities like usernames, URLs, bot commands, etc.
    /// that appear in the caption
    CaptionEntities: MessageEntity[] option

    /// True, if the message media is covered by a spoiler animation
    HasMediaSpoiler: bool option

    /// Message is a shared contact, information about the contact
    Contact: Contact option

    /// Message is a dice with random value
    Dice: Dice option

    /// Message is a game, information about the game. More about games »
    Game: Game option

    /// Message is a native poll, information about the poll
    Poll: Poll option

    /// Message is a venue, information about the venue. For backward compatibility, when this field is set, the location field will also be set
    Venue: Venue option

    /// Message is a shared location, information about the location
    Location: Location option

    /// New members that were added to the group or supergroup and information about them (the bot itself may be one of these members)
    NewChatMembers: User[] option

    /// A member was removed from the group, information about them (this member may be the bot itself)
    LeftChatMember: User option

    /// A chat title was changed to this value
    NewChatTitle: string option

    /// A chat photo was change to this value
    NewChatPhoto: PhotoSize[] option

    /// Service message: the chat photo was deleted
    DeleteChatPhoto: bool option

    /// Service message: the group has been created
    GroupChatCreated: bool option

    /// Service message: the supergroup has been created. This field can't be received in a message coming through
    /// updates, because bot can't be a member of a supergroup when it is created. It can only be found in
    /// reply_to_message if someone replies to a very first message in a directly created supergroup.
    SupergroupChatCreated: bool option

    /// Service message: the channel has been created. This field can't be received in a message coming through
    /// updates, because bot can't be a member of a channel when it is created. It can only be found in
    /// reply_to_message if someone replies to a very first message in a channel.
    ChannelChatCreated: bool option

    /// Service message: auto-delete timer settings changed in the chat
    MessageAutoDeleteTimerChanged: MessageAutoDeleteTimerChanged option

    /// The group has been migrated to a supergroup with the specified identifier. This number may have more than 32
    /// significant bits and some programming languages may have difficulty/silent defects in interpreting it.
    /// But it has at most 52 significant bits, so a signed 64-bit integer or double-precision float
    /// type are safe for storing this identifier.
    MigrateToChatId: int64 option

    /// The supergroup has been migrated from a group with the specified identifier. This number may have more than 32
    /// significant bits and some programming languages may have difficulty/silent defects in interpreting it.
    /// But it has at most 52 significant bits, so a signed 64-bit integer or double-precision float
    /// type are safe for storing this identifier.
    MigrateFromChatId: int64 option

    /// Specified message was pinned. Note that the Message object in this field will not contain
    /// further reply_to_message fields even if it is itself a reply.
    PinnedMessage: Message option

    /// Message is an invoice for a payment, information about the invoice. More about payments »
    Invoice: Invoice option

    /// Message is a service message about a successful payment, information about the payment. More about payments »
    SuccessfulPayment: SuccessfulPayment option

    /// Service message: a user was shared with the bot
    UserShared: UserShared option

    /// Service message: a chat was shared with the bot
    ChatShared: ChatShared option

    /// The domain name of the website on which the user has logged in. More about Telegram Login »
    ConnectedWebsite: string option

    /// Service message: the user allowed the bot to write messages after adding it to the attachment or side menu,
    /// launching a Web App from a link, or accepting an explicit request from a Web App sent by the method
    /// requestWriteAccess
    WriteAccessAllowed: WriteAccessAllowed option

    /// Telegram Passport data
    PassportData: PassportData option

    /// Service message. A user in the chat triggered another user's proximity alert while sharing Live Location.
    ProximityAlertTriggered: ProximityAlertTriggered option

    /// Service message: forum topic created
    ForumTopicCreated: ForumTopicCreated option

    /// Service message: forum topic edited
    ForumTopicEdited: ForumTopicEdited option

    /// Service message: forum topic closed
    ForumTopicClosed: ForumTopicClosed option

    /// Service message: forum topic reopened
    ForumTopicReopened: ForumTopicReopened option

    /// Service message: the 'General' forum topic hidden
    GeneralForumTopicHidden: GeneralForumTopicHidden option

    /// Service message: the 'General' forum topic unhidden
    GeneralForumTopicUnhidden: GeneralForumTopicUnhidden option

    /// Service message: video chat scheduled
    VideoChatScheduled: VideoChatScheduled option

    /// Service message: video chat started
    VideoChatStarted: VideoChatStarted option

    /// Service message: video chat ended
    VideoChatEnded: VideoChatEnded option

    /// Service message: new participants invited to a video chat
    VideoChatParticipantsInvited: VideoChatParticipantsInvited option

    /// Service message: data sent by a Web App
    WebAppData: WebAppData option

    /// Inline keyboard attached to the message. login_url buttons are represented as ordinary url buttons.
    ReplyMarkup: InlineKeyboardMarkup option
} with
    static member Empty = {
         MessageId = 0
         MessageThreadId = None
         From = None
         SenderChat = None
         Date = 0
         Chat = {
             Id = 0
             Type = ChatType.Unknown
             Title = None
             Username = None
             FirstName = None
             LastName = None
             IsForum = None
             Photo = None
             ActiveUsernames = None
             EmojiStatusCustomEmojiId = None
             EmojiStatusExpirationDate = None
             Bio = None
             HasPrivateForwards = None
             HasRestrictedVoiceAndVideoMessages = None
             JoinToSendMessages = None
             JoinByRequest = None
             Description = None
             InviteLink = None
             PinnedMessage = None
             Permissions = None
             SlowModeDelay = None
             MessageAutoDeleteTime = None
             HasAggressiveAntiSpamEnabled = None
             HasHiddenMembers = None
             HasProtectedContent = None
             StickerSetName = None
             CanSetStickerSet = None
             LinkedChatId = None
             Location = None
         }
         ForwardFrom = None
         ForwardFromChat = None
         ForwardFromMessageId = None
         ForwardSignature = None
         ForwardSenderName = None
         ForwardDate = None
         IsTopicMessage = None
         IsAutomaticForward = None
         ReplyToMessage = None
         ViaBot = None
         EditDate = None
         HasProtectedContent = None
         MediaGroupId = None
         AuthorSignature = None
         Text = None
         Entities = None
         Animation = None
         Audio = None
         Document = None
         Photo = None
         Sticker = None
         Story = None
         Video = None
         VideoNote = None
         Voice = None
         Caption = None
         CaptionEntities = None
         HasMediaSpoiler = None
         Contact = None
         Dice = None
         Game = None
         Poll = None
         Venue = None
         Location = None
         NewChatMembers = None
         LeftChatMember = None
         NewChatTitle = None
         NewChatPhoto = None
         DeleteChatPhoto = None
         GroupChatCreated = None
         SupergroupChatCreated = None
         ChannelChatCreated = None
         MessageAutoDeleteTimerChanged = None
         MigrateToChatId = None
         MigrateFromChatId = None
         PinnedMessage = None
         Invoice = None
         SuccessfulPayment = None
         UserShared = None
         ChatShared = None
         ConnectedWebsite = None
         WriteAccessAllowed = None
         PassportData = None
         ProximityAlertTriggered = None
         ForumTopicCreated = None
         ForumTopicEdited = None
         ForumTopicClosed = None
         ForumTopicReopened = None
         GeneralForumTopicHidden = None
         GeneralForumTopicUnhidden = None
         VideoChatScheduled = None
         VideoChatStarted = None
         VideoChatEnded = None
         VideoChatParticipantsInvited = None
         WebAppData = None
         ReplyMarkup = None
    }

/// This object represents an audio file to be treated as music by the Telegram clients.
and Audio = {
    /// Identifier for this file, which can be used to download or reuse the file
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    FileUniqueId: string

    /// Duration of the audio in seconds as defined by sender
    Duration: int64

    /// Performer of the audio as defined by sender or by audio tags
    Performer: string option

    /// Title of the audio as defined by sender or by audio tags
    Title: string option

    /// Original filename as defined by sender
    FileName: string option

    /// MIME type of the file as defined by sender
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent
    /// defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit
    /// integer or double-precision float type are safe for storing this value.
    FileSize: int64 option

    /// Thumbnail of the album cover to which the music file belongs
    Thumbnail: PhotoSize option
}

/// This object represents type of a poll, which is allowed to be created and sent when the
/// corresponding button is pressed.
and KeyboardButtonPollType = {
    /// If quiz is passed, the user will be allowed to create only polls in the quiz mode.
    /// If regular is passed, only regular polls will be allowed. Otherwise, the user will be allowed
    /// to create a poll of any type.
    Type: string option
}

/// This object defines the criteria used to request a suitable user. The identifier of the selected user will be
/// shared with the bot when the corresponding button is pressed. More about requesting users »
and KeyboardButtonRequestUser = {
    /// Signed 32-bit identifier of the request, which will be received back in the UserShared object.
    /// Must be unique within the message
    RequestId: int64

    /// Pass True to request a bot, pass False to request a regular user.
    /// If not specified, no additional restrictions are applied.
    UserIsBot: bool option

    /// Pass True to request a premium user, pass False to request a non-premium user.
    /// If not specified, no additional restrictions are applied.
    UserIsPremium: bool option
}

/// This object defines the criteria used to request a suitable chat. The identifier of the selected chat will
/// be shared with the bot when the corresponding button is pressed. More about requesting chats »
and KeyboardButtonRequestChat = {
    /// Signed 32-bit identifier of the request, which will be received back in the ChatShared object.
    /// Must be unique within the message
    RequestId: int64

    /// Pass True to request a channel chat, pass False to request a group or a supergroup chat.
    ChatIsChannel: bool

    /// Pass True to request a forum supergroup, pass False to request a non-forum chat. If not specified,
    /// no additional restrictions are applied.
    ChatIsForum: bool option

    /// Pass True to request a supergroup or a channel with a username, pass False to request a chat without a username.
    /// If not specified, no additional restrictions are applied.
    ChatHasUsername: bool option

    /// Pass True to request a chat owned by the user. Otherwise, no additional restrictions are applied.
    ChatIsCreated: bool option

    /// A JSON-serialized object listing the required administrator rights of the user in the chat.
    /// The rights must be a superset of bot_administrator_rights. If not specified,
    /// no additional restrictions are applied.
    UserAdministratorRights: ChatAdministratorRights option

    /// A JSON-serialized object listing the required administrator rights of the bot in the chat.
    /// The rights must be a subset of user_administrator_rights. If not specified,
    /// no additional restrictions are applied.
    BotAdministratorRights: ChatAdministratorRights option

    /// Pass True to request a chat with the bot as a member. Otherwise, no additional restrictions are applied.
    BotIsMember: bool option
}

/// Represents the rights of an administrator in a chat.
and ChatAdministratorRights = {
    /// True, if the user's presence in the chat is hidden
    IsAnonymous: bool

    /// True, if the administrator can access the chat event log, boost list in channels, see channel members,
    /// report spam messages, see anonymous administrators in supergroups and ignore slow mode.
    /// Implied by any other administrator privilege
    CanManageChat: bool

    /// True, if the administrator can delete messages of other users
    CanDeleteMessages: bool

    /// True, if the administrator can manage video chats
    CanManageVideoChats: bool

    /// True, if the administrator can restrict, ban or unban chat members, or access supergroup statistics
    CanRestrictMembers: bool

    /// True, if the administrator can add new administrators with a subset of their own privileges or demote
    /// administrators that they have promoted, directly or indirectly (promoted by administrators that
    /// were appointed by the user)
    CanPromoteMembers: bool

    /// True, if the user is allowed to change the chat title, photo and other settings
    CanChangeInfo: bool

    /// True, if the user is allowed to invite new users to the chat
    CanInviteUsers: bool

    /// True, if the administrator can post messages in the channel, or access channel statistics; channels only
    CanPostMessages: bool option

    /// True, if the administrator can edit messages of other users and can pin messages; channels only
    CanEditMessages: bool option

    /// True, if the user is allowed to pin messages; groups and supergroups only
    CanPinMessages: bool option

    /// True, if the administrator can post stories in the channel; channels only
    CanPostStories: bool option

    /// True, if the administrator can edit stories posted by other users; channels only
    CanEditStories: bool option

    /// True, if the administrator can delete stories posted by other users; channels only
    CanDeleteStories: bool option

    /// True, if the user is allowed to create, rename, close, and reopen forum topics; supergroups only
    CanManageTopics: bool option

    /// DEPRECATED: use can_manage_video_chats instead
    CanManageVoiceChats: bool option
}

/// Upon receiving a message with this object, Telegram clients will remove the current custom keyboard
/// and display the default letter-keyboard. By default, custom keyboards are displayed until a new keyboard
/// is sent by a bot. An exception is made for one-time keyboards that are hidden immediately after the user
/// presses a button (see ReplyKeyboardMarkup).
and ReplyKeyboardRemove = {
    /// Requests clients to remove the custom keyboard (user will not be able to summon this keyboard;
    /// if you want to hide the keyboard from sight but keep it accessible, use one_time_keyboard
    /// in ReplyKeyboardMarkup)
    RemoveKeyboard: bool

    /// Use this parameter if you want to remove the keyboard for specific users only. Targets:
    /// 1) users that are @mentioned in the text of the Message object; 2) if the bot's message is a
    /// reply (has reply_to_message_id), sender of the original message.
    ///
    /// Example: A user votes in a poll, bot returns confirmation message in reply to the vote and
    /// removes the keyboard for that user, while still showing the keyboard with poll options to users who haven't voted yet.
    Selective: bool option
}

/// Upon receiving a message with this object, Telegram clients will display a reply interface to the user
/// (act as if the user has selected the bot's message and tapped 'Reply'). This can be extremely useful
/// if you want to create user-friendly step-by-step interfaces without having to sacrifice privacy mode.
and ForceReply = {
    /// Shows reply interface to the user, as if they manually selected the bot's message and tapped 'Reply'
    ForceReply: bool

    /// The placeholder to be shown in the input field when the reply is active; 1-64 characters
    InputFieldPlaceholder: string option

    /// Use this parameter if you want to force reply from specific users only. Targets: 1) users that are
    /// @mentioned in the text of the Message object; 2) if the bot's message is a reply
    /// (has reply_to_message_id), sender of the original message.
    Selective: bool option
}

/// This object represents an incoming update.
/// At most one of the optional parameters can be present in any given update.
and Update = {
    /// The update's unique identifier. Update identifiers start from a certain positive number and increase
    /// sequentially. This ID becomes especially handy if you're using webhooks, since it allows you to ignore
    /// repeated updates or to restore the correct update sequence, should they get out of order.
    /// If there are no new updates for at least a week, then identifier of the next update will
    /// be chosen randomly instead of sequentially.
    UpdateId: int64

    /// New incoming message of any kind - text, photo, sticker, etc.
    Message: Message option

    /// New version of a message that is known to the bot and was edited
    EditedMessage: Message option

    /// New incoming channel post of any kind - text, photo, sticker, etc.
    ChannelPost: Message option

    /// New version of a channel post that is known to the bot and was edited
    EditedChannelPost: Message option

    /// New incoming inline query
    InlineQuery: InlineQuery option

    /// The result of an inline query that was chosen by a user and sent to their chat partner.
    /// Please see our documentation on the feedback collecting for details
    /// on how to enable these updates for your bot.
    ChosenInlineResult: ChosenInlineResult option

    /// New incoming callback query
    CallbackQuery: CallbackQuery option

    /// New incoming shipping query. Only for invoices with flexible price
    ShippingQuery: ShippingQuery option

    /// New incoming pre-checkout query. Contains full information about checkout
    PreCheckoutQuery: PreCheckoutQuery option

    /// New poll state. Bots receive only updates about stopped polls and polls, which are sent by the bot
    Poll: Poll option

    /// A user changed their answer in a non-anonymous poll. Bots receive new votes only in polls that
    /// were sent by the bot itself.
    PollAnswer: PollAnswer option

    /// The bot's chat member status was updated in a chat. For private chats, this update is received only when
    /// the bot is blocked or unblocked by the user.
    MyChatMember: ChatMemberUpdated option

    /// A chat member's status was updated in a chat. The bot must be an administrator in the chat and must explicitly
    /// specify "chat_member" in the list of allowed_updates to receive these updates.
    ChatMember: ChatMemberUpdated option

    /// A request to join the chat has been sent. The bot must have the can_invite_users administrator right in the
    /// chat to receive these updates.
    ChatJoinRequest: ChatJoinRequest option
}

/// This object represents an incoming inline query. When the user sends an empty query, your bot could return some default or trending results.
and InlineQuery = {
    /// Unique identifier for this query
    Id: string

    /// Sender
    From: User

    /// Text of the query (up to 256 characters)
    Query: string

    /// Offset of the results to be returned, can be controlled by the bot
    Offset: string

    /// Type of the chat from which the inline query was sent. Can be either “sender" for a private chat with the inline query sender, “private", “group", “supergroup", or “channel". The chat type should be always known for requests sent from official clients and most third-party clients, unless the request was sent from a secret chat
    ChatType: ChatType option

    /// Sender location, only for bots that request user location
    Location: Location option
} with
    static member Create(id: string, from: User, query: string, offset: string, ?chatType: ChatType, ?location: Location) = {
        Id = id
        From = from
        Query = query
        Offset = offset
        ChatType = chatType
        Location = location
    }

/// This object represents an answer of a user in a non-anonymous poll.
and PollAnswer = {
    /// Unique poll identifier
    PollId: string

    /// The chat that changed the answer to the poll, if the voter is anonymous
    VoterChat: Chat option

    /// The user that changed the answer to the poll, if the voter isn't anonymous
    User: User option

    /// 0-based identifiers of chosen answer options. May be empty if the vote was retracted.
    OptionIds: int64[]
}
with
    static member Create(pollId: string, optionIds: int64[], ?voterChat: Chat, ?user: User) = {
        PollId = pollId
        OptionIds = optionIds
        VoterChat = voterChat
        User = user
    }

/// Represents a join request sent to a chat.
and ChatJoinRequest = {
    /// Chat to which the request was sent
    Chat: Chat

    /// User that sent the join request
    From: User

    /// Identifier of a private chat with the user who sent the join request. This number may have more than 32
    /// significant bits and some programming languages may have difficulty/silent defects in interpreting it.
    /// But it has at most 52 significant bits, so a 64-bit integer or double-precision float type are safe for
    /// storing this identifier. The bot can use this identifier for 5 minutes to send messages until the join request
    /// is processed, assuming no other administrator contacted the user.
    UserChatId: int64

    /// Date the request was sent in Unix time
    Date: DateTime

    /// Bio of the user.
    Bio: string option

    /// Chat invite link that was used by the user to send the join request
    InviteLink: ChatInviteLink option
} with
    static member Create(chat: Chat, from: User, userChatId: int64, date: DateTime, ?bio: string, ?inviteLink: ChatInviteLink) = {
        Chat = chat
        From = from
        UserChatId = userChatId
        Date = date
        Bio = bio
        InviteLink = inviteLink
    }

/// This object contains information about a poll.
and Poll = {
    /// Unique poll identifier
    Id: string

    /// Poll question, 1-300 characters
    Question: string

    /// List of poll options
    Options: PollOption[]

    /// Total number of users that voted in the poll
    TotalVoterCount: int64

    /// True, if the poll is closed
    IsClosed: bool

    /// True, if the poll is anonymous
    IsAnonymous: bool

    /// Poll type, currently can be “regular" or “quiz"
    Type: string

    /// True, if the poll allows multiple answers
    AllowsMultipleAnswers: bool

    /// 0-based identifier of the correct answer option. Available only for polls in the quiz mode, which are closed, or was sent (not forwarded) by the bot or to the private chat with the bot.
    CorrectOptionId: int64 option

    /// Text that is shown when a user chooses an incorrect answer or taps on the lamp icon in a quiz-style poll, 0-200 characters
    Explanation: string option

    /// Special entities like usernames, URLs, bot commands, etc. that appear in the explanation
    ExplanationEntities: MessageEntity[] option

    /// Amount of time in seconds the poll will be active after creation
    OpenPeriod: int64 option

    /// Point in time (Unix timestamp) when the poll will be automatically closed
    CloseDate: int64 option
} with
    static member Create(id: string, question: string, options: PollOption[], totalVoterCount: int64, isClosed: bool, isAnonymous: bool, ``type``: string, allowsMultipleAnswers: bool, ?correctOptionId: int64, ?explanation: string, ?explanationEntities: MessageEntity[], ?openPeriod: int64, ?closeDate: int64) = {
        Id = id
        Question = question
        Options = options
        TotalVoterCount = totalVoterCount
        IsClosed = isClosed
        IsAnonymous = isAnonymous
        Type = ``type``
        AllowsMultipleAnswers = allowsMultipleAnswers
        CorrectOptionId = correctOptionId
        Explanation = explanation
        ExplanationEntities = explanationEntities
        OpenPeriod = openPeriod
        CloseDate = closeDate
    }

/// This object contains information about one answer option in a poll.
and PollOption = {
    /// Option text, 1-100 characters
    Text: string

    /// Number of users that voted for this option
    VoterCount: int64
} with
    static member Create(text: string, voterCount: int64) = {
        Text = text
        VoterCount = voterCount
    }

/// This object contains information about an incoming pre-checkout query.
/// Telegram Passport is a unified authorization method for services that require personal identification. Users can upload their documents once, then instantly share their data with services that require real-world ID (finance, ICOs, etc.). Please see the manual for details.
and PreCheckoutQuery = {
    /// Unique query identifier
    Id: string

    /// User who sent the query
    From: User

    /// Three-letter ISO 4217 currency code
    Currency: string

    /// Total price in the smallest units of the currency (integer, not float/double). For example, for a price of US$ 1.45 pass amount = 145. See the exp parameter in currencies.json, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).
    TotalAmount: int64

    /// Bot specified invoice payload
    InvoicePayload: string

    /// Identifier of the shipping option chosen by the user
    ShippingOptionId: string option

    /// Order information provided by the user
    OrderInfo: OrderInfo option
} with
    static member Create(id: string, from: User, currency: string, totalAmount: int64, invoicePayload: string, ?shippingOptionId: string, ?orderInfo: OrderInfo) = {
        Id = id
        From = from
        Currency = currency
        TotalAmount = totalAmount
        InvoicePayload = invoicePayload
        ShippingOptionId = shippingOptionId
        OrderInfo = orderInfo
    }

 /// This object represents information about an order.
and OrderInfo = {
    /// User name
    Name: string option

    /// User's phone number
    PhoneNumber: string option

    /// User email
    Email: string option

    /// User shipping address
    ShippingAddress: ShippingAddress option
} with
    static member Create(?name: string, ?phoneNumber: string, ?email: string, ?shippingAddress: ShippingAddress) = {
        Name = name
        PhoneNumber = phoneNumber
        Email = email
        ShippingAddress = shippingAddress
    }

 /// This object contains information about an incoming shipping query.
and ShippingQuery = {
    /// Unique query identifier
    Id: string

    /// User who sent the query
    From: User

    /// Bot specified invoice payload
    InvoicePayload: string

    /// User specified shipping address
    ShippingAddress: ShippingAddress
} with
    static member Create(id: string, from: User, invoicePayload: string, shippingAddress: ShippingAddress) = {
        Id = id
        From = from
        InvoicePayload = invoicePayload
        ShippingAddress = shippingAddress
    }

/// This object represents a shipping address.
and ShippingAddress = {
    /// Two-letter ISO 3166-1 alpha-2 country code
    CountryCode: string

    /// State, if applicable
    State: string

    /// City
    City: string

    /// First line for the address
    StreetLine1: string

    /// Second line for the address
    StreetLine2: string

    /// Address post code
    PostCode: string
} with
    static member Create(countryCode: string, state: string, city: string, streetLine1: string, streetLine2: string, postCode: string) = {
        CountryCode = countryCode
        State = state
        City = city
        StreetLine1 = streetLine1
        StreetLine2 = streetLine2
        PostCode = postCode
    }

/// This object represents changes in the status of a chat member.
and ChatMemberUpdated = {
    /// Chat the user belongs to
    Chat: Chat

    /// Performer of the action, which resulted in the change
    From: User

    /// Date the change was done in Unix time
    Date: DateTime

    /// Previous information about the chat member
    OldChatMember: ChatMember

    /// New information about the chat member
    NewChatMember: ChatMember

    /// Chat invite link, which was used by the user to join the chat; for joining by invite link events only.
    InviteLink: ChatInviteLink option

    /// True, if the user joined the chat via a chat folder invite link
    ViaChatFolderInviteLink: bool option
}
with
    static member Create(chat: Chat, from: User, date: DateTime, oldChatMember: ChatMember, newChatMember: ChatMember, ?inviteLink: ChatInviteLink, ?viaChatFolderInviteLink: bool) = {
        Chat = chat
        From = from
        Date = date
        OldChatMember = oldChatMember
        NewChatMember = newChatMember
        InviteLink = inviteLink
        ViaChatFolderInviteLink = viaChatFolderInviteLink
    }

/// Represents an invite link for a chat.
and ChatInviteLink = {
    /// The invite link. If the link was created by another chat administrator, then the second part of the link will be replaced with “…".
    InviteLink: string

    /// Creator of the link
    Creator: User

    /// True, if users joining the chat via the link need to be approved by chat administrators
    CreatesJoinRequest: bool

    /// True, if the link is primary
    IsPrimary: bool

    /// True, if the link is revoked
    IsRevoked: bool

    /// Invite link name
    Name: string option

    /// Point in time (Unix timestamp) when the link will expire or has been expired
    ExpireDate: int64 option

    /// The maximum number of users that can be members of the chat simultaneously after joining the chat via this invite link; 1-99999
    MemberLimit: int64 option

    /// Number of pending join requests created using this link
    PendingJoinRequestCount: int64 option
} with
    static member Create(inviteLink: string, creator: User, createsJoinRequest: bool, isPrimary: bool, isRevoked: bool, ?name: string, ?expireDate: int64, ?memberLimit: int64, ?pendingJoinRequestCount: int64) = {
        InviteLink = inviteLink
        Creator = creator
        CreatesJoinRequest = createsJoinRequest
        IsPrimary = isPrimary
        IsRevoked = isRevoked
        Name = name
        ExpireDate = expireDate
        MemberLimit = memberLimit
        PendingJoinRequestCount = pendingJoinRequestCount
    }

/// This object contains information about one member of a chat. Currently, the following 6 types of chat members are supported:
and ChatMember =
  | Owner of ChatMemberOwner
  | Administrator of ChatMemberAdministrator
  | Member of ChatMemberMember
  | Restricted of ChatMemberRestricted
  | Left of ChatMemberLeft
  | Banned of ChatMemberBanned

/// Represents a chat member that isn't currently a member of the chat, but may join it themselves.
and ChatMemberLeft = {
    /// The member's status in the chat, always “left"
    Status: string

    /// Information about the user
    User: User
} with
    static member Create(status: string, user: User) = {
        Status = status
        User = user
    }

/// Represents a chat member that was banned in the chat and can't return to the chat or view chat messages.
and ChatMemberBanned = {
    /// The member's status in the chat, always “kicked"
    Status: string

    /// Information about the user
    User: User

    /// Date when restrictions will be lifted for this user; Unix time. If 0, then the user is banned forever
    UntilDate: DateTime
} with
    static member Create(status: string, user: User, untilDate: DateTime) = {
        Status = status
        User = user
        UntilDate = untilDate
    }

/// Represents a chat member that is under certain restrictions in the chat. Supergroups only.
and ChatMemberRestricted = {
    /// The member's status in the chat, always “restricted"
    Status: string

    /// Information about the user
    User: User

    /// True, if the user is a member of the chat at the moment of the request
    IsMember: bool

    /// True, if the user is allowed to send text messages, contacts, invoices, locations and venues
    CanSendMessages: bool

    /// True, if the user is allowed to send audios
    CanSendAudios: bool

    /// True, if the user is allowed to send documents
    CanSendDocuments: bool

    /// True, if the user is allowed to send photos
    CanSendPhotos: bool

    /// True, if the user is allowed to send videos
    CanSendVideos: bool

    /// True, if the user is allowed to send video notes
    CanSendVideoNotes: bool

    /// True, if the user is allowed to send voice notes
    CanSendVoiceNotes: bool

    /// True, if the user is allowed to send polls
    CanSendPolls: bool

    /// True, if the user is allowed to send animations, games, stickers and use inline bots
    CanSendOtherMessages: bool

    /// True, if the user is allowed to add web page previews to their messages
    CanAddWebPagePreviews: bool

    /// True, if the user is allowed to change the chat title, photo and other settings
    CanChangeInfo: bool

    /// True, if the user is allowed to invite new users to the chat
    CanInviteUsers: bool

    /// True, if the user is allowed to pin messages
    CanPinMessages: bool

    /// True, if the user is allowed to create forum topics
    CanManageTopics: bool

    /// Date when restrictions will be lifted for this user; Unix time. If 0, then the user is restricted forever

    UntilDate: DateTime
} with
    static member Create(status: string, canPinMessages: bool, canInviteUsers: bool, canChangeInfo: bool, canAddWebPagePreviews: bool, canSendOtherMessages: bool, canSendPolls: bool, canSendVoiceNotes: bool, canSendVideoNotes: bool, canSendVideos: bool, canSendPhotos: bool, canSendDocuments: bool, canSendAudios: bool, canSendMessages: bool, isMember: bool, user: User, canManageTopics: bool, untilDate: DateTime) = {
        Status = status
        CanPinMessages = canPinMessages
        CanInviteUsers = canInviteUsers
        CanChangeInfo = canChangeInfo
        CanAddWebPagePreviews = canAddWebPagePreviews
        CanSendOtherMessages = canSendOtherMessages
        CanSendPolls = canSendPolls
        CanSendVoiceNotes = canSendVoiceNotes
        CanSendVideoNotes = canSendVideoNotes
        CanSendVideos = canSendVideos
        CanSendPhotos = canSendPhotos
        CanSendDocuments = canSendDocuments
        CanSendAudios = canSendAudios
        CanSendMessages = canSendMessages
        IsMember = isMember
        User = user
        CanManageTopics = canManageTopics
        UntilDate = untilDate
    }

/// Represents a chat member that has some additional privileges.
and ChatMemberAdministrator = {
    /// The member's status in the chat, always “administrator"
    Status: string

    /// Information about the user
    User: User

    /// True, if the bot is allowed to edit administrator privileges of that user
    CanBeEdited: bool

    /// True, if the user's presence in the chat is hidden
    IsAnonymous: bool

    /// True, if the administrator can access the chat event log, boost list in channels, see channel members, report spam messages, see anonymous administrators in supergroups and ignore slow mode. Implied by any other administrator privilege
    CanManageChat: bool

    /// True, if the administrator can delete messages of other users
    CanDeleteMessages: bool

    /// True, if the administrator can manage video chats
    CanManageVideoChats: bool

    /// True, if the administrator can restrict, ban or unban chat members, or access supergroup statistics
    CanRestrictMembers: bool

    /// True, if the administrator can add new administrators with a subset of their own privileges or demote administrators that they have promoted, directly or indirectly (promoted by administrators that were appointed by the user)
    CanPromoteMembers: bool

    /// True, if the user is allowed to change the chat title, photo and other settings
    CanChangeInfo: bool

    /// True, if the user is allowed to invite new users to the chat
    CanInviteUsers: bool

    /// True, if the administrator can post messages in the channel, or access channel statistics; channels only
    CanPostMessages: bool option

    /// True, if the administrator can edit messages of other users and can pin messages; channels only
    CanEditMessages: bool option

    /// True, if the user is allowed to pin messages; groups and supergroups only
    CanPinMessages: bool option

    /// True, if the administrator can post stories in the channel; channels only
    CanPostStories: bool option

    /// True, if the administrator can edit stories posted by other users; channels only
    CanEditStories: bool option

    /// True, if the administrator can delete stories posted by other users; channels only
    CanDeleteStories: bool option

    /// True, if the user is allowed to create, rename, close, and reopen forum topics; supergroups only
    CanManageTopics: bool option

    /// Custom title for this user
    CustomTitle: string option

    /// DEPRECATED: use can_manage_video_chats instead
    CanManageVoiceChats: bool option
} with
    static member Create(status: string, canInviteUsers: bool, canPromoteMembers: bool, canRestrictMembers: bool, canManageVideoChats: bool, canChangeInfo: bool, canManageChat: bool, isAnonymous: bool, canBeEdited: bool, user: User, canDeleteMessages: bool, ?customTitle: string, ?canPostMessages: bool, ?canEditMessages: bool, ?canPinMessages: bool, ?canPostStories: bool, ?canEditStories: bool, ?canDeleteStories: bool, ?canManageTopics: bool, ?canManageVoiceChats: bool) = {
        Status = status
        CanInviteUsers = canInviteUsers
        CanPromoteMembers = canPromoteMembers
        CanRestrictMembers = canRestrictMembers
        CanManageVideoChats = canManageVideoChats
        CanChangeInfo = canChangeInfo
        CanManageChat = canManageChat
        IsAnonymous = isAnonymous
        CanBeEdited = canBeEdited
        User = user
        CanDeleteMessages = canDeleteMessages
        CustomTitle = customTitle
        CanPostMessages = canPostMessages
        CanEditMessages = canEditMessages
        CanPinMessages = canPinMessages
        CanPostStories = canPostStories
        CanEditStories = canEditStories
        CanDeleteStories = canDeleteStories
        CanManageTopics = canManageTopics
        CanManageVoiceChats = canManageVoiceChats
    }

/// Represents a chat member that has no additional privileges or restrictions.
and ChatMemberMember = {
    /// The member's status in the chat, always “member"
    Status: string

    /// Information about the user
    User: User
} with
    static member Create(status: string, user: User) = {
        Status = status
        User = user
    }

/// Represents a chat member that owns the chat and has all administrator privileges.
and ChatMemberOwner = {
    /// The member's status in the chat, always “creator"
    Status: string

    /// Information about the user
    User: User

    /// True, if the user's presence in the chat is hidden
    IsAnonymous: bool

    /// Custom title for this user
    CustomTitle: string option
} with
    static member Create(status: string, user: User, isAnonymous: bool, ?customTitle: string) = {
        Status = status
        User = user
        IsAnonymous = isAnonymous
        CustomTitle = customTitle
    }

/// This object represents an incoming callback query from a callback button in an inline keyboard.
/// If the button that originated the query was attached to a message sent by the bot, the field message will be
/// present. If the button was attached to a message sent via the bot (in inline mode), the field inline_message_id
/// will be present. Exactly one of the fields data or game_short_name will be present.
and CallbackQuery = {
    /// Unique identifier for this query
    Id: string

    /// Sender
    From: User

    /// Message with the callback button that originated the query. Note that message content and message
    /// date will not be available if the message is too old
    Message: Message option

    /// Identifier of the message sent via the bot in inline mode, that originated the query.
    InlineMessageId: string option

    /// Global identifier, uniquely corresponding to the chat to which the message with the callback button was sent.
    /// Useful for high scores in games.
    ChatInstance: string

    /// Data associated with the callback button. Be aware that the message originated the query can contain
    /// no callback buttons with this data.
    Data: string option

    /// Short name of a Game to be returned, serves as the unique identifier for the game
    GameShortName: string option
} with
    static member Create(id: string, from: User, chatInstance: string, ?message: Message, ?inlineMessageId: string,
                         ?data: string, ?gameShortName: string) =  {
        Id = id
        From = from
        ChatInstance = chatInstance
        Message = message
        InlineMessageId = inlineMessageId
        Data = data
        GameShortName = gameShortName
    }

/// Represents a result of an inline query that was chosen by the user and sent to their chat partner.
/// Note: It is necessary to enable inline feedback via @BotFather in order to receive these objects in updates.
and ChosenInlineResult = {
    /// The unique identifier for the result that was chosen
    ResultId: string

    /// The user that chose the result
    From: User

    /// Sender location, only for bots that require user location
    Location: Location option

    /// Identifier of the sent inline message. Available only if there is an inline keyboard attached to the message.
    /// Will be also received in callback queries and can be used to edit the message.
    InlineMessageId: string option

    /// The query that was used to obtain the result
    Query: string
} with
    static member Create(resultId: string, from: User, query: string, ?location: Location, ?inlineMessageId: string) = {
        ResultId = resultId
        From = from
        Query = query
        Location = location
        InlineMessageId = inlineMessageId
    }

/// This object represents a point on the map.
and Location = {
    /// Longitude as defined by sender
    Longitude: float

    /// Latitude as defined by sender
    Latitude: float

    /// The radius of uncertainty for the location, measured in meters; 0-1500
    HorizontalAccuracy: float option

    /// Time relative to the message sending date, during which the location can be updated; in seconds.
    /// For active live locations only.
    LivePeriod: int64 option

    /// The direction in which user is moving, in degrees; 1-360. For active live locations only.
    Heading: int64 option

    /// The maximum distance for proximity alerts about approaching another chat member, in meters.
    /// For sent live locations only.
    ProximityAlertRadius: int64 option
} with
    static member Create(longitude: float, latitude: float, ?horizontalAccuracy: float, ?livePeriod: int64,
                         ?heading: int64, ?proximityAlertRadius: int64) = {
        Longitude = longitude
        Latitude = latitude
        HorizontalAccuracy = horizontalAccuracy
        LivePeriod = livePeriod
        Heading = heading
        ProximityAlertRadius = proximityAlertRadius
    }

and Markup =
    | InlineKeyboardMarkup of InlineKeyboardMarkup
    | ReplyKeyboardMarkup of ReplyKeyboardMarkup
    | ReplyKeyboardRemove of ReplyKeyboardRemove
    | ForceReply of ForceReply

/// This object represents a bot command.
and BotCommand = {
    /// Text of the command; 1-32 characters. Can contain only lowercase English letters, digits and underscores.
    Command: string

    /// Description of the command; 1-256 characters.
    Description: string
}

with static member Create(command: string, description: string) = {
      Command = command
      Description = description
}

/// Represents the default scope of bot commands. Default commands are used if no commands
/// with a narrower scope are specified for the user.
and BotCommandScopeDefault = {
    /// Scope type, must be default
    Type: string
}
with
    static member Create(``type``: string) = {
        Type = ``type``
    }

/// Represents the scope of bot commands, covering all private chats.
and BotCommandScopeAllPrivateChats = {
    /// Scope type, must be all_private_chats
    Type: string
} with
    static member Create(``type``: string) = {
        Type = ``type``
    }

/// Represents the scope of bot commands, covering all group and supergroup chats.
and BotCommandScopeAllGroupChats = {
    /// Scope type, must be all_group_chats
    Type: string
} with
    static member Create(``type``: string) = {
        Type = ``type``
    }

/// Represents the scope of bot commands, covering all group and supergroup chat administrators.
and BotCommandScopeAllChatAdministrators = {
    /// Scope type, must be all_chat_administrators
    Type: string
} with
    static member Create(``type``: string) = {
        Type = ``type``
    }

/// Represents the scope of bot commands, covering a specific chat.
and BotCommandScopeChat = {
    /// Scope type, must be chat
    Type: string

    /// Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)
    ChatId: ChatId
} with
    static member Create(``type``: string, chatId: ChatId) = {
        Type = ``type``
        ChatId = chatId
    }

/// Represents the scope of bot commands, covering all administrators of a specific group or supergroup chat.
and BotCommandScopeChatAdministrators = {
    /// Scope type, must be chat_administrators
    Type: string

    /// Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)
    ChatId: ChatId
} with
    static member Create(``type``: string, chatId: ChatId) = {
        Type = ``type``
        ChatId = chatId
    }

/// Represents the scope of bot commands, covering a specific member of a group or supergroup chat.
and BotCommandScopeChatMember = {
    /// Scope type, must be chat_member
    Type: string

    /// Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)
    ChatId: ChatId

    /// Unique identifier of the target user
    UserId: int64
} with
    static member Create(``type``: string, chatId: ChatId, userId: int64) = {
        Type = ``type``
        ChatId = chatId
        UserId = userId
    }

/// This object represents the scope to which bot commands are applied. Currently, the following 7 scopes are supported:
and BotCommandScope =
    | Default of BotCommandScopeDefault
    | AllPrivateChats of BotCommandScopeAllPrivateChats
    | AllGroupChats of BotCommandScopeAllGroupChats
    | AllChatAdministrators of BotCommandScopeAllChatAdministrators
    | Chat of BotCommandScopeChat
    | ChatAdministrators of BotCommandScopeChatAdministrators
    | ChatMember of BotCommandScopeChatMember
