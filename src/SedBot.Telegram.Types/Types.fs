namespace SedBot.Telegram.Types

open System
open System.IO
open System.Runtime.Serialization
open System.Text.Json.Serialization

type ApiResponse<'a> = {
    [<JsonPropertyName("ok")>]
    Ok: bool

    [<JsonPropertyName("result")>]
    Result: 'a option

    [<JsonPropertyName("description")>]
    Description: string option

    [<JsonPropertyName("error-code")>]
    ErrorCode: int option
}

type IBotRequest =
    [<JsonIgnore>]
    abstract MethodName: string

    [<JsonIgnore>]
    abstract Type: Type

type IRequestBase<'a> =
    inherit IBotRequest

type InputFile =
    | Url of Uri
    | File of string * Stream
    | FileId of string

/// This object represents a file ready to be downloaded. The file can be downloaded via the link https://api.telegram.org/file/bot<token>/<file_path>. It is guaranteed that the link will be valid for at least 1 hour. When the link expires, a new one can be requested by calling getFile.
and [<CLIMutable>] File = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots. Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer or double-precision float type are safe for storing this value.
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option

    /// File path. Use https://api.telegram.org/file/bot<token>/<file_path> to get the file.
    [<JsonPropertyName("file_path")>]
    FilePath: string option
}
    with static member Create(fileId: string, fileUniqueId: string, ?fileSize: int64, ?filePath: string) = {
          FileId = fileId
          FileUniqueId = fileUniqueId
          FileSize = fileSize
          FilePath = filePath
    }


and [<CLIMutable>] WebhookInfo = {
    /// Webhook URL, may be empty if webhook is not set up
    [<JsonPropertyName("url")>]
    Url: string

    /// True, if a custom certificate was provided for webhook certificate checks
    [<JsonPropertyName("has_custom_certificate")>]
    HasCustomCertificate: bool

    /// Number of updates awaiting delivery
    [<JsonPropertyName("pending_update_count")>]
    PendingUpdateCount: int64

    /// Currently used webhook IP address
    [<JsonPropertyName("ip_address")>]
    IpAddress: string option

    /// Unix time for the most recent error that happened when trying to deliver an update via webhook
    [<JsonPropertyName("last_error_date")>]
    LastErrorDate: int64 option

    /// Error message in human-readable format for the most recent error that happened when trying to deliver an update via webhook
    [<JsonPropertyName("last_error_message")>]
    LastErrorMessage: string option

    /// Unix time of the most recent error that happened when trying to synchronize available updates with Telegram datacenters
    [<JsonPropertyName("last_synchronization_error_date")>]
    LastSynchronizationErrorDate: int64 option

    /// The maximum allowed number of simultaneous HTTPS connections to the webhook for update delivery
    [<JsonPropertyName("max_connections")>]
    MaxConnections: int64 option

    /// A list of update types the bot is subscribed to. Defaults to all update types except chat_member
    [<JsonPropertyName("allowed_updates")>]
    AllowedUpdates: string[] option
}

and [<CLIMutable>] User = {
    /// Unique identifier for this user or bot. This number may have more than 32 significant bits and some programming
    /// languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits, so a
    /// 64-bit integer or double-precision float type are safe for storing this identifier.
    [<JsonPropertyName("id")>]
    Id: int64

    /// True, if this user is a bot
    [<JsonPropertyName("is_bot")>]
    IsBot: bool

    /// User's or bot's first name
    [<JsonPropertyName("first_name")>]
    FirstName: string

    /// User's or bot's last name
    [<JsonPropertyName("last_name")>]
    LastName: string option

    /// User's or bot's username
    [<JsonPropertyName("username")>]
    Username: string option

    /// IETF language tag of the user's language
    [<JsonPropertyName("language_code")>]
    LanguageCode: string option

    /// True, if this user is a Telegram Premium user
    [<JsonPropertyName("is_premium")>]
    IsPremium: bool option

    /// True, if this user added the bot to the attachment menu
    [<JsonPropertyName("added_to_attachment_menu")>]
    AddedToAttachmentMenu: bool option

    /// True, if the bot can be invited to groups. Returned only in getMe.
    [<JsonPropertyName("can_join_groups")>]
    CanJoinGroups: bool option

    /// True, if privacy mode is disabled for the bot. Returned only in getMe.
    [<JsonPropertyName("can_read_all_group_messages")>]
    CanReadAllGroupMessages: bool option

    /// True, if the bot supports inline queries. Returned only in getMe.
    [<JsonPropertyName("supports_inline_queries")>]
    SupportsInlineQueries: bool option
}

/// This object represents one special entity in a text message. For example, hashtags, usernames, URLs, etc.
and [<CLIMutable>] MessageEntity = {
    /// Type of the entity. Currently, can be “mention” (@username), “hashtag” (#hashtag), “cashtag” ($USD),
    /// “bot_command” (/start@jobs_bot), “url” (https://telegram.org), “email” (do-not-reply@telegram.org),
    /// “phone_number” (+1-212-555-0123), “bold” (bold text), “italic” (italic text),
    /// “underline” (underlined text), “strikethrough” (strikethrough text),
    /// “spoiler” (spoiler message), “code” (monowidth string),
    /// “pre” (monowidth block), “text_link” (for clickable text URLs),
    /// “text_mention” (for users without usernames),
    /// “custom_emoji” (for inline custom emoji stickers)
    [<JsonPropertyName("type")>]
    Type: string

    /// Offset in UTF-16 code units to the start of the entity
    [<JsonPropertyName("offset")>]
    Offset: int64

    /// Length of the entity in UTF-16 code units
    [<JsonPropertyName("length")>]
    Length: int64

    /// For “text_link” only, URL that will be opened after user taps on the text
    [<JsonPropertyName("url")>]
    Url: string option

    /// For “text_mention” only, the mentioned user
    [<JsonPropertyName("user")>]
    User: User option

    /// For “pre” only, the programming language of the entity text
    [<JsonPropertyName("language")>]
    Language: string option

    /// For “custom_emoji” only, unique identifier of the custom emoji.
    /// Use getCustomEmojiStickers to get full information about the sticker
    [<JsonPropertyName("custom_emoji_id")>]
    CustomEmojiId: string option
}

