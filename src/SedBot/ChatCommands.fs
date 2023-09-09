module SedBot.ChatCommands

open System.Text.RegularExpressions
open Funogram.Telegram
open Funogram.Telegram.Types

open SedBot.Common
open SedBot.Utilities

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

module CommandParser =
    type CommandPipelineItem = {
        Message: FunogramMessage
        BotUsername: string
        Command: CommandType voption
    } with

        member item.SetCommand(command: CommandType) =
            { item with Command = ValueSome command }

        static member Create(message, botUsername) = {
            Message = message
            BotUsername = botUsername
            Command = ValueNone
        }

        static member GetCommand(item: CommandPipelineItem) =
            match item.Command with
            | ValueSome command ->
                command
            | ValueNone ->
                CommandType.Nope

    let rec commandPatternInternal botUsername (text: string) chatType =
        let tryGetArgs rawArgs =
            let matched = Regex.Matches(rawArgs, """(?:(['"])(.*?)(?<!\\)(?>\\\\)*\1|([^\s]+))""")
            if Seq.isEmpty matched then
                None
            else
                matched |> Seq.map (fun m -> m.Value) |> Array.ofSeq |> Some

        if text.StartsWith "t!" then
            match text.Split " " |> List.ofArray with
            | [ command ] ->
                Some (command.Substring(2)), None
            | head :: args ->
                Some (head.Substring(2)), tryGetArgs (args |> String.concat " ")
            | _ ->
                None, None
        elif chatType = SuperGroup then
            let command = Regex.Match(text, "(\/)(.*?)((@" + botUsername + " (\*.))|(@" + botUsername + "))")
            if command.Length > 0 then
                Some (command.Value.Substring(1, command.Value.Length - botUsername.Length - 2)), tryGetArgs (text.Substring(command.Value.Length))
            else
                None, None
        else
            let command = Regex.Match(text, "(\/.*?)")
            if command.Length > 0 && command.Value <> "/" then
                Some (command.Value.Substring(1, command.Value.Length - 1)), tryGetArgs (text.Substring(command.Value.Length))
            else
                commandPatternInternal botUsername text SuperGroup

    let (|Command|) (item: CommandPipelineItem) =
        match item.Message with
        | { Text = Some text; Chat = { Type = cType } } ->
            commandPatternInternal (item.BotUsername.Substring(1)) text cType
        | _ ->
            None, None

    let (|%>) (item: CommandPipelineItem) (messageProcessor: CommandPipelineItem -> CommandPipelineItem) =
        match item.Command with
        | ValueNone -> messageProcessor item
        | _ -> item

    let private handleSed (item: CommandPipelineItem) : CommandPipelineItem =
        let tryGetValidExpression (expression: string) =
            let isValidExpression expression =
                Process.getStatusCode "sed" [| "-E"; expression |] "data" = 0

            if isValidExpression expression then
                    Some expression
                else
                    None

        match item.Message with
        | { MessageId = srcMsgId
            Chat = { Id = chatId }
            Text = Some expression
            ReplyToMessage = Some {
                Text = text
                MessageId = msgId
                Caption = caption
            }
          } ->
            let expression = tryGetValidExpression expression
            match expression with
            | Some expression ->
                let text = Option.anyOf2 text caption

                match text with
                | Some text ->
                    let res = CommandType.Sed({
                        TelegramOmniMessageId = (chatId, msgId)
                        SrcMsgId = srcMsgId
                        Expression = expression
                        Text = text
                    })
                    item.SetCommand(res)
                | None -> item
            | _ -> item
        | _ -> item

    let private handleRawMessageInfo (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { MessageId = msgId
            Chat = { Id = chatId }
            ReplyToMessage = Some replyToMessage }, Command (Some "raw", _)
         ->
            let res = CommandType.RawMessageInfo({
                TelegramOmniMessageId = (chatId, msgId)
                ReplyTo = replyToMessage
            })
            item.SetCommand(res)
        | _ ->
            item

    let private handleReverse (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "rev", _) ->
            let res = CommandType.Reverse({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Gif
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "rev", _) ->
            let res = CommandType.Reverse({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Video
            })
            item.SetCommand(res)

        | _ ->
            item

    let private handleVerticalFlip (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "vflip", _) ->
            let res = CommandType.VerticalFlip({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Gif
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "vflip", _) ->
            let res = CommandType.VerticalFlip({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Video
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "vflip", _) ->
            let fileId =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head
                |> fun p -> p.FileId

            let res =
                CommandType.VerticalFlip({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Picture
                })

            item.SetCommand(res)
        | _ -> item

    let private handleHorizontalFlip (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "hflip", _)
            ->
            let res = CommandType.HorizontalFlip({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Gif
                })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "hflip", _)
            ->
            let res = CommandType.HorizontalFlip({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Video
                })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "hflip", _)
            ->
            let fileId =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head
                |> fun p -> p.FileId

            let res =
                CommandType.HorizontalFlip({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Picture
                })

            item.SetCommand(res)
        | _ -> item

    let private handleClockwiseRotation (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "clock", _) ->
            let res = CommandType.ClockwiseRotation({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Gif
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "clock", _) ->
            let res = CommandType.ClockwiseRotation({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Video
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "clock", _) ->
            let fileId =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head
                |> fun p -> p.FileId

            let res = CommandType.ClockwiseRotation({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Picture
            })

            item.SetCommand(res)
        | _ ->
            item

    let private handleCounterclockwiseRotation (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some {
                MessageId = msgId
                Document = Some {
                    MimeType = Some "video/mp4"
                    FileId = fileId
                }
            } }, Command (Some "cclock", _) ->
            let res = CommandType.CounterClockwiseRotation({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Gif
                })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "cclock", _) ->
            let res =
                CommandType.CounterClockwiseRotation({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Video
                })

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId; Photo = Some photos } } , Command (Some "cclock", _)  ->
            let fileId =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head
                |> fun p -> p.FileId

            let res = CommandType.CounterClockwiseRotation({
                TelegramOmniMessageId = chatId, msgId
                File = fileId, FileType.Picture
            })

            item.SetCommand(res)
        | _ -> item

    let private handleDistortion (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Audio = Some { FileId = fileId } } }, Command (Some "dist", _) ->
            let res = CommandType.Distortion({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Audio
                })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Voice = Some { MimeType = Some "audio/ogg"
                                                   FileId = fileId } } }, Command (Some "dist", _) ->
            let res = CommandType.Distortion({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Voice
                })

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "dist", _)
            ->
            let res = CommandType.Distortion({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Gif
                })

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "dist", _) ->
            let res = CommandType.Distortion({
                    TelegramOmniMessageId = (chatId, msgId)
                    File = fileId, FileType.Video
                })

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "dist", _) ->
            let fileId =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head
                |> fun p -> p.FileId

            let res = CommandType.Distortion({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Picture
            })

            item.SetCommand(res)
        | _ -> item

    let private handleClown (item: CommandPipelineItem) =
        match item.Message with
        | { Chat = { Id = chatId }
            Text = Some command } when command.Contains("🤡") ->
            let res = CommandType.Clown({
                ChatId = chatId
                Count = String.getCountOfOccurrences command "🤡"
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            Caption = Some command } when command.Contains("🤡") ->
            let res = CommandType.Clown({
                ChatId = chatId
                Count = String.getCountOfOccurrences command "🤡"
            })
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            Sticker = Some { Emoji = Some emoji } } when emoji.Contains("🤡") ->
            let res = CommandType.Clown({ ChatId = chatId; Count = 1 })
            item.SetCommand(res)
        | _ -> item

    let private handleJq (item: CommandPipelineItem) =
        match item.Message, item with
        | { Chat = { Id = chatId }
            MessageId = msgId
            ReplyToMessage = Some { Text = Some data } }, Command (Some "jq", Some args) ->
            let res =
                CommandType.Jq({
                    TelegramOmniMessageId = (chatId, msgId)
                    Text = data
                    Expression = args |> String.concat " "
                })

            item.SetCommand(res)
        | _ ->
            item

    let processMessage message botUsername =
        CommandPipelineItem.Create(message, botUsername)
        |%> handleSed
        |%> handleJq
        |%> handleClown
        |%> handleRawMessageInfo
        |%> handleReverse
        |%> handleDistortion
        |%> handleVerticalFlip
        |%> handleHorizontalFlip
        |%> handleClockwiseRotation
        |%> handleCounterclockwiseRotation
        |> CommandPipelineItem.GetCommand
