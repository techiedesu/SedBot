module SedBot.SpotifyAuthServer.Actors

open Akka.Dispatch
open Akka.FSharp

type Track = string // TODO: make struct?

type FetchTracksMail = {
    Tracks: Track array
}

let fetchTrackActors (mailbox: Actor<FetchTracksMail>) =
    let rec loop () = actor {
        let! mail = mailbox.Receive()

        let q =
            mail.Tracks
            |> Array.map (fun t -> ())
        // TODO: parallel?

        return! loop()
    }
    loop()