and [<CLIMutable>] InlineKeyboardButton = {
    /// Label text on the button
    [<JsonPropertyName("text")>]
    Text: string

    /// HTTP or tg:// URL to be opened when the button is pressed. Links tg://user?id=<user_id> can
    /// be used to mention a user by their ID without using a username, if this is allowed by their privacy settings.
    [<JsonPropertyName("url")>]
    Url: string option

    /// Data to be sent in a callback query to the bot when button is pressed, 1-64 bytes
    [<JsonPropertyName("callback_data")>]
    CallbackData: string option

    /// Description of the Web App that will be launched when the user presses the button. The Web App will be able
    /// to send an arbitrary message on behalf of the user using the method answerWebAppQuery. Available
    /// only in private chats between a user and the bot.
    [<JsonPropertyName("web_app")>]
    WebApp: WebAppInfo option

    /// An HTTPS URL used to automatically authorize the user. Can be used as a replacement for the Telegram
    /// Login Widget.
    [<JsonPropertyName("login_url")>]
    LoginUrl: LoginUrl option

    /// If set, pressing the button will prompt the user to select one of their chats, open that chat and insert the
    /// bot's username and the specified inline query in the input field. May be empty, in which case just
    /// the bot's username will be inserted.
    [<JsonPropertyName("switch_inline_query")>]
    SwitchInlineQuery: string option

    /// If set, pressing the button will insert the bot's username and the specified inline query
    /// in the current chat's input field. May be empty, in which case only the bot's username will be inserted.
    ///
    /// This offers a quick way for the user to open your bot in inline mode in the same chat -
    /// good for selecting something from multiple options.
    [<JsonPropertyName("switch_inline_query_current_chat")>]
    SwitchInlineQueryCurrentChat: string option

    /// If set, pressing the button will prompt the user to select one of their chats of the specified type,
    /// open that chat and insert the bot's username and the specified inline query in the input field
    [<JsonPropertyName("switch_inline_query_chosen_chat")>]
    SwitchInlineQueryChosenChat: SwitchInlineQueryChosenChat option

    /// Description of the game that will be launched when the user presses the button.
    ///
    /// NOTE: This type of button must always be the first button in the first row.
    [<JsonPropertyName("callback_game")>]
    CallbackGame: CallbackGame option

    /// Specify True, to send a Pay button.
    ///
    /// NOTE: This type of button must always be the first button in the first row and can only be used in invoice messages.
    [<JsonPropertyName("pay")>]
    Pay: bool option
}

and [<CLIMutable>] WebAppInfo = {
    /// An HTTPS URL of a Web App to be opened with additional data as specified in Initializing Web Apps
    [<JsonPropertyName("url")>]
    Url: string
}

/// This object represents a parameter of the inline keyboard button used to automatically authorize a user.
/// Serves as a great replacement for the Telegram Login Widget when the user is coming from Telegram.
/// All the user needs to do is tap/click a button and confirm that they want to log in:
/// Telegram apps support these buttons as of version 5.7.
and [<CLIMutable>] LoginUrl = {
    /// An HTTPS URL to be opened with user authorization data added to the query string
    /// when the button is pressed. If the user refuses to provide authorization data, the original
    /// URL without information about the user will be opened.
    /// The data added is the same as described in Receiving authorization data.
    ///
    /// NOTE: You must always check the hash of the received data to verify
    /// the authentication and the integrity of the data as described in Checking authorization.
    [<JsonPropertyName("url")>]
    Url: string

    /// New text of the button in forwarded messages.
    [<JsonPropertyName("forward_text")>]
    ForwardText: string option

    /// Username of a bot, which will be used for user authorization.
    /// See Setting up a bot for more details. If not specified, the current
    /// bot's username will be assumed. The url's domain must be the same
    /// as the domain linked with the bot. See Linking your domain to the bot for more details.
    [<JsonPropertyName("bot_username")>]
    BotUsername: string option

    /// Pass True to request the permission for your bot to send messages to the user.
    [<JsonPropertyName("request_write_access")>]
    RequestWriteAccess: bool option
}

/// A placeholder, currently holds no information. Use BotFather to set up your game.
and CallbackGame =
    new() = { }

