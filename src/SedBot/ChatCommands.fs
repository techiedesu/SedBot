module SedBot.ChatCommands

open SedBot.Utilities

open Funogram.Telegram
open Funogram.Telegram.Types
open SedBot

type Command =
    | SedCommand of chatId: int64 * msgId: int64 * srcMsgId: int64 * expression: string * text: string
    | VflipCommand of chatId: int64 * msgId: int64 * fileId: string * FileType: FileType
    | HflipCommand of chatId: int64 * msgId: int64 * fileId: string * FileType: FileType
    | ReverseCommand of chatId: int64 * msgId: int64 * fileId: string * FileType: FileType
    | DistortCommand of chatId: int64 * msgId: int64 * fileId: string * FileType: FileType
    | JqCommand of chatId: int64 * msgId: int64 * expression: string * text: string
    | ClownCommand of chatId: int64 * count: int
    | Nope

module CommandParser =
    let private isSedTelegramCommand (text: string) =
        text.StartsWith("s/")

    let parse (message: Types.Message option) =
        match message with
        | Some
            {
                MessageId = srcMsgId
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
            Command.SedCommand (chatId, msgId, srcMsgId, expression, text)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some
                        {
                            MimeType = Some mimeType
                            FileSize = Some _
                            FileId = fileId
                        }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!rev" ->
            Command.ReverseCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some
                        {
                            MimeType = Some mimeType
                            FileSize = Some _
                            FileId = fileId
                        }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!vflip" ->
            Command.VflipCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Photo = Some photos // Collection of photos. Thank you.
                }
            } when command.Trim() = "t!vflip" ->
            let photo = photos
                        |> Array.sortBy ^ fun p -> p.Width
                        |> Array.rev
                        |> Array.head
            Command.VflipCommand (chatId, msgId, photo.FileId, FileType.Picture)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim() = "t!vflip" ->
            Command.VflipCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim() = "t!rev" ->
            Command.ReverseCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim() = "t!hflip" ->
            Command.HflipCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Photo = Some photos
                }
            } when command.Trim() = "t!vflip" ->
            let photo = photos
                        |> Array.sortBy ^ fun p -> p.Width
                        |> Array.rev
                        |> Array.head
            Command.VflipCommand (chatId, msgId, photo.FileId, FileType.Picture)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some
                        {
                            MimeType = Some mimeType
                            FileSize = Some _
                            FileId = fileId
                        }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!hflip" ->
            Command.HflipCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Document = Some
                        {
                            MimeType = Some mimeType
                            FileSize = Some _
                            FileId = fileId
                        }
                }
            } when mimeType = "video/mp4" && command.Trim() = "t!dist" ->
            Command.DistortCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Photo = Some photos
                }
            } when command.Trim() = "t!dist" ->
            let photo = photos
                        |> Array.sortBy ^ fun p -> p.Width
                        |> Array.rev
                        |> Array.head
            Command.DistortCommand (chatId, msgId, photo.FileId, FileType.Picture)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim() = "t!dist" ->
            Command.DistortCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Sticker = Some { FileId = fileId }
                }
            } when command.Trim() = "t!dist" ->
            Command.DistortCommand (chatId, msgId, fileId, FileType.Sticker)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Sticker = Some { FileId = fileId }
                }
            } when command.Trim() = "t!vflip" ->
            Command.VflipCommand (chatId, msgId, fileId, FileType.Sticker)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Sticker = Some { FileId = fileId }
                }
            } when command.Trim() = "t!hflip" ->
            Command.HflipCommand (chatId, msgId, fileId, FileType.Sticker)
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
            Command.ClownCommand (chatId, command.Split("🤡").Length - 1)
        | Some
            {
                Chat = {
                    Id = chatId
                }
                Sticker = Some { Emoji = Some emoji }
            } when emoji.Contains("🤡") ->
            Command.ClownCommand (chatId, 1)
        | _ ->
            Command.Nope
