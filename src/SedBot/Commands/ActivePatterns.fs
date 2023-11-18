module SedBot.Commands.ActivePatterns

open System.Text.RegularExpressions
open Funogram.Telegram.Types

open SedBot.ChatCommands.Types
open SedBot.Common

module [<RequireQualifiedAccess>] CommandPipelineItem =
    let set (item: CommandPipelineItem) command =
        item.SetCommand(command)

let rec public commandPatternInternal botUsername (text: string) chatType =
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
            Some (command.Value.Substring(1, command.Value.Length - botUsername.Length - 2)), tryGetArgs (text.Substring(command.Value |> String.length))
        else
            None, None
    else
        let command = Regex.Match(text, "(\/.*?)")
        if command.Length > 0 && command.Value <> "/" then
            Some (command.Value.Substring(1, command.Value.Length - 1)), tryGetArgs (text.Substring(command.Value.Length))
        else
            commandPatternInternal botUsername text SuperGroup

let (|CommandWithArgs|) (item: CommandPipelineItem) =
    match item.Message with
    | { Text = Some text; Chat = { Type = cType } } ->
        commandPatternInternal (item.BotUsername.Substring(1)) text cType
    | _ ->
        None, None

let (|Command|) (item: CommandPipelineItem) =
    match item.Message with
    | { Text = Some text; Chat = { Type = cType } } ->
        commandPatternInternal (item.BotUsername.Substring(1)) text cType |> fst
    | _ ->
        None

let (|Message|) (item: CommandPipelineItem) =
    item.Message

let (|ReplyMessage|) (item: CommandPipelineItem) =
    item.Message.ReplyToMessage

let (|ReplyDocument|) (item: CommandPipelineItem) =
    match item with
    | ReplyMessage (Some { Document = document }) -> document
    | _ -> None

let (|WhenReplyDocumentMimeTypeIs|_|) (mimeType: string) (item: CommandPipelineItem) =
    match item with
    | ReplyMessage (Some { Document = Some { MimeType = Some mimeType' } }) ->
        Option.ofBool (mimeType = mimeType')
    | _ ->
        None

let (|ReplyDocumentFileId|) (item: CommandPipelineItem) =
    match item with
    | ReplyMessage (Some { Document = Some { FileId = fileId } }) -> Some fileId
    | _ -> None

let (|ReplyGifFileId|) (item: CommandPipelineItem) =
    match item with
    | ReplyDocumentFileId (Some fileId) & WhenReplyDocumentMimeTypeIs "video/mp4" -> Some (fileId, FileType.Gif)
    | _ -> None

let (|ReplyVoiceFileId|) (item: CommandPipelineItem) =
    match item with
    | ReplyMessage (Some { Voice = Some { FileId = fileId }}) -> Some (fileId, FileType.Voice)
    | _ -> None

let (|ReplyAudioFileId|) (item: CommandPipelineItem) =
    match item with
    | ReplyMessage (Some { Audio = Some { FileId = fileId }}) -> Some (fileId, FileType.Voice)
    | _ -> None

let (|ReplyVideoFileId|) (item: CommandPipelineItem) =
    match item with
    | Message { ReplyToMessage = Some { Video = Some { FileId = fileId } } } -> Some (fileId, FileType.Video)
    | _ -> None

let (|ReplyPhotos|) (item: CommandPipelineItem) =
    match item with
    | Message { ReplyToMessage = Some { Photo = photos } } -> photos
    | _ -> None

let (|ChatId|) (item: CommandPipelineItem) =
    match item with Message { Chat = { Id = id } } -> Some id

let (|MessageId|) (item: CommandPipelineItem) =
    match item with Message { MessageId = id } -> Some id

let (|ReplyToMessageId|) (item: CommandPipelineItem) =
    match item with
    | ReplyMessage (Some { MessageId = id }) -> Some id
    | _ -> None

let (|IsCommand|_|) (commandName: string) (item: CommandPipelineItem) =
    match item with
    | Command (Some c) -> c = commandName |> Option.ofBool
    | _ -> None

let (|%>) (item: CommandPipelineItem) (messageProcessor: CommandPipelineItem -> CommandPipelineItem) =
    match item.Command with
    | ValueNone -> messageProcessor item
    | _ -> item