/// This object represents an inline button that switches the current user to
/// inline mode in a chosen chat, with an optional default inline query.
and [<CLIMutable>] SwitchInlineQueryChosenChat = {
    /// The default inline query to be inserted in the input field. If left empty,
    /// only the bot's username will be inserted
    [<JsonPropertyName("query")>]
    Query: string option

    /// True, if private chats with users can be chosen
    [<JsonPropertyName("allow_user_chats")>]
    AllowUserChats: bool option

    /// True, if private chats with bots can be chosen
    [<JsonPropertyName("allow_bot_chats")>]
    AllowBotChats: bool option

    /// True, if group and supergroup chats can be chosen
    [<JsonPropertyName("allow_group_chats")>]
    AllowGroupChats: bool option

    /// True, if channel chats can be chosen
    [<JsonPropertyName("allow_channel_chats")>]
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
and [<CLIMutable>] InlineKeyboardMarkup =  {
    /// Array of button rows, each represented by an Array of InlineKeyboardButton objects
    [<JsonPropertyName("inline_keyboard")>]
    InlineKeyboard: InlineKeyboardButton[][]
}
with static member Create(inlineKeyboard: InlineKeyboardButton[][]) = {
        InlineKeyboard = inlineKeyboard
    }

/// This object represents a custom keyboard with reply options
/// (see Introduction to bots for details and examples).
and [<CLIMutable>] ReplyKeyboardMarkup = {
    /// Array of button rows, each represented by an Array of KeyboardButton objects
    [<JsonPropertyName("keyboard")>]
    Keyboard: KeyboardButton[][]

    /// Requests clients to always show the keyboard when the regular keyboard is hidden.
    /// Defaults to false, in which case the custom keyboard can be hidden and opened with a keyboard icon.
    [<JsonPropertyName("is_persistent")>]
    IsPersistent: bool option

    /// Requests clients to resize the keyboard vertically for optimal fit (e.g., make the keyboard smaller
    /// if there are just two rows of buttons). Defaults to false, in which case the custom keyboard
    /// is always of the same height as the app's standard keyboard.
    [<JsonPropertyName("resize_keyboard")>]
    ResizeKeyboard: bool option

    /// Requests clients to hide the keyboard as soon as it's been used.
    /// The keyboard will still be available, but clients will automatically display
    /// the usual letter-keyboard in the chat - the user can press a special button
    /// in the input field to see the custom keyboard again. Defaults to false.
    [<JsonPropertyName("one_time_keyboard")>]
    OneTimeKeyboard: bool option

    /// The placeholder to be shown in the input field when the keyboard is active; 1-64 characters
    [<JsonPropertyName("input_field_placeholder")>]
    InputFieldPlaceholder: string option

    /// Use this parameter if you want to show the keyboard to specific users only.
    /// Targets:
    /// 1) users that are @mentioned in the text of the Message object;
    /// 2) if the bot's message is a reply (has reply_to_message_id), sender of the original message.
    ///
    /// Example: A user requests to change the bot's language,
    /// bot replies to the request with a keyboard to select the new language. Other users
    /// in the group don't see the keyboard.
    [<JsonPropertyName("selective")>]
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
and [<CLIMutable>] KeyboardButton = {
    /// Text of the button. If none of the optional fields are used, it will be sent as a message when the button is pressed
    [<JsonPropertyName("text")>]
    Text: string
    // /// If specified, pressing the button will open a list of suitable users. Tapping on any user will send their identifier to the bot in a “user_shared” service message. Available in private chats only.
    // [<JsonPropertyName("request_user")>]
    // RequestUser: KeyboardButtonRequestUser option
    // /// If specified, pressing the button will open a list of suitable chats. Tapping on a chat will send its identifier to the bot in a “chat_shared” service message. Available in private chats only.
    // [<JsonPropertyName("request_chat")>]
    // RequestChat: KeyboardButtonRequestChat option
    /// If True, the user's phone number will be sent as a contact when the button is pressed. Available in private chats only.
    [<JsonPropertyName("request_contact")>]
    RequestContact: bool option
    /// If True, the user's current location will be sent when the button is pressed. Available in private chats only.
    [<JsonPropertyName("request_location")>]
    RequestLocation: bool option
    // /// If specified, the user will be asked to create a poll and send it to the bot when the button is pressed. Available in private chats only.
    // [<JsonPropertyName("request_poll")>]
    // RequestPoll: KeyboardButtonPollType option
    /// If specified, the described Web App will be launched when the button is pressed. The Web App will be able to send a “web_app_data” service message. Available in private chats only.
    [<JsonPropertyName("web_app")>]
    WebApp: WebAppInfo option
}

/// This object represents a voice note.
and [<CLIMutable>] Voice = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string
    /// Unique identifier for this file, which is supposed to be the same over time and for different bots. Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string
    /// Duration of the audio in seconds as defined by sender
    [<JsonPropertyName("duration")>]
    Duration: int64
    /// MIME type of the file as defined by sender
    [<JsonPropertyName("mime_type")>]
    MimeType: string option
    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer or double-precision float type are safe for storing this value.
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option
}

and ChatType =
    | Private
    | Group
    | [<JsonPropertyName("supergroup")>] SuperGroup
    | Channel
    | Sender
    | Unknown

/// This object represents a chat.
and [<CLIMutable>] Chat = {
    /// Unique identifier for this chat. This number may have more than 32 significant bits and some
    /// programming languages may have difficulty/silent defects in interpreting it. But it has at most
    /// 52 significant bits, so a signed 64-bit integer or double-precision float type are safe
    /// for storing this identifier.
    [<JsonPropertyName("id")>]
    Id: int64

    /// Type of chat, can be either “private”, “group”, “supergroup” or “channel”
    [<JsonPropertyName("type")>]
    Type: ChatType

    /// Title, for supergroups, channels and group chats
    [<JsonPropertyName("title")>]
    Title: string option

    /// Username, for private chats, supergroups and channels if available
    [<JsonPropertyName("username")>]
    Username: string option

    /// First name of the other party in a private chat
    [<JsonPropertyName("first_name")>]
    FirstName: string option

    /// Last name of the other party in a private chat
    [<JsonPropertyName("last_name")>]
    LastName: string option

    /// True, if the supergroup chat is a forum (has topics enabled)
    [<JsonPropertyName("is_forum")>]
    IsForum: bool option

    // /// Chat photo. Returned only in getChat.
    // [<JsonPropertyName("photo")>]
    // Photo: ChatPhoto option

    /// If non-empty, the list of all active chat usernames; for private chats, supergroups and channels.
    /// Returned only in getChat.
    [<JsonPropertyName("active_usernames")>]
    ActiveUsernames: string[] option

    /// Custom emoji identifier of emoji status of the other party in a private chat.
    /// Returned only in getChat.
    [<JsonPropertyName("emoji_status_custom_emoji_id")>]
    EmojiStatusCustomEmojiId: string option

    /// Expiration date of the emoji status of the other party in a private chat in Unix time, if any.
    /// Returned only in getChat.
    [<JsonPropertyName("emoji_status_expiration_date")>]
    EmojiStatusExpirationDate: int64 option

    /// Bio of the other party in a private chat. Returned only in getChat.
    [<JsonPropertyName("bio")>]
    Bio: string option

    /// True, if privacy settings of the other party in the private chat allows
    /// to use tg://user?id=<user_id> links only in chats with the user. Returned only in getChat.
    [<JsonPropertyName("has_private_forwards")>]
    HasPrivateForwards: bool option

    /// True, if the privacy settings of the other party restrict sending voice and video note messages
    /// in the private chat. Returned only in getChat.
    [<JsonPropertyName("has_restricted_voice_and_video_messages")>]
    HasRestrictedVoiceAndVideoMessages: bool option

    /// True, if users need to join the supergroup before they can send messages. Returned only in getChat.
    [<JsonPropertyName("join_to_send_messages")>]
    JoinToSendMessages: bool option

    /// True, if all users directly joining the supergroup need to be approved by supergroup
    /// administrators. Returned only in getChat.
    [<JsonPropertyName("join_by_request")>]
    JoinByRequest: bool option

    /// Description, for groups, supergroups and channel chats. Returned only in getChat.
    [<JsonPropertyName("description")>]
    Description: string option

    /// Primary invite link, for groups, supergroups and channel chats. Returned only in getChat.
    [<JsonPropertyName("invite_link")>]
    InviteLink: string option

    /// The most recent pinned message (by sending date). Returned only in getChat.
    [<JsonPropertyName("pinned_message")>]
    PinnedMessage: Message option

    // /// Default chat member permissions, for groups and supergroups. Returned only in getChat.
    // [<JsonPropertyName("permissions")>]
    // Permissions: ChatPermissions option

    /// For supergroups, the minimum allowed delay between consecutive messages sent by each unpriviledged user;
    /// in seconds. Returned only in getChat.
    [<JsonPropertyName("slow_mode_delay")>]
    SlowModeDelay: int64 option

    /// The time after which all messages sent to the chat will be automatically deleted; in seconds.
    /// Returned only in getChat.
    [<JsonPropertyName("message_auto_delete_time")>]
    MessageAutoDeleteTime: int64 option

    /// True, if aggressive anti-spam checks are enabled in the supergroup. The field is only
    /// available to chat administrators. Returned only in getChat.
    [<JsonPropertyName("has_aggressive_anti_spam_enabled")>]
    HasAggressiveAntiSpamEnabled: bool option

    /// True, if non-administrators can only get the list of bots and administrators in the chat.
    /// Returned only in getChat.
    [<JsonPropertyName("has_hidden_members")>]
    HasHiddenMembers: bool option

    /// True, if messages from the chat can't be forwarded to other chats.
    /// Returned only in getChat.
    [<JsonPropertyName("has_protected_content")>]
    HasProtectedContent: bool option

    /// For supergroups, name of group sticker set. Returned only in getChat.
    [<JsonPropertyName("sticker_set_name")>]
    StickerSetName: string option

    /// True, if the bot can change the group sticker set. Returned only in getChat.
    [<JsonPropertyName("can_set_sticker_set")>]
    CanSetStickerSet: bool option

    /// Unique identifier for the linked chat, i.e. the discussion group identifier for a channel and vice versa;
    /// for supergroups and channel chats. This identifier may be greater than 32 bits and some programming languages
    /// may have difficulty/silent defects in interpreting it. But it is smaller than 52 bits, so a signed 64 bit
    /// integer or double-precision float type are safe for storing this identifier. Returned only in getChat.
    [<JsonPropertyName("linked_chat_id")>]
    LinkedChatId: int64 option

// /// For supergroups, the location to which the supergroup is connected. Returned only in getChat.
// [<JsonPropertyName("location")>]
// Location: ChatLocation option
}

and [<CLIMutable>] PhotoSize = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time
    /// and for different bots. Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// Photo width
    [<JsonPropertyName("width")>]
    Width: int64

    /// Photo height
    [<JsonPropertyName("height")>]
    Height: int64

    /// File size in bytes
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option
}

