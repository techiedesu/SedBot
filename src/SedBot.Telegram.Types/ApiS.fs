module SedBot.Telegram.Types.ApiS


let deleteWebhookBase () =
    ReqS.GetWebhookInfo()

let getMe = ReqS.GetMe()

type GetFile =
  {
    FileId: string
  }
  static member Make(fileId: string) =
    {
      FileId = fileId
    }
  interface IRequestBase<File> with
    member _.MethodName = "getFile"
    member this.Type = typeof<GetFile>

let getFile fileId = {
    ReqS.GetFile.FileId = fileId
}

let sendMessageReply chatId text replyToMessageId = ReqS.SendMessage.Make(ChatId.Int chatId, text, replyToMessageId = replyToMessageId)

let sendMessage chatId text = ReqS.SendMessage.Make(ChatId.Int chatId, text)

let private deleteMessageBase chatId messageId =
  ({ ChatId = chatId; MessageId = messageId }: ReqS.DeleteMessage)
let deleteMessage chatId messageId = deleteMessageBase (ChatId.Int chatId) messageId
