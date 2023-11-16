module SedBot.SpotifyAuthServer.Actors

open System.IO
open System.Net
open System.Threading.Tasks
open Akka.Dispatch
open Akka.FSharp
open Amazon.S3
open Amazon.S3.Model

type JobId = string

type UploadTrackJob = {
    Id: JobId
    Stream: MemoryStream
    Name: string
    BucketName: string
}

type UploadTrackActorOperationResult =
    | Success of JobId: JobId
    | Failed of JobId: JobId

type FetchTracksMail = {
    Tracks: UploadTrackJob array
}

let uploadTrack (client: IAmazonS3) (mailbox: Actor<UploadTrackJob>) =
    let rec loop () = actor {
        let! job = mailbox.Receive()
        ActorTaskScheduler.RunTask (fun _ ->
            task {
                let! resp = client.PutObjectAsync(PutObjectRequest(BucketName = job.BucketName, InputStream = job.Stream))
                if resp.HttpStatusCode = HttpStatusCode.OK then
                    mailbox.Sender() <! UploadTrackActorOperationResult.Success job.Id
                else
                    mailbox.Sender() <! UploadTrackActorOperationResult.Failed job.Id
            } :> Task)
        return! loop ()
    }
    loop()

let youtubePlaylistBuilderActor () =
    ()
