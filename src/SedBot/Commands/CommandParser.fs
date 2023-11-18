module SedBot.Commands.CommandParser

open Funogram.Telegram.Types

open SedBot.ChatCommands.Types
open SedBot.Commands.ActivePatterns
open SedBot.Common
open SedBot.ProcessingChannels
open SedBot.Common.MaybeBuilder

let handleSed (item: CommandPipelineItem) : CommandPipelineItem =
    let tryGetValidExpression (expression: string) =
        let isValidExpression expression =
            Process.getStatusCode "sed" [| "-E"; expression |] "data" = 0

        if isValidExpression expression then
                Some expression
            else
                None

    match item with
    | Message { MessageId = srcMsgId; Chat = { Id = chatId }; Text = Some expression }
        & ReplyMessage (Some { Text = text; MessageId = msgId; Caption = caption }) ->
        maybe {
            let! expression = tryGetValidExpression expression
            let! text = maybe {
                return! text
                return! caption
            }

            let res = CommandType.Sed({
                TelegramOmniMessageId = (chatId, msgId)
                SrcMsgId = srcMsgId
                Expression = expression
                Text = text
            })
            return item.SetCommand(res) } |> Option.defaultValue item
    | _ ->
        item

let handleRawMessageInfo (item: CommandPipelineItem) =
    match item with
    | Message {
        MessageId = msgId
        Chat = { Id = chatId }
        ReplyToMessage = Some replyToMessage } & IsCommand "raw" ->
        let res = CommandType.RawMessageInfo({
            TelegramOmniMessageId = (chatId, msgId)
            ReplyTo = replyToMessage
        })
        item.SetCommand(res)
    | _ ->
        item

let handleReverse (item: CommandPipelineItem) =
    match item with
    | IsCommand "rev"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & (ReplyGifFileId (Some file) | ReplyVideoFileId (Some file)) ->
        let res = CommandType.Reverse({
            TelegramOmniMessageId = (chatId, msgId)
            File = file
        })
        CommandPipelineItem.set item res
    | _ ->
        item

let handleVerticalFlip (item: CommandPipelineItem) =
    match item with
    | IsCommand "vflip"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & (ReplyGifFileId (Some file) | ReplyVideoFileId (Some file)) ->
        let res = CommandType.VerticalFlip({
            TelegramOmniMessageId = (chatId, msgId)
            File = file
        })
        item.SetCommand(res)

    | IsCommand "vflip"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & ReplyPhotos (Some photos) ->
        let fileId =
            photos
            |> Array.sortByDescending It.Width
            |> Array.head
            |> fun p -> p.FileId

        let res =
            CommandType.VerticalFlip({
                TelegramOmniMessageId = (chatId, msgId)
                File = fileId, FileType.Picture
            })

        item.SetCommand(res)
    | _ -> item

let handleHorizontalFlip (item: CommandPipelineItem) =
    match item with
    | IsCommand "hflip"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & (ReplyGifFileId (Some file) | ReplyVideoFileId (Some file)) ->
        let res = CommandType.HorizontalFlip({
            TelegramOmniMessageId = (chatId, msgId)
            File = file
        })
        item.SetCommand(res)

    | IsCommand "hflip"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & ReplyPhotos (Some photos) ->
        let fileId =
            photos
            |> Array.sortBy It.Width
            |> Array.rev
            |> Array.head
            |> fun p -> p.FileId

        let res = CommandType.HorizontalFlip({
            TelegramOmniMessageId = (chatId, msgId)
            File = fileId, FileType.Picture
        })

        item.SetCommand(res)
    | _ -> item

let handleClockwiseRotation (item: CommandPipelineItem) =
    match item with
    | IsCommand "clock"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & (ReplyGifFileId (Some file) | ReplyVideoFileId (Some file)) ->
        let res = CommandType.ClockwiseRotation({
            TelegramOmniMessageId = (chatId, msgId)
            File = file
        })
        item.SetCommand(res)

    | IsCommand "clock"
        & ReplyPhotos (Some photos)
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId) ->
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

let handleCounterclockwiseRotation (item: CommandPipelineItem) =
    match item with
    | IsCommand "cclock"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & (ReplyGifFileId (Some file) | ReplyVideoFileId (Some file)) ->
        let res = CommandType.CounterClockwiseRotation({
                TelegramOmniMessageId = (chatId, msgId)
                File = file
            })
        item.SetCommand(res)

    | IsCommand "cclock"
        & ReplyPhotos (Some photos)
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId) ->
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

let handleDistortion (item: CommandPipelineItem) =
    match item with
    | IsCommand "dist"
        & ChatId (Some chatId)
        & ReplyToMessageId (Some msgId)
        & (ReplyVoiceFileId (Some file)
            | ReplyAudioFileId (Some file)
            | ReplyVideoFileId (Some file)
            | ReplyGifFileId (Some file)
        ) ->
        let res = CommandType.Distortion({
            TelegramOmniMessageId = (chatId, msgId)
            File = file
        })
        item.SetCommand(res)

    | IsCommand "dist"
        & ChatId (Some chatId)
        & ReplyPhotos (Some photos)
        & ReplyToMessageId (Some msgId) ->
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

let handleClown (item: CommandPipelineItem) =
    match item with
    | ChatId (Some chatId)
        & Message { Text = Some command } when command.Contains("🤡") ->
        let res = CommandType.Clown({
            ChatId = chatId
            Count = String.getCountOfOccurrences command "🤡"
        })
        item.SetCommand(res)

    | ChatId (Some chatId)
        & Message { Caption = Some command } when command.Contains("🤡") ->
        let res = CommandType.Clown({
            ChatId = chatId
            Count = String.getCountOfOccurrences command "🤡"
        })
        item.SetCommand(res)

    | ChatId (Some chatId)
        & Message { Sticker = Some { Emoji = Some emoji } } when emoji.Contains("🤡") ->
        let res = CommandType.Clown({ ChatId = chatId; Count = 1 })
        item.SetCommand(res)
    | _ -> item

let handleJq (item: CommandPipelineItem) =
    match item with
    | ChatId (Some chatId)
        & MessageId (Some msgId)
        & ReplyMessage (Some { Text = Some data })
        & CommandWithArgs (Some "jq", Some args) ->
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