/// This object represents an animation file (GIF or H.264/MPEG-4 AVC video without sound).
and [<CLIMutable>] Animation = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots. Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// Video width as defined by sender
    [<JsonPropertyName("width")>]
    Width: int64

    /// Video height as defined by sender
    [<JsonPropertyName("height")>]
    Height: int64

    /// Duration of the video in seconds as defined by sender
    [<JsonPropertyName("duration")>]
    Duration: int64

    /// Animation thumbnail as defined by sender
    [<JsonPropertyName("thumbnail")>]
    Thumbnail: PhotoSize option

    /// Original animation filename as defined by sender
    [<JsonPropertyName("file_name")>]
    FileName: string option

    /// MIME type of the file as defined by sender
    [<JsonPropertyName("mime_type")>]
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have
    /// difficulty/silent defects in interpreting it. But it has at most 52 significant bits,
    /// so a signed 64-bit integer or double-precision float type are safe for storing this value.
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option
}

/// This object represents a video file.
and [<CLIMutable>] Video = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// Video width as defined by sender
    [<JsonPropertyName("width")>]
    Width: int64

    /// Video height as defined by sender
    [<JsonPropertyName("height")>]
    Height: int64

    /// Duration of the video in seconds as defined by sender
    [<JsonPropertyName("duration")>]
    Duration: int64

    /// Video thumbnail
    [<JsonPropertyName("thumbnail")>]
    Thumbnail: PhotoSize option

    /// Original filename as defined by sender
    [<JsonPropertyName("file_name")>]
    FileName: string option

    /// MIME type of the file as defined by sender
    [<JsonPropertyName("mime_type")>]
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent
    /// defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer
    /// or double-precision float type are safe for storing this value.
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option
}

/// This object represents a general file (as opposed to photos, voice messages and audio files).
and [<CLIMutable>] Document = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// Document thumbnail as defined by sender
    [<JsonPropertyName("thumbnail")>]
    Thumbnail: PhotoSize option

    /// Original filename as defined by sender
    [<JsonPropertyName("file_name")>]
    FileName: string option

    /// MIME type of the file as defined by sender
    [<JsonPropertyName("mime_type")>]
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have
    /// difficulty/silent defects in interpreting it. But it has at most 52 significant bits,
    /// so a signed 64-bit integer or double-precision float type are safe for storing this value.
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option
}

/// This object represents a sticker.
and [<CLIMutable>] Sticker = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// Type of the sticker, currently one of “regular”, “mask”, “custom_emoji”. The type of the sticker is
    /// independent from its format, which is determined by the fields is_animated and is_video.
    [<JsonPropertyName("type")>]
    Type: string

    /// Sticker width
    [<JsonPropertyName("width")>]
    Width: int64

    /// Sticker height
    [<JsonPropertyName("height")>]
    Height: int64

    /// True, if the sticker is animated
    [<JsonPropertyName("is_animated")>]
    IsAnimated: bool

    /// True, if the sticker is a video sticker
    [<JsonPropertyName("is_video")>]
    IsVideo: bool

    /// Sticker thumbnail in the .WEBP or .JPG format
    [<JsonPropertyName("thumbnail")>]
    Thumbnail: PhotoSize option

    /// Emoji associated with the sticker
    [<JsonPropertyName("emoji")>]
    Emoji: string option

    /// Name of the sticker set to which the sticker belongs
    [<JsonPropertyName("set_name")>]
    SetName: string option

    // /// For premium regular stickers, premium animation for the sticker
    // [<JsonPropertyName("premium_animation")>]
    // PremiumAnimation: File option
    // /// For mask stickers, the position where the mask should be placed
    // [<JsonPropertyName("mask_position")>]
    // MaskPosition: MaskPosition option
    /// For custom emoji stickers, unique identifier of the custom emoji
    [<JsonPropertyName("custom_emoji_id")>]
    CustomEmojiId: string option

    /// True, if the sticker must be repainted to a text color in messages, the color of the
    /// Telegram Premium badge in emoji status, white color on chat photos,
    /// or another appropriate color in other places
    [<JsonPropertyName("needs_repainting")>]
    NeedsRepainting: bool option

    /// File size in bytes
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option
}


