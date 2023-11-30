module rec SedBot.Common.YoutubeRepository

open System
open Npgsql
open Dapper.FSharp.PostgreSQL
open Microsoft.FSharp.Collections

type YoutubeTrack = {
    Id: string
    CreatedAt: DateTimeOffset
    S3Link: string
}

let createYoutubeRepository collectionString =
    let connection = NpgsqlDataSourceBuilder(collectionString)
    let connection = connection.Build()
    let connection = connection.CreateConnection()

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
            for y in youtubeTracksTable do where (y.Id = id)
        }
        let! res = connection.SelectAsync<YoutubeTrack>(query)
        return Seq.tryHead res
    }

    addEntity, tryGetEntity
