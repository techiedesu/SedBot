module SedBot.ChatCommands

open SedBot.Utilities

open Funogram.Telegram
open Funogram.Telegram.Types

module ActivePatterns =
    let private hasReplyText (msg: Types.Message) =
        Option.isSome msg.Text

    let private isSedTelegramCommand (text: string) =
        text.StartsWith("s/")
    
    let (|SedCommand|VflipCommand|HflipCommand|ReverseCommand|DistortCommand|JqCommand|None|) (message: Types.Message option) =
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
            SedCommand (chatId, msgId, expression, text)
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
            ReverseCommand (chatId, msgId, fileId)
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
            VflipCommand (chatId, msgId, fileId)
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
            HflipCommand (chatId, msgId, fileId)
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
            DistortCommand (chatId, msgId, fileId)
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
            JqCommand (chatId, msgId, data, command |> String.removeFromStart "t!jq")
        | _ ->
            None

