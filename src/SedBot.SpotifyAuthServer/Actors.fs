module SedBot.SpotifyAuthServer.Actors

open Akka.Dispatch
open Akka.FSharp

type Track = string // TODO: make struct?

type FetchTracksMail = {
    Tracks: Track seq
}

let fetchTrackActors (mailbox: Actor<FetchTracksMail>) =
    let rec loop () = actor {
        let! mail = mailbox.Receive()

        mail.Tracks |> Seq.iter (fun t -> ()) // TODO: parallel?

        return! loop()
    }
    loop()

