module SedBot.CommandActors

open System.Threading.Channels
open System.Threading.Tasks
open Akka.FSharp.Actors
open Akka.FSharp
open Funogram.Telegram.Bot
open SedBot.Utilities
open Microsoft.Extensions.Logging
open SedBot.Shared.CE

type PictureMail = {
    Picture: byte[]
}

let inline (^) f x = f(x)

let tCmd (command: string) handler context =
    let command = command.Trim().ToLowerInvariant()
    let textNormalized =
        maybe {
            let! message = context.Update.Message
            let! text = message.Text
            return text.Trim().ToLowerInvariant()
        } |> Option.defaultValue null

    if command = textNormalized then
        context |> handler
        false
    else
        context |> cmd command handler

let system =
    Configuration.load()
    |> System.create "system"

type ProcessingPictureMessage = {
    Picture: byte[]
    Context: UpdateContext
}

let pictureFlipProcessor (mailbox: Actor<ProcessingPictureMessage>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        // Handle message here
        return! loop ()
    }
    loop ()

let messageRouterActor (mailbox: Actor<UpdateContext>) =
    let log = Logger.get "messageRouterActor"

    let pictureProcessorActor = pictureFlipProcessor |> spawn system "pictureFlipProcessor"

    let rec loop () = actor {
        let! message = mailbox.Receive()
        let isProcessed =
            [|
                tCmd "t!rev" ^
                    fun context ->
                        let picture = maybe {
                            let! message = context.Update.Message
                            let! replyMessage = message.ReplyToMessage
                            let! photos = replyMessage.Photo
                            let picture =
                                photos
                                |> Array.sortBy ^ fun p -> p.Width
                                |> Array.rev
                                |> Array.head
                            let pictureBytes =
                                picture.FileId
                                |> Api.tryGetFileBytes context
                                |> fun t -> t.ConfigureAwait(false).GetAwaiter().GetResult()
                                |> ValueOption.map Some
                                |> ValueOption.defaultValue None
                            return! pictureBytes
                        }

                        // TODO: Create struct

                        pictureProcessorActor <! picture
            |] |> processCommands message
        log.LogTrace("message {msg}: {status}", message, isProcessed)

        // Handle message here
        return! loop ()
    }
    loop ()



let processorRef = messageRouterActor |> spawn system "processor"