/// This object represents a message.
and [<CLIMutable>] Message = {
    /// Unique message identifier inside this chat
    [<JsonPropertyName("message_id")>]
    MessageId: int64

    /// Unique identifier of a message thread to which the message belongs; for supergroups only
    [<JsonPropertyName("message_thread_id")>]
    MessageThreadId: int64 option

    /// Sender of the message; empty for messages sent to channels. For backward compatibility, the field contains a fake sender user in non-channel chats, if the message was sent on behalf of a chat.
    [<JsonPropertyName("from")>]
    From: User option

    /// Sender of the message, sent on behalf of a chat. For example, the channel itself for channel posts, the supergroup itself for messages from anonymous group administrators, the linked channel for messages automatically forwarded to the discussion group. For backward compatibility, the field from contains a fake sender user in non-channel chats, if the message was sent on behalf of a chat.
    [<JsonPropertyName("sender_chat")>]
    SenderChat: Chat option

    /// Date the message was sent in Unix time
    [<JsonPropertyName("date")>]
    Date: int64

    /// Conversation the message belongs to
    [<JsonPropertyName("chat")>]
    Chat: Chat

    /// For forwarded messages, sender of the original message
    [<JsonPropertyName("forward_from")>]
    ForwardFrom: User option

    /// For messages forwarded from channels or from anonymous administrators, information about the original sender chat
    [<JsonPropertyName("forward_from_chat")>]
    ForwardFromChat: Chat option

    /// For messages forwarded from channels, identifier of the original message in the channel
    [<JsonPropertyName("forward_from_message_id")>]
    ForwardFromMessageId: int64 option

    /// For forwarded messages that were originally sent in channels or by an anonymous chat administrator, signature of the message sender if present
    [<JsonPropertyName("forward_signature")>]
    ForwardSignature: string option

    /// Sender's name for messages forwarded from users who disallow adding a link to their account in forwarded messages
    [<JsonPropertyName("forward_sender_name")>]
    ForwardSenderName: string option

    /// For forwarded messages, date the original message was sent in Unix time
    [<JsonPropertyName("forward_date")>]
    ForwardDate: int64 option

    /// True, if the message is sent to a forum topic
    [<JsonPropertyName("is_topic_message")>]
    IsTopicMessage: bool option

    /// True, if the message is a channel post that was automatically forwarded to the connected discussion group
    [<JsonPropertyName("is_automatic_forward")>]
    IsAutomaticForward: bool option

    /// For replies, the original message. Note that the Message object in this field will not contain further reply_to_message fields even if it itself is a reply.
    [<JsonPropertyName("reply_to_message")>]
    ReplyToMessage: Message option

    /// Bot through which the message was sent
    [<JsonPropertyName("via_bot")>]
    ViaBot: User option

    /// Date the message was last edited in Unix time
    [<JsonPropertyName("edit_date")>]
    EditDate: int64 option

    /// True, if the message can't be forwarded
    [<JsonPropertyName("has_protected_content")>]
    HasProtectedContent: bool option

    /// The unique identifier of a media message group this message belongs to
    [<JsonPropertyName("media_group_id")>]
    MediaGroupId: string option

    /// Signature of the post author for messages in channels, or the custom title of an anonymous group administrator
    [<JsonPropertyName("author_signature")>]
    AuthorSignature: string option

    /// For text messages, the actual UTF-8 text of the message
    [<JsonPropertyName("text")>]
    Text: string option

    /// For text messages, special entities like usernames, URLs, bot commands, etc. that appear in the text
    [<JsonPropertyName("entities")>]
    Entities: MessageEntity[] option

    /// Message is an animation, information about the animation. For backward compatibility, when this field is set, the document field will also be set
    [<JsonPropertyName("animation")>]

    Animation: Animation option
    /// Message is an audio file, information about the file
    [<JsonPropertyName("audio")>]
    Audio: Audio option

    /// Message is a general file, information about the file
    [<JsonPropertyName("document")>]
    Document: Document option

    /// Message is a photo, available sizes of the photo
    [<JsonPropertyName("photo")>]
    Photo: PhotoSize[] option

    /// Message is a sticker, information about the sticker
    [<JsonPropertyName("sticker")>]
    Sticker: Sticker option

    // /// Message is a forwarded story
    // [<JsonPropertyName("story")>]
    // Story: Story option
    /// Message is a video, information about the video
    [<JsonPropertyName("video")>]
    Video: Video option

    // /// Message is a video note, information about the video message
    // [<JsonPropertyName("video_note")>]
    // VideoNote: VideoNote option
    /// Message is a voice message, information about the file
    [<JsonPropertyName("voice")>]
    Voice: Voice option

    /// Caption for the animation, audio, document, photo, video or voice
    [<JsonPropertyName("caption")>]
    Caption: string option

    /// For messages with a caption, special entities like usernames, URLs, bot commands, etc. that appear in the caption
    [<JsonPropertyName("caption_entities")>]
    CaptionEntities: MessageEntity[] option

    /// True, if the message media is covered by a spoiler animation
    [<JsonPropertyName("has_media_spoiler")>]
    HasMediaSpoiler: bool option

    // /// Message is a shared contact, information about the contact
    // [<JsonPropertyName("contact")>]
    // Contact: Contact option

    // /// Message is a dice with random value
    // [<JsonPropertyName("dice")>]
    // Dice: Dice option

    // /// Message is a game, information about the game. More about games »
    // [<JsonPropertyName("game")>]
    // Game: Game option

    // /// Message is a native poll, information about the poll
    // [<JsonPropertyName("poll")>]
    // Poll: Poll option

    // /// Message is a venue, information about the venue. For backward compatibility, when this field is set, the location field will also be set
    // [<JsonPropertyName("venue")>]
    // Venue: Venue option

    // /// Message is a shared location, information about the location
    // [<JsonPropertyName("location")>]
    // Location: Location option

    /// New members that were added to the group or supergroup and information about them (the bot itself may be one of these members)
    [<JsonPropertyName("new_chat_members")>]
    NewChatMembers: User[] option

    /// A member was removed from the group, information about them (this member may be the bot itself)
    [<JsonPropertyName("left_chat_member")>]
    LeftChatMember: User option

    /// A chat title was changed to this value
    [<JsonPropertyName("new_chat_title")>]
    NewChatTitle: string option

    // /// A chat photo was change to this value
    // [<JsonPropertyName("new_chat_photo")>]
    // NewChatPhoto: PhotoSize[] option

    /// Service message: the chat photo was deleted
    [<JsonPropertyName("delete_chat_photo")>]
    DeleteChatPhoto: bool option

    /// Service message: the group has been created
    [<JsonPropertyName("group_chat_created")>]
    GroupChatCreated: bool option

    /// Service message: the supergroup has been created. This field can't be received in a message coming through updates, because bot can't be a member of a supergroup when it is created. It can only be found in reply_to_message if someone replies to a very first message in a directly created supergroup.
    [<JsonPropertyName("supergroup_chat_created")>]
    SupergroupChatCreated: bool option

    /// Service message: the channel has been created. This field can't be received in a message coming through updates, because bot can't be a member of a channel when it is created. It can only be found in reply_to_message if someone replies to a very first message in a channel.
    [<JsonPropertyName("channel_chat_created")>]
    ChannelChatCreated: bool option

    // /// Service message: auto-delete timer settings changed in the chat
    // [<JsonPropertyName("message_auto_delete_timer_changed")>]
    // MessageAutoDeleteTimerChanged: MessageAutoDeleteTimerChanged option

    /// The group has been migrated to a supergroup with the specified identifier. This number may have more than 32 significant bits and some programming languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer or double-precision float type are safe for storing this identifier.
    [<JsonPropertyName("migrate_to_chat_id")>]
    MigrateToChatId: int64 option

    /// The supergroup has been migrated from a group with the specified identifier. This number may have more than 32 significant bits and some programming languages may have difficulty/silent defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit integer or double-precision float type are safe for storing this identifier.
    [<JsonPropertyName("migrate_from_chat_id")>]
    MigrateFromChatId: int64 option

    /// Specified message was pinned. Note that the Message object in this field will not contain further reply_to_message fields even if it is itself a reply.
    [<JsonPropertyName("pinned_message")>]
    PinnedMessage: Message option

    // /// Message is an invoice for a payment, information about the invoice. More about payments »
    // [<JsonPropertyName("invoice")>]
    // Invoice: Invoice option

    // /// Message is a service message about a successful payment, information about the payment. More about payments »
    // [<JsonPropertyName("successful_payment")>]
    // SuccessfulPayment: SuccessfulPayment option

    // /// Service message: a user was shared with the bot
    // [<JsonPropertyName("user_shared")>]
    // UserShared: UserShared option

    // /// Service message: a chat was shared with the bot
    // [<JsonPropertyName("chat_shared")>]
    // ChatShared: ChatShared option

    /// The domain name of the website on which the user has logged in. More about Telegram Login »
    [<JsonPropertyName("connected_website")>]
    ConnectedWebsite: string option

    // /// Service message: the user allowed the bot to write messages after adding it to the attachment or side menu,
    // launching a Web App from a link, or accepting an explicit request from a Web App sent by the method
    // requestWriteAccess
    // [<JsonPropertyName("write_access_allowed")>]
    // WriteAccessAllowed: WriteAccessAllowed option

    // /// Telegram Passport data
    // [<JsonPropertyName("passport_data")>]
    // PassportData: PassportData option

    // /// Service message. A user in the chat triggered another user's proximity alert while sharing Live Location.
    // [<JsonPropertyName("proximity_alert_triggered")>]
    // ProximityAlertTriggered: ProximityAlertTriggered option

    // /// Service message: forum topic created
    // [<JsonPropertyName("forum_topic_created")>]
    // ForumTopicCreated: ForumTopicCreated option

    // /// Service message: forum topic edited
    // [<JsonPropertyName("forum_topic_edited")>]
    // ForumTopicEdited: ForumTopicEdited option

    // /// Service message: forum topic closed
    // [<JsonPropertyName("forum_topic_closed")>]
    // ForumTopicClosed: ForumTopicClosed option

    // /// Service message: forum topic reopened
    // [<JsonPropertyName("forum_topic_reopened")>]
    // ForumTopicReopened: ForumTopicReopened option

    // /// Service message: the 'General' forum topic hidden
    // [<JsonPropertyName("general_forum_topic_hidden")>]
    // GeneralForumTopicHidden: GeneralForumTopicHidden option

    // /// Service message: the 'General' forum topic unhidden
    // [<JsonPropertyName("general_forum_topic_unhidden")>]
    // GeneralForumTopicUnhidden: GeneralForumTopicUnhidden option

    // /// Service message: video chat scheduled
    // [<JsonPropertyName("video_chat_scheduled")>]
    // VideoChatScheduled: VideoChatScheduled option

    // /// Service message: video chat started
    // [<JsonPropertyName("video_chat_started")>]
    // VideoChatStarted: VideoChatStarted option

    // /// Service message: video chat ended
    // [<JsonPropertyName("video_chat_ended")>]
    // VideoChatEnded: VideoChatEnded option

    // /// Service message: new participants invited to a video chat
    // [<JsonPropertyName("video_chat_participants_invited")>]
    // VideoChatParticipantsInvited: VideoChatParticipantsInvited option

    // /// Service message: data sent by a Web App
    // [<JsonPropertyName("web_app_data")>]
    // WebAppData: WebAppData option

    /// Inline keyboard attached to the message. login_url buttons are represented as ordinary url buttons.
    [<JsonPropertyName("reply_markup")>]
    ReplyMarkup: InlineKeyboardMarkup option
}

