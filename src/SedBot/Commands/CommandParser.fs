﻿module SedBot.Commands.CommandParser

open SedBot.ChatCommands.Types
open SedBot.Commands.ActivePatterns
open SedBot.Common
open SedBot.Common.MaybeBuilder
open SedBot.Telegram.BotApi.Types

let handleSed (item: CommandPipelineItem) : CommandPipelineItem =
    // Too many valid expressions. For example ":)", "q"...
    // Let's limit the range of valid expressions

    let tryGetValidExpression (expression: string) =
        let isValidExpression expression =
            Process.getStatusCode "sed" [| "-E"; expression |] "data" = 0

        let startWith =
            String.startsWith expression


        let expression =
            match item.Message.Chat.Id with
            | -1001373811109L ->
                let expression =
                    if startWith "t/" then
                        Some ("s/" + expression[2..])
                    elif startWith "t@" then
                        Some ("s@" + expression[2..])
                    else
                        None

                expression

            | _ ->
                if (startWith "s/" || startWith "s@") && isValidExpression expression then
                    Some expression
                else
                    None

        match expression with
        | None -> None
        | Some expression -> if isValidExpression expression then Some expression else None

    match item with
    | ChatId(Some chatId)
        & MessageId (Some srcMsgId)
        & Message { Text = Some expression }
        & ReplyMessage (Some { Text = text; MessageId = msgId; Caption = caption }) ->

        maybe {
            let! expression = tryGetValidExpression expression

            let! text =
                maybe {
                    return! text
                    return! caption
                }

            let res =
                CommandType.Sed(
                    { TelegramOmniMessageId = (chatId, msgId)
                      SrcMsgId = srcMsgId
                      Expression = expression
                      Text = text }
                )

            return item.SetCommand(res)
        }
        |> Option.defaultValue item
    | _ -> item

let handleAwk (item: CommandPipelineItem) : CommandPipelineItem =
    match item with
    | ChatId(Some chatId) & MessageId(Some srcMsgId) & Message { Text = Some expression } & ReplyMessage(Some { Text = text
                                                                                                                MessageId = msgId
                                                                                                                Caption = caption }) ->
        let tryGetValidExpression (expression: string) =
            let isValidExpression (expression: string) =
                expression <> null
                && (expression.StartsWith("{") || expression.StartsWith("BEGIN"))
                && Process.getStatusCode "awk" [| "--sandbox"; expression |] "data" = 0

            if isValidExpression expression then
                Some expression
            else
                None

        maybe {
            let! expression = tryGetValidExpression expression

            let! text =
                maybe {
                    return! text
                    return! caption
                }

            let res =
                CommandType.Awk(
                    { TelegramOmniMessageId = (chatId, msgId)
                      SrcMsgId = srcMsgId
                      Expression = expression
                      Multiline = expression.StartsWith("BEGIN")
                      Text = text }
                )

            return item.SetCommand(res)
        }
        |> Option.defaultValue item
    | _ -> item

let handleZov (item: CommandPipelineItem) : CommandPipelineItem =
    match item with
    | IsCommand "zov" & ChatId(Some chatId) & MessageId(Some srcMsgId) & ReplyMessage(Some { Text = text
                                                                                             MessageId = msgId
                                                                                             Caption = caption }) ->
        maybe {
            let! text =
                maybe {
                    return! text
                    return! caption
                }

            let res =
                CommandType.Zov(
                    { TelegramOmniMessageId = (chatId, msgId)
                      SrcMsgId = srcMsgId
                      Text = text }
                )

            return item.SetCommand(res)
        }
        |> Option.defaultValue item
    | _ -> item

