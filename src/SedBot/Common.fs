module [<AutoOpen>] SedBot.Common

open System.IO
open System.Text.Json
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core

let inline (^) f x = f(x)

type FileType =
    | Gif
    | Video
    | Picture
    | Sticker

let extension (ft: FileType) =
    match ft with
    | Gif | Video -> ".mp4"
    | Picture -> ".png"
    | Sticker -> ".webp"

module String =
    let removeFromStart (input: string) (text: string) =
        if input = null || text = null then
            text
        else
            text.Trim().Substring(input.Length)

type System.String with
    member this.AnyOf(prams: string seq) =
        prams
        |> Seq.tryFind (fun p -> p = this)
        |> Option.isSome

    member this.StartsWithAnyOf(prams: string seq) =
        prams
        |> Seq.tryFind this.StartsWith
        |> Option.isSome

    member this.RemoveAnyOf(prams: string seq) =
        prams
        |> Seq.tryFind this.StartsWith
        |> Option.map (String.removeFromStart this)
        |> Option.defaultValue this

module Option =
    let anyOfList<'a> (items: Option<'a> list) =
        items |> List.find Option.isSome

    let anyOf2 a b =
        [a; b] |> anyOfList

module Json =
    open System.Text.Json.Serialization

    let settings =
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options

    let serialize<'a> (t: 'a) =
        settings.WriteIndented <- false
        JsonSerializer.Serialize(t, settings)

    let serializeNicely (t: 'a) =
        settings.WriteIndented <- true
        JsonSerializer.Serialize(t, settings)

module ActivePatterns =
    let (|NonEmptySeq|_|) a = if Seq.isEmpty a then Some () else None

module File =
    let rec deleteOrNotUnit (files: string list) =
        match files with
        | [] -> ()
        | head :: tail ->
            try
                File.Delete(head)
            with
            | _ -> ()
            deleteOrNotUnit tail