/// This object represents an audio file to be treated as music by the Telegram clients.
and [<CLIMutable>] Audio = {
    /// Identifier for this file, which can be used to download or reuse the file
    [<JsonPropertyName("file_id")>]
    FileId: string

    /// Unique identifier for this file, which is supposed to be the same over time and for different bots.
    /// Can't be used to download or reuse the file.
    [<JsonPropertyName("file_unique_id")>]
    FileUniqueId: string

    /// Duration of the audio in seconds as defined by sender
    [<JsonPropertyName("duration")>]
    Duration: int64

    /// Performer of the audio as defined by sender or by audio tags
    [<JsonPropertyName("performer")>]
    Performer: string option

    /// Title of the audio as defined by sender or by audio tags
    [<JsonPropertyName("title")>]
    Title: string option

    /// Original filename as defined by sender
    [<JsonPropertyName("file_name")>]
    FileName: string option

    /// MIME type of the file as defined by sender
    [<JsonPropertyName("mime_type")>]
    MimeType: string option

    /// File size in bytes. It can be bigger than 2^31 and some programming languages may have difficulty/silent
    /// defects in interpreting it. But it has at most 52 significant bits, so a signed 64-bit
    /// integer or double-precision float type are safe for storing this value.
    [<JsonPropertyName("file_size")>]
    FileSize: int64 option

    /// Thumbnail of the album cover to which the music file belongs
    [<JsonPropertyName("thumbnail")>]
    Thumbnail: PhotoSize option
}

/// This object represents type of a poll, which is allowed to be created and sent when the
/// corresponding button is pressed.
and [<CLIMutable>] KeyboardButtonPollType = {
    /// If quiz is passed, the user will be allowed to create only polls in the quiz mode.
    /// If regular is passed, only regular polls will be allowed. Otherwise, the user will be allowed
    /// to create a poll of any type.
    [<JsonPropertyName("type")>]
    Type: string option
}

/// This object defines the criteria used to request a suitable user. The identifier of the selected user will be
/// shared with the bot when the corresponding button is pressed. More about requesting users »
and [<CLIMutable>] KeyboardButtonRequestUser = {
    /// Signed 32-bit identifier of the request, which will be received back in the UserShared object.
    /// Must be unique within the message
    [<JsonPropertyName("request_id")>]
    RequestId: int64

    /// Pass True to request a bot, pass False to request a regular user.
    /// If not specified, no additional restrictions are applied.
    [<JsonPropertyName("user_is_bot")>]
    UserIsBot: bool option

    /// Pass True to request a premium user, pass False to request a non-premium user.
    /// If not specified, no additional restrictions are applied.
    [<JsonPropertyName("user_is_premium")>]
    UserIsPremium: bool option
}

