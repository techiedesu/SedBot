namespace SedBot.ChatCommands.Types

open System
open Funogram.Telegram
open Funogram.Telegram.Types
open SedBot.Common

type FunogramMessage = Types.Message

type FileId = string
type ChatId = int64
type MessageId = int64

type CommandType =
    | Sed of args: SedArgs
    | VerticalFlip of args: FileManipulationArgs
    | HorizontalFlip of args: FileManipulationArgs
    | ClockwiseRotation of args: FileManipulationArgs
    | CounterClockwiseRotation of args: FileManipulationArgs
    | Reverse of args: FileManipulationArgs
    | Distortion of args: FileManipulationArgs
    | Jq of args: JqArgs
    | Clown of args: ClownArgs
    | RawMessageInfo of args: RawMessageArgs
    | Nope

and FileManipulationArgs = {
    TelegramOmniMessageId: TelegramSourceOmniMessageId
    File: SourceFile
}
and SedArgs = {
    TelegramOmniMessageId: TelegramSourceOmniMessageId
    SrcMsgId: int64
    Expression: string
    Text: string
}
and JqArgs = {
    TelegramOmniMessageId: TelegramSourceOmniMessageId
    Expression: string
    Text: string
}
and ClownArgs = {
    ChatId: int64
    Count: int
}
and RawMessageArgs = {
    TelegramOmniMessageId: TelegramSourceOmniMessageId
    ReplyTo: FunogramMessage
}

and SourceFile = FileId * FileType

/// Global telegram identifier (ChatId + MessageId seems to be enough global)
and TelegramSourceOmniMessageId = ChatId * MessageId

type InlineCommandInfo = {
    Command: string
    Description: string
}

type CommandPipelineItem = {
    Message: FunogramMessage
    BotUsername: string
    Command: CommandType voption
    ProvideInlineHelp: bool
    CommandHelpInfo: InlineCommandInfo list
} with

    member item.SetCommand(command: CommandType) =
        { item with Command = ValueSome command }

    static member Create(message, botUsername, inlineHelp) = {
        Message = message
        BotUsername = botUsername
        Command = ValueNone
        ProvideInlineHelp = inlineHelp
        CommandHelpInfo = []
    }

    static member GetCommand(item: CommandPipelineItem) =
        match item.Command with
        | ValueNone ->
            CommandType.Nope
        | ValueSome command ->
            command

module Ext =
    let emptyTelegramMessage : Types.Message = {
         MessageId = 0
         MessageThreadId = None
         From = None
         SenderChat = None
         Date = DateTime.MinValue
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
