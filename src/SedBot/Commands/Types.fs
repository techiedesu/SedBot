namespace SedBot.ChatCommands.Types

open SedBot.Telegram.BotApi.Types
open SedBot.Telegram.Types.Extensions

type FileId = string
type ChatId = int64
type MessageId = int64

type CommandType =
    | Sed of args: SedArgs
    | Awk of args: AwkArgs
    | Zov of args: ZovArgs
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

and FileManipulationArgs =
    { TelegramOmniMessageId: TelegramSourceOmniMessageId
      File: SourceFile }

and SedArgs =
    { TelegramOmniMessageId: TelegramSourceOmniMessageId
      SrcMsgId: int64
      Expression: string
      Text: string }

and AwkArgs =
    { TelegramOmniMessageId: TelegramSourceOmniMessageId
      SrcMsgId: int64
      Expression: string
      Multiline: bool
      Text: string }

and ZovArgs =
    { TelegramOmniMessageId: TelegramSourceOmniMessageId
      SrcMsgId: int64
      Text: string }

and JqArgs =
    { TelegramOmniMessageId: TelegramSourceOmniMessageId
      Expression: string
      Text: string }

and ClownArgs = { ChatId: int64; Count: int }

and RawMessageArgs =
    { TelegramOmniMessageId: TelegramSourceOmniMessageId
      ReplyTo: Message }

and SourceFile = FileId * FileType

/// Global telegram identifier (ChatId + MessageId seems to be enough global)
and TelegramSourceOmniMessageId = ChatId * MessageId

type InlineCommandInfo =
    { Command: string; Description: string }

type CommandPipelineItem =
    { Message: Message
      BotUsername: string
      Command: CommandType voption
      ProvideInlineHelp: bool
      CommandHelpInfo: InlineCommandInfo list }

    member item.SetCommand(command: CommandType) =
        { item with
            Command = ValueSome command }

    static member Create(message, botUsername, inlineHelp) =
        { Message = message
          BotUsername = botUsername
          Command = ValueNone
          ProvideInlineHelp = inlineHelp
          CommandHelpInfo = [] }

    static member GetCommand(item: CommandPipelineItem) =
        match item.Command with
        | ValueNone -> CommandType.Nope
        | ValueSome command -> command