/// This object defines the criteria used to request a suitable chat. The identifier of the selected chat will
/// be shared with the bot when the corresponding button is pressed. More about requesting chats »
and [<CLIMutable>] KeyboardButtonRequestChat = {
    /// Signed 32-bit identifier of the request, which will be received back in the ChatShared object.
    /// Must be unique within the message
    [<JsonPropertyName("request_id")>]
    RequestId: int64

    /// Pass True to request a channel chat, pass False to request a group or a supergroup chat.
    [<JsonPropertyName("chat_is_channel")>]
    ChatIsChannel: bool

    /// Pass True to request a forum supergroup, pass False to request a non-forum chat. If not specified,
    /// no additional restrictions are applied.
    [<JsonPropertyName("chat_is_forum")>]
    ChatIsForum: bool option

    /// Pass True to request a supergroup or a channel with a username, pass False to request a chat without a username.
    /// If not specified, no additional restrictions are applied.
    [<JsonPropertyName("chat_has_username")>]
    ChatHasUsername: bool option

    /// Pass True to request a chat owned by the user. Otherwise, no additional restrictions are applied.
    [<JsonPropertyName("chat_is_created")>]
    ChatIsCreated: bool option

    /// A JSON-serialized object listing the required administrator rights of the user in the chat.
    /// The rights must be a superset of bot_administrator_rights. If not specified,
    /// no additional restrictions are applied.
    [<JsonPropertyName("user_administrator_rights")>]
    UserAdministratorRights: ChatAdministratorRights option

    /// A JSON-serialized object listing the required administrator rights of the bot in the chat.
    /// The rights must be a subset of user_administrator_rights. If not specified,
    /// no additional restrictions are applied.
    [<JsonPropertyName("bot_administrator_rights")>]
    BotAdministratorRights: ChatAdministratorRights option

    /// Pass True to request a chat with the bot as a member. Otherwise, no additional restrictions are applied.
    [<JsonPropertyName("bot_is_member")>]
    BotIsMember: bool option
}

/// Represents the rights of an administrator in a chat.
and [<CLIMutable>] ChatAdministratorRights = {
    /// True, if the user's presence in the chat is hidden
    [<JsonPropertyName("is_anonymous")>]
    IsAnonymous: bool

    /// True, if the administrator can access the chat event log, boost list in channels, see channel members,
    /// report spam messages, see anonymous administrators in supergroups and ignore slow mode.
    /// Implied by any other administrator privilege
    [<JsonPropertyName("can_manage_chat")>]
    CanManageChat: bool

    /// True, if the administrator can delete messages of other users
    [<JsonPropertyName("can_delete_messages")>]
    CanDeleteMessages: bool

    /// True, if the administrator can manage video chats
    [<JsonPropertyName("can_manage_video_chats")>]
    CanManageVideoChats: bool

    /// True, if the administrator can restrict, ban or unban chat members, or access supergroup statistics
    [<JsonPropertyName("can_restrict_members")>]
    CanRestrictMembers: bool

    /// True, if the administrator can add new administrators with a subset of their own privileges or demote
    /// administrators that they have promoted, directly or indirectly (promoted by administrators that
    /// were appointed by the user)
    [<JsonPropertyName("can_promote_members")>]
    CanPromoteMembers: bool

    /// True, if the user is allowed to change the chat title, photo and other settings
    [<JsonPropertyName("can_change_info")>]
    CanChangeInfo: bool

    /// True, if the user is allowed to invite new users to the chat
    [<JsonPropertyName("can_invite_users")>]
    CanInviteUsers: bool

    /// True, if the administrator can post messages in the channel, or access channel statistics; channels only
    [<JsonPropertyName("can_post_messages")>]
    CanPostMessages: bool option

    /// True, if the administrator can edit messages of other users and can pin messages; channels only
    [<JsonPropertyName("can_edit_messages")>]
    CanEditMessages: bool option

    /// True, if the user is allowed to pin messages; groups and supergroups only
    [<JsonPropertyName("can_pin_messages")>]
    CanPinMessages: bool option

    /// True, if the administrator can post stories in the channel; channels only
    [<JsonPropertyName("can_post_stories")>]
    CanPostStories: bool option

    /// True, if the administrator can edit stories posted by other users; channels only
    [<JsonPropertyName("can_edit_stories")>]
    CanEditStories: bool option

    /// True, if the administrator can delete stories posted by other users; channels only
    [<JsonPropertyName("can_delete_stories")>]
    CanDeleteStories: bool option

    /// True, if the user is allowed to create, rename, close, and reopen forum topics; supergroups only
    [<JsonPropertyName("can_manage_topics")>]
    CanManageTopics: bool option

    /// DEPRECATED: use can_manage_video_chats instead
    [<JsonPropertyName("can_manage_voice_chats")>]
    CanManageVoiceChats: bool option
}

/// Upon receiving a message with this object, Telegram clients will remove the current custom keyboard
/// and display the default letter-keyboard. By default, custom keyboards are displayed until a new keyboard
/// is sent by a bot. An exception is made for one-time keyboards that are hidden immediately after the user
/// presses a button (see ReplyKeyboardMarkup).
and [<CLIMutable>] ReplyKeyboardRemove =
    {
        /// Requests clients to remove the custom keyboard (user will not be able to summon this keyboard;
        /// if you want to hide the keyboard from sight but keep it accessible, use one_time_keyboard
        /// in ReplyKeyboardMarkup)
        [<JsonPropertyName("remove_keyboard")>]
        RemoveKeyboard: bool
        /// Use this parameter if you want to remove the keyboard for specific users only. Targets:
        /// 1) users that are @mentioned in the text of the Message object; 2) if the bot's message is a
        /// reply (has reply_to_message_id), sender of the original message.
        ///
        /// Example: A user votes in a poll, bot returns confirmation message in reply to the vote and
        /// removes the keyboard for that user, while still showing the keyboard with poll options to users who haven't voted yet.
        [<JsonPropertyName("selective")>]
        Selective: bool option
    }

/// Upon receiving a message with this object, Telegram clients will display a reply interface to the user
/// (act as if the user has selected the bot's message and tapped 'Reply'). This can be extremely useful
/// if you want to create user-friendly step-by-step interfaces without having to sacrifice privacy mode.
and [<CLIMutable>] ForceReply =
    {
        /// Shows reply interface to the user, as if they manually selected the bot's message and tapped 'Reply'
        [<JsonPropertyName("force_reply")>]
        ForceReply: bool
        /// The placeholder to be shown in the input field when the reply is active; 1-64 characters
        [<JsonPropertyName("input_field_placeholder")>]
        InputFieldPlaceholder: string option
        /// Use this parameter if you want to force reply from specific users only. Targets: 1) users that are
        /// @mentioned in the text of the Message object; 2) if the bot's message is a reply
        /// (has reply_to_message_id), sender of the original message.
        [<JsonPropertyName("selective")>]
        Selective: bool option
    }

