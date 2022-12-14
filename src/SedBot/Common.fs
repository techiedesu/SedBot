module [<AutoOpen>] SedBot.Common

open System.IO
open System.Text.Json
open Microsoft.FSharp.Core

let inline (^) f x = f(x)

let inline (<-?) (field: _ byref) a =
    if System.Object.ReferenceEquals(null, a) then
        field <- ValueNone
    else
        field <- ValueSome a

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
    let removeFromStart (text: string) (input: string) =
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

module It =
    let inline Value a = (^a: (member Value: ^b) a)
    let inline Key a = (^a: (member Key: ^b) a)
    let inline KeyIs v a = ((^a: (member Key: ^b) a) = v)
    let inline Width a = (^a: (member Width: ^b) a)

module File =
    let rec deleteOrIgnore (files: string list) =
        match files with
        | [] -> ()
        | head :: tail ->
            try
                File.Delete(head)
            with
            | _ -> ()
            deleteOrIgnore tail