let handleRawMessageInfo (item: CommandPipelineItem) =
    match item with
    | IsCommand "raw" & MessageId(Some msgId) & ChatId(Some chatId) & ReplyMessage(Some replyMessage) ->
        let res =
            CommandType.RawMessageInfo(
                { TelegramOmniMessageId = (chatId, msgId)
                  ReplyTo = replyMessage }
            )

        item.SetCommand(res)
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "raw"
              Description = "Prints internal representation of message" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleReverse (item: CommandPipelineItem) =
    match item with
    | IsCommand "rev" & ChatId(Some chatId) & ReplyToMessageId(Some msgId) & (ReplyGifFileId(Some file) | ReplyVideoFileId(Some file)) ->
        let res =
            CommandType.Reverse(
                { TelegramOmniMessageId = (chatId, msgId)
                  File = file }
            )

        CommandPipelineItem.set item res
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "rev"
              Description = "Applies reverse to gifs, videos, pics, voices and music" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleVerticalFlip (item: CommandPipelineItem) =
    match item with
    | IsCommand "vflip" & ChatId(Some chatId) & ReplyToMessageId(Some msgId) & (ReplyGifFileId(Some file) | ReplyVideoFileId(Some file) | ReplyPhotoMaxQualityId(Some file)) ->
        let res =
            CommandType.VerticalFlip(
                { TelegramOmniMessageId = (chatId, msgId)
                  File = file }
            )

        item.SetCommand(res)
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "vflip"
              Description = "Applies vertical flip to gifs, videos and pics" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleHorizontalFlip (item: CommandPipelineItem) =
    match item with
    | IsCommand "hflip" & ChatId(Some chatId) & ReplyToMessageId(Some msgId) & (ReplyGifFileId(Some file) | ReplyVideoFileId(Some file) | ReplyPhotoMaxQualityId(Some file)) ->
        let res =
            CommandType.HorizontalFlip(
                { TelegramOmniMessageId = (chatId, msgId)
                  File = file }
            )

        item.SetCommand(res)
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "hflip"
              Description = "Applies horizontal flip to gifs, videos and pics" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleClockwiseRotation (item: CommandPipelineItem) =
    match item with
    | IsCommand "clock" & ChatId(Some chatId) & ReplyToMessageId(Some msgId) & (ReplyGifFileId(Some file) | ReplyVideoFileId(Some file) | ReplyPhotoMaxQualityId(Some file)) ->
        let res =
            CommandType.ClockwiseRotation(
                { TelegramOmniMessageId = (chatId, msgId)
                  File = file }
            )

        item.SetCommand(res)
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "clock"
              Description = "Applies counterclockwise rotation to gifs, videos and pics" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleCounterclockwiseRotation (item: CommandPipelineItem) =
    match item with
    | IsCommand "cclock" & ChatId(Some chatId) & ReplyToMessageId(Some msgId) & (ReplyGifFileId(Some file) | ReplyVideoFileId(Some file) | ReplyPhotoMaxQualityId(Some file)) ->
        let res =
            CommandType.CounterClockwiseRotation(
                { TelegramOmniMessageId = (chatId, msgId)
                  File = file }
            )

        item.SetCommand(res)
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "dist"
              Description = "Applies distortion to gifs, videos, pics, voices and music" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleDistortion (item: CommandPipelineItem) =
    match item with
    | IsCommand "dist" & ChatId(Some chatId) & ReplyToMessageId(Some msgId) & (ReplyVoiceFileId(Some file) | ReplyAudioFileId(Some file) | ReplyVideoFileId(Some file) | ReplyGifFileId(Some file) | ReplyPhotoMaxQualityId(Some file)) ->
        let res =
            CommandType.Distortion(
                { TelegramOmniMessageId = (chatId, msgId)
                  File = file }
            )

        item.SetCommand(res)
    | InlineHelp ->
        let inlineHelp: InlineCommandInfo =
            { Command = "dist"
              Description = "Applies distortion to gif, video, voices and music" }

        { item with
            CommandHelpInfo = inlineHelp :: item.CommandHelpInfo }
    | _ -> item

let handleClown (item: CommandPipelineItem) =
    match item with
    | ChatId(Some chatId) & (Text(Some text) | Caption(Some text) | StickerEmoji(Some text)) when text.Contains("🤡") ->
        let res =
            CommandType.Clown(
                { ChatId = chatId
                  Count = String.getCountOfOccurrences text "🤡" }
            )

        item.SetCommand(res)
    | _ -> item

let handleJq (item: CommandPipelineItem) =
    match item with
    | ChatId(Some chatId) & MessageId(Some msgId) & ReplyMessage(Some { Text = Some data }) & CommandWithArgs(Some "jq",
                                                                                                              Some args) ->
        let res =
            CommandType.Jq(
                { TelegramOmniMessageId = (chatId, msgId)
                  Text = data
                  Expression = args |> String.concat " " }
            )

        item.SetCommand(res)
    | _ -> item

let private processMessageAux message botUsername inlineHelp =
    (message, botUsername, inlineHelp) |> CommandPipelineItem.Create
    |%> handleSed
    |%> handleAwk
    |%> handleZov
    |%> handleJq
    |%> handleClown
    |%> handleRawMessageInfo
    |%> handleReverse
    |%> handleDistortion
    |%> handleVerticalFlip
    |%> handleHorizontalFlip
    |%> handleClockwiseRotation
    |%> handleCounterclockwiseRotation

let processMessage message botUsername =
    processMessageAux message botUsername false |> CommandPipelineItem.GetCommand

let processInlineHelp () =
    processMessageAux Message.Empty "" true |> _.CommandHelpInfo |> Array.ofSeq