/// This object represents an incoming update.
/// At most one of the optional parameters can be present in any given update.
and [<CLIMutable>] Update = {
    /// The update's unique identifier. Update identifiers start from a certain positive number and increase
    /// sequentially. This ID becomes especially handy if you're using webhooks, since it allows you to ignore
    /// repeated updates or to restore the correct update sequence, should they get out of order.
    /// If there are no new updates for at least a week, then identifier of the next update will
    /// be chosen randomly instead of sequentially.
    [<JsonPropertyName("update_id")>]
    UpdateId: int64

    /// New incoming message of any kind - text, photo, sticker, etc.
    [<JsonPropertyName("message")>]
    Message: Message option

    /// New version of a message that is known to the bot and was edited
    [<JsonPropertyName("edited_message")>]
    EditedMessage: Message option

    /// New incoming channel post of any kind - text, photo, sticker, etc.
    [<JsonPropertyName("channel_post")>]
    ChannelPost: Message option

    /// New version of a channel post that is known to the bot and was edited
    [<JsonPropertyName("edited_channel_post")>]
    EditedChannelPost: Message option
    // /// New incoming inline query
    // [<JsonPropertyName("inline_query")>]
    // InlineQuery: InlineQuery option

    // /// The result of an inline query that was chosen by a user and sent to their chat partner.
    // Please see our documentation on the feedback collecting for details
    // on how to enable these updates for your bot.
    // [<JsonPropertyName("chosen_inline_result")>]
    // ChosenInlineResult: ChosenInlineResult option

    // /// New incoming callback query
    // [<JsonPropertyName("callback_query")>]
    // CallbackQuery: CallbackQuery option

    // /// New incoming shipping query. Only for invoices with flexible price
    // [<JsonPropertyName("shipping_query")>]
    // ShippingQuery: ShippingQuery option

    // /// New incoming pre-checkout query. Contains full information about checkout
    // [<JsonPropertyName("pre_checkout_query")>]
    // PreCheckoutQuery: PreCheckoutQuery option

    // /// New poll state. Bots receive only updates about stopped polls and polls, which are sent by the bot
    // [<JsonPropertyName("poll")>]
    // Poll: Poll option

    // /// A user changed their answer in a non-anonymous poll. Bots receive new votes only in polls that
    // were sent by the bot itself.
    // [<JsonPropertyName("poll_answer")>]
    // PollAnswer: PollAnswer option

    // /// The bot's chat member status was updated in a chat. For private chats, this update is received only when
    // the bot is blocked or unblocked by the user.
    // [<JsonPropertyName("my_chat_member")>]
    // MyChatMember: ChatMemberUpdated option

    // /// A chat member's status was updated in a chat. The bot must be an administrator in the chat and must explicitly
    // specify "chat_member" in the list of allowed_updates to receive these updates.
    // [<JsonPropertyName("chat_member")>]
    // ChatMember: ChatMemberUpdated option

    // /// A request to join the chat has been sent. The bot must have the can_invite_users administrator right in the
    // chat to receive these updates.
    // [<JsonPropertyName("chat_join_request")>]
    // ChatJoinRequest: ChatJoinRequest option
}

and Markup =
    | InlineKeyboardMarkup of InlineKeyboardMarkup
    | ReplyKeyboardMarkup of ReplyKeyboardMarkup
    | ReplyKeyboardRemove of ReplyKeyboardRemove
    | ForceReply of ForceReply

/// This object represents a bot command.
and [<CLIMutable>] BotCommand = {
    /// Text of the command; 1-32 characters. Can contain only lowercase English letters, digits and underscores.
    [<DataMember(Name = "command")>]
    Command: string

    /// Description of the command; 1-256 characters.
    [<DataMember(Name = "description")>]
    Description: string
}

with static member Create(command: string, description: string) = {
      Command = command
      Description = description
}


/// Represents the default scope of bot commands. Default commands are used if no commands
/// with a narrower scope are specified for the user.
and [<CLIMutable>] BotCommandScopeDefault = {
    /// Scope type, must be default
    [<DataMember(Name = "type")>]
    Type: string
}
with static member Create(``type``: string) = {
        Type = ``type``
}

/// Represents the scope of bot commands, covering all private chats.
and [<CLIMutable>] BotCommandScopeAllPrivateChats =
    {
        /// Scope type, must be all_private_chats
        [<DataMember(Name = "type")>]
        Type: string
    }

    static member Create(``type``: string) = { Type = ``type`` }

/// Represents the scope of bot commands, covering all group and supergroup chats.
and [<CLIMutable>] BotCommandScopeAllGroupChats =
    {
        /// Scope type, must be all_group_chats
        [<DataMember(Name = "type")>]
        Type: string
    }

    static member Create(``type``: string) = { Type = ``type`` }

/// Represents the scope of bot commands, covering all group and supergroup chat administrators.
and [<CLIMutable>] BotCommandScopeAllChatAdministrators =
    {
        /// Scope type, must be all_chat_administrators
        [<DataMember(Name = "type")>]
        Type: string
    }

    static member Create(``type``: string) = { Type = ``type`` }

/// Represents the scope of bot commands, covering a specific chat.
and [<CLIMutable>] BotCommandScopeChat =
    {
        /// Scope type, must be chat
        [<DataMember(Name = "type")>]
        Type: string
        /// Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)
        [<DataMember(Name = "chat_id")>]
        ChatId: ChatId
    }

    static member Create(``type``: string, chatId: ChatId) = { Type = ``type``; ChatId = chatId }

/// Represents the scope of bot commands, covering all administrators of a specific group or supergroup chat.
and [<CLIMutable>] BotCommandScopeChatAdministrators = {
    /// Scope type, must be chat_administrators
    [<DataMember(Name = "type")>]
    Type: string
    /// Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)
    [<DataMember(Name = "chat_id")>]
    ChatId: ChatId
}
with static member Create(``type``: string, chatId: ChatId) = {
        Type = ``type``
        ChatId = chatId
}

/// Represents the scope of bot commands, covering a specific member of a group or supergroup chat.
and [<CLIMutable>] BotCommandScopeChatMember = {
    /// Scope type, must be chat_member
    [<DataMember(Name = "type")>]
    Type: string
    /// Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)
    [<DataMember(Name = "chat_id")>]
    ChatId: ChatId
    /// Unique identifier of the target user
    [<DataMember(Name = "user_id")>]
    UserId: int64
}
with static member Create(``type``: string, chatId: ChatId, userId: int64) = {
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
