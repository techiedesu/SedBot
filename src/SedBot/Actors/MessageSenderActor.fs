module SedBot.Actors.MessageSenderActor

open Akka.FSharp

type OperationId = System.Guid

type SendMessageLetter = {
    OperationId: OperationId
    ChatId: string
    Text: string option
}

let spawn (mailbox: SendMessageLetter Actor) =
    let rec loop () = actor {
        let! message = mailbox.Receive()


        return! loop()
    }
    loop

module SentMessageResultActor =
    type SentMessageLetter = {
        OperationId: OperationId
        Success: bool
    }
