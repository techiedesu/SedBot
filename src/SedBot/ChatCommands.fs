module SedBot.ChatCommands

open Funogram.Telegram
open Funogram.Telegram.Types
open SedBot
open SedBot.Utilities

type FunogramMessage = Types.Message

type FileId = string
type ChatId = int64
type MessageId = int64

type CommandType =
    | Sed of message: SourceMessage * srcMsgId: int64 * expression: string * text: string
    | VerticalFlip of message: SourceMessage * file: SourceFile
    | HorizontalFlip of message: SourceMessage * file: SourceFile
    | ClockwiseRotation of message: SourceMessage * file: SourceFile
    | CounterclockwiseRotation of message: SourceMessage * file: SourceFile
    | Reverse of message: SourceMessage * file: SourceFile
    | Distortion of message: SourceMessage * file: SourceFile
    | Jq of message: SourceMessage * expression: string * text: string
    | Clown of chatId: int64 * count: int
    | RawMessageInfo of message: SourceMessage * replyTo: FunogramMessage
    | UserId of message: SourceMessage * victimUserId: int64
    | Nope

and SourceFile = FileId * FileType
and SourceMessage = ChatId * MessageId

module CommandParser =
    type CommandPipelineItem =
        { Message: FunogramMessage
          BotUsername: string
          Command: CommandType voption }

        member item.SetCommand(command: CommandType) =
            { item with Command = ValueSome command }

        static member Create(message, botUsername) =
            { Message = message
              BotUsername = botUsername
              Command = ValueNone }

        static member GetCommand(item: CommandPipelineItem) =
            match item.Command with
            | ValueSome command -> command
            | _ -> CommandType.Nope

    let (|%>) (item: CommandPipelineItem) (messageProcessor: CommandPipelineItem -> CommandPipelineItem) =
        match item.Command with
        | ValueNone -> messageProcessor item
        | _ -> item

    let private prefix mType prefix =
        if mType = SuperGroup then
            prefix
        else
            ""

    let private handleSed (item: CommandPipelineItem) : CommandPipelineItem =
        let isValidExpression expression =
            Process.getStatusCode "sed" [| "-E"; expression |] "data" = 0

        match item.Message with
        | { MessageId = srcMsgId
            Chat = { Id = chatId }
            Text = Some expression
            ReplyToMessage = Some { Text = text
                                    MessageId = msgId
                                    Caption = caption } } when isValidExpression expression ->
            let text = Option.anyOf2 text caption

            match text with
            | Some text ->
                let res = CommandType.Sed((chatId, msgId), srcMsgId, expression, text)
                item.SetCommand(res)
            | None -> item
        | _ -> item

    let private handleRawMessageInfo (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message with
        | { MessageId = msgId
            Chat = { Id = chatId; Type = cType }
            Text = Some text
            ReplyToMessage = Some replyToMessage } when
            text
                .Trim()
                .AnyOf("t!info", $"/info{prefix cType item.BotUsername}")
            ->
            let res = CommandType.RawMessageInfo((chatId, msgId), replyToMessage)
            item.SetCommand(res)
        | _ -> item

    let private handleReverse (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message with
        | { Chat = { Id = chatId; Type = cType }
            Text = Some command
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some mimeType
                                                      FileId = fileId } } } when
            mimeType = "video/mp4"
            && command
                .Trim()
                .AnyOf("t!rev", $"/rev{(prefix cType item.BotUsername)}")
            ->
            let res = CommandType.Reverse((chatId, msgId), (fileId, FileType.Gif))
            item.SetCommand(res)
        | { Chat = { Id = chatId; Type = cType }
            Text = Some command
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } } when
            command
                .Trim()
                .AnyOf("t!rev", $"/rev{(prefix cType item.BotUsername)}")
            ->
            let res = CommandType.Reverse((chatId, msgId), (fileId, FileType.Video))
            item.SetCommand(res)
        | _ -> item

    let processMessage message botUsername =
        CommandPipelineItem.Create(message, botUsername)
        |%> handleSed
        |%> handleRawMessageInfo
        |%> handleReverse
        |> CommandPipelineItem.GetCommand
