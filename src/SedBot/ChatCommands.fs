module SedBot.ChatCommands

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
    | InfoCommand of chatId: int64 * msgId: int64 * replyMsgId: Message
    | CreateKick of chatId: int64 * victimUserId: int64 * msgId: int64
    | Nope

module CommandParser =
    let private isSedTelegramCommand (text: string) =
        text.StartsWith("s/")

    let parse (message: Types.Message option) botUsername =
        let prefix mType prefix = if mType = SuperGroup then prefix else ""
        match message with
        | Some
            {
                MessageId = msgId
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some text
                ReplyToMessage = Some replyToMessage
            } when text.Trim().AnyOf(["t!info"; "/info" + (prefix cType botUsername)]) ->
            Command.InfoCommand (chatId, msgId, replyToMessage)
        | Some
            {
                MessageId = srcMsgId
                Chat = {
                    Id = chatId
                }
                Text = Some expression
                ReplyToMessage = Some
                    {
                        Text = text
                        MessageId = msgId
                        Caption = caption
                    }
            } when isSedTelegramCommand expression ->
            let text = Option.anyOf2 text caption
            if text |> Option.isSome then
                Command.SedCommand (chatId, msgId, srcMsgId, expression, text |> Option.get)
            else
                Command.Nope
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
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
            } when mimeType = "video/mp4" && command.Trim().AnyOf(["t!rev"; "/rev" + (prefix cType botUsername)]) ->
            Command.ReverseCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
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
            } when mimeType = "video/mp4" && command.Trim().AnyOf(["t!vflip"; "/vflip" + (prefix cType botUsername)]) ->
            Command.VflipCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Photo = Some photos // Collection of photos. Thank you.
                }
            } when command.Trim().AnyOf(["t!vflip"; "/vflip" + (prefix cType botUsername)]) ->
            let photo = photos
                        |> Array.sortBy ^ fun p -> p.Width
                        |> Array.rev
                        |> Array.head
            Command.VflipCommand (chatId, msgId, photo.FileId, FileType.Picture)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim().AnyOf(["t!vflip"; "/vflip" + (prefix cType botUsername)]) ->
            Command.VflipCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim().AnyOf(["t!rev"; "/rev" + (prefix cType botUsername)]) ->
            Command.ReverseCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Video = Some { FileId = fileId }
                }
            } when command.Trim().AnyOf(["t!hflip"; "/hflip" + (prefix cType botUsername)]) ->
            Command.HflipCommand (chatId, msgId, fileId, FileType.Video)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Photo = Some photos
                }
            } when command.Trim().AnyOf(["t!vflip"; "/vflip" + (prefix cType botUsername)]) ->
            let photo = photos
                        |> Array.sortBy ^ fun p -> p.Width
                        |> Array.rev
                        |> Array.head
            Command.VflipCommand (chatId, msgId, photo.FileId, FileType.Picture)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
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
            } when mimeType = "video/mp4" && command.Trim().AnyOf(["t!hflip"; "/hflip" + (prefix cType botUsername)]) ->
            Command.HflipCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
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
            } when mimeType = "video/mp4" && command.Trim().AnyOf(["t!dist"; "/dist" + (prefix cType botUsername)]) ->
            Command.DistortCommand (chatId, msgId, fileId, FileType.Gif)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Photo = Some photos
                }
            } when command.Trim().AnyOf(["t!dist"; "/dist" + (prefix cType botUsername)]) ->
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
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Sticker = Some { FileId = fileId }
                }
            } when command.Trim().AnyOf(["t!dist"; "/dist" + (prefix cType botUsername)]) ->
            Command.DistortCommand (chatId, msgId, fileId, FileType.Sticker)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Sticker = Some { FileId = fileId }
                }
            } when command.Trim().AnyOf(["t!vflip"; "/vflip" + (prefix cType botUsername)]) ->
            Command.VflipCommand (chatId, msgId, fileId, FileType.Sticker)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                ReplyToMessage = Some {
                    MessageId = msgId
                    Sticker = Some { FileId = fileId }
                }
            } when command.Trim().AnyOf(["t!hflip"; "/hflip" + (prefix cType botUsername)]) ->
            Command.HflipCommand (chatId, msgId, fileId, FileType.Sticker)
        | Some
            {
                Chat = {
                    Id = chatId
                    Type = cType
                }
                Text = Some command
                MessageId = msgId
                ReplyToMessage = Some {
                    Text = Some data
                }
            } when command.Trim().StartsWithAnyOf(["t!jq"; "/jq" + (prefix cType botUsername)]) ->
            Command.JqCommand (chatId, msgId, data, command.Trim().RemoveAnyOf(["t!jq"; "/jq" + (prefix cType botUsername)]))
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
        | Some
            {
                MessageId = msgId
                Chat = {
                    Id = chatId
                }
                ReplyToMessage = Some {
                    From = Some { Id = victimUserId }
                }
                Text = Some command
            } when command.Trim().StartsWith("t!9") ->
            Command.CreateKick(chatId, victimUserId, msgId)
        | _ ->
            Command.Nope
