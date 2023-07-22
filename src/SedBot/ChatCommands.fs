module SedBot.ChatCommands

open System.Text.RegularExpressions
open Funogram.Telegram
open Funogram.Telegram.Types
open NUnit.Framework
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
    | CounterClockwiseRotation of message: SourceMessage * file: SourceFile
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

    let rec commandPatternInternal botUsername (text: string) chatType =
        let tryGetArgs rawArgs =
            let matched = Regex.Matches(rawArgs, """(?:(['"])(.*?)(?<!\\)(?>\\\\)*\1|([^\s]+))""")
            if matched.Count = 0 then
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

            if expression.StartsWith("t") then
                let expression = $"s{expression.Substring(1)}"
                if isValidExpression expression then
                    Some expression
                else
                    None
            else
                None

        match item.Message with
        | { MessageId = srcMsgId
            Chat = { Id = chatId }
            Text = Some expression
            ReplyToMessage = Some { Text = text
                                    MessageId = msgId
                                    Caption = caption } } ->
            let expression = tryGetValidExpression expression
            match expression with
            | Some expression ->
                let text = Option.anyOf2 text caption

                match text with
                | Some text ->
                    let res = CommandType.Sed((chatId, msgId), srcMsgId, expression, text)
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
            let res = CommandType.RawMessageInfo((chatId, msgId), replyToMessage)
            item.SetCommand(res)
        | _ -> item

    let private handleReverse (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "rev", _) ->
            let res = CommandType.Reverse((chatId, msgId), (fileId, FileType.Gif))
            item.SetCommand(res)
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "rev", _) ->
            let res = CommandType.Reverse((chatId, msgId), (fileId, FileType.Video))
            item.SetCommand(res)
        | _ -> item

    let private handleVerticalFlip (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "vflip", _) ->
            let res = CommandType.VerticalFlip((chatId, msgId), (fileId, FileType.Gif))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "vflip", _) ->
            let res = CommandType.VerticalFlip((chatId, msgId), (fileId, FileType.Video))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "vflip", _) ->
            let photo =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head

            let res =
                CommandType.VerticalFlip((chatId, msgId), (photo.FileId, FileType.Picture))

            item.SetCommand(res)
        | _ -> item

    let private handleHorizontalFlip (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "hflip", _)
            ->
            let res = CommandType.HorizontalFlip((chatId, msgId), (fileId, FileType.Gif))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "hflip", _)
            ->
            let res = CommandType.HorizontalFlip((chatId, msgId), (fileId, FileType.Video))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "hflip", _)
            ->
            let photo =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head

            let res =
                CommandType.HorizontalFlip((chatId, msgId), (photo.FileId, FileType.Picture))

            item.SetCommand(res)
        | _ -> item

    let private handleClockwiseRotation (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "clock", _) ->
            let res = CommandType.ClockwiseRotation((chatId, msgId), (fileId, FileType.Gif))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "clock", _) ->
            let res = CommandType.ClockwiseRotation((chatId, msgId), (fileId, FileType.Video))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "clock", _) ->
            let photo =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head

            let res =
                CommandType.ClockwiseRotation((chatId, msgId), (photo.FileId, FileType.Picture))

            item.SetCommand(res)
        | _ -> item

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
            let res = CommandType.CounterClockwiseRotation((chatId, msgId), (fileId, FileType.Gif))
            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "cclock", _) ->
            let res =
                CommandType.CounterClockwiseRotation((chatId, msgId), (fileId, FileType.Video))

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId; Photo = Some photos } } , Command (Some "cclock", _)  ->
            let photo =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head

            let res =
                CommandType.CounterClockwiseRotation((chatId, msgId), (photo.FileId, FileType.Picture))

            item.SetCommand(res)
        | _ -> item

    let private handleDistortion (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Audio = Some { FileId = fileId } } }, Command (Some "dist", _) ->
            let res = CommandType.Distortion((chatId, msgId), (fileId, FileType.Audio))
            item.SetCommand(res)
        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Voice = Some { MimeType = Some "audio/ogg"
                                                   FileId = fileId } } }, Command (Some "dist", _) ->
            let res = CommandType.Distortion((chatId, msgId), (fileId, FileType.Voice))

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Document = Some { MimeType = Some "video/mp4"
                                                      FileId = fileId } } }, Command (Some "dist", _)
            ->
            let res = CommandType.Distortion((chatId, msgId), (fileId, FileType.Gif))

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Video = Some { FileId = fileId } } }, Command (Some "dist", _) ->
            let res = CommandType.Distortion((chatId, msgId), (fileId, FileType.Video))

            item.SetCommand(res)

        | { Chat = { Id = chatId }
            ReplyToMessage = Some { MessageId = msgId
                                    Photo = Some photos } }, Command (Some "dist", _) ->
            let photo =
                photos
                |> Array.sortBy It.Width
                |> Array.rev
                |> Array.head

            let res = CommandType.Distortion((chatId, msgId), (photo.FileId, FileType.Picture))

            item.SetCommand(res)
        | _ -> item

    let private handleClown (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message with
        | { Chat = { Id = chatId }
            Text = Some command } when command.Trim().Contains("🤡") ->
            let res = CommandType.Clown(chatId, command.Split("🤡").Length - 1)
            item.SetCommand(res)
        | { Chat = { Id = chatId }
            Caption = Some command } when command.Trim().Contains("🤡") ->
            let res = CommandType.Clown(chatId, command.Split("🤡").Length - 1)
            item.SetCommand(res)
        | { Chat = { Id = chatId }
            Sticker = Some { Emoji = Some emoji } } when emoji.Contains("🤡") ->
            let res = CommandType.Clown(chatId, 1)
            item.SetCommand(res)
        | _ -> item

    let private handleJq (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            MessageId = msgId
            ReplyToMessage = Some { Text = Some data } }, Command (Some "jq", Some args) ->
            let res =
                CommandType.Jq(
                    (chatId, msgId),
                    data,
                    args |> String.concat " "
                )

            item.SetCommand(res)
        | _ -> item

    let private handleUserId (item: CommandPipelineItem) : CommandPipelineItem =
        match item.Message, item with
        | { Chat = { Id = chatId }
            MessageId = msgId
            ReplyToMessage = Some { From = Some { Id = victimId } }}, Command (Some "9", _) ->
            let res = CommandType.UserId((chatId, msgId), victimId)
            item.SetCommand(res)
        | _ -> item

    let processMessage message botUsername =
        CommandPipelineItem.Create(message, botUsername)
        |%> handleSed
        |%> handleJq
        |%> handleClown
        |%> handleRawMessageInfo
        |%> handleUserId
        |%> handleReverse
        |%> handleDistortion
        |%> handleVerticalFlip
        |%> handleHorizontalFlip
        |%> handleClockwiseRotation
        |%> handleCounterclockwiseRotation
        |> CommandPipelineItem.GetCommand

    [<TestCase("sample_bot", "/send@sample_bot horny bonk", "supergroup", "send", "horny,bonk")>]
    [<TestCase("sample_bot", "/send@sample_bot horny bonk", "private", "send", "horny,bonk")>]
    [<TestCase("sample_bot", "t!send horny bonk", "private", "send", "horny,bonk")>]
    [<TestCase("sample_bot", "t!send", "private", "send", "")>]
    [<TestCase("sample_bot", "/send@sample_bot", "private", "send", "")>]
    [<TestCase("sample_bot", "/send@sample_bot", "private", "send", "")>]
    let [<Test>] ``Command handler works properly`` (botName, command, chatType, expectedCommand, expectedArgs) =
        let chatType =
            match chatType with
            | "supergroup" -> ChatType.SuperGroup
            | "unknown" -> ChatType.Unknown
            | "sender" -> ChatType.Sender
            | "private" -> ChatType.Private
            | "group" -> ChatType.Group
            | "channel" -> ChatType.Channel
            | s -> failwith $"Can't handle -> {s}"

        match commandPatternInternal botName command chatType with
        | Some command, Some args ->
            Assert.AreEqual(command, expectedCommand)
            Assert.AreEqual(expectedArgs, System.String.Join(",", args))
        | Some command, None ->
            Assert.AreEqual(command, expectedCommand)
            Assert.AreEqual(expectedArgs, "")
        | _ ->
            Assert.Fail("Broken")
