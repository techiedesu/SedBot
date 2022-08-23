module SedBot.ChatCommands

open SedBot.Utilities

open Funogram.Telegram
open Funogram.Telegram.Types

type Command =
    | SedCommand of chatId: int64 * msgId: int64 * expression: string * text: string
    | VflipCommand of chatId: int64 * msgId: int64 * fileId: string
    | HflipCommand of chatId: int64 * msgId: int64 * fileId: string
    | ReverseCommand of chatId: int64 * msgId: int64 * fileId: string
    | DistortCommand of chatId: int64 * msgId: int64 * fileId: string
    | JqCommand of chatId: int64 * msgId: int64 * expression: string * text: string
    | ClownCommand of chatId: int64
    | Nope

module CommandParser =
    let private hasReplyText (msg: Types.Message) =
        Option.isSome msg.Text

    let private isSedTelegramCommand (text: string) =
        text.StartsWith("s/")

    let parse (message: Types.Message option) =
        match message with
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some expression
                ReplyToMessage = Some
                    {
                        Text = Some text
                        MessageId = msgId
                    }
            } when isSedTelegramCommand expression ->
            Command.SedCommand (chatId, msgId, expression, text)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some {
                        MimeType = Some mimeType
                        FileSize = Some _
                        FileId = fileId
                    }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!rev" ->
            Command.ReverseCommand (chatId, msgId, fileId)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some {
                        MimeType = Some mimeType
                        FileSize = Some _
                        FileId = fileId
                    }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!vflip" ->
            Command.VflipCommand (chatId, msgId, fileId)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some {
                        MimeType = Some mimeType
                        FileSize = Some _
                        FileId = fileId
                    }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!hflip" ->
            Command.HflipCommand (chatId, msgId, fileId)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some {
                        MimeType = Some mimeType
                        FileSize = Some _
                        FileId = fileId
                    }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!dist" ->
            Command.DistortCommand (chatId, msgId, fileId)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                MessageId = msgId
                ReplyToMessage = Some {
                    Text = Some data
                }
            } when command.Trim().StartsWith("t!jq") ->
            Command.JqCommand (chatId, msgId, data, command |> String.removeFromStart "t!jq")
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
            } when command.Trim().Contains("🤡") ->
            Command.ClownCommand chatId
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Sticker = Some { Emoji = Some emoji }
            } when emoji.Contains("🤡") ->
            Command.ClownCommand chatId
        | _ ->
            Command.Nope