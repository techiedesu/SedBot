module rec SedBot.Common.YoutubeRepository

open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Npgsql
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open Microsoft.FSharp.Collections

type YoutubeTrack = {
    Id: string
    CreatedAt: DateTimeOffset
    S3Link: string
}

let createYoutubeRepository collectionString =
    let connection = NpgsqlDataSourceBuilder(collectionString)
    use connection = connection.Build() // :> Dapper.FSharp.IDbConnection
    use connection = connection.CreateConnection()

    let youtubeTracksTable = table'<YoutubeTrack> "YoutubeTracks" |> inSchema "dbo"

    let addEntity (t: YoutubeTrack) = task {
        let query = insert {
            into youtubeTracksTable
            value t
        }
        return! connection.InsertAsync(query)
    }

    let tryGetEntity (id: string) = task {
        let query = select {
            for _ in youtubeTracksTable do ()
        }
        ()
    }

    addEntity, tryGetEntity
