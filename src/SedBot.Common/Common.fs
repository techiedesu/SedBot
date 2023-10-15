module [<AutoOpen>] SedBot.Common.TypeExtensions

open System
open System.IO
open System.Text.Json
open Microsoft.FSharp.Core

let inline (^) f x = f x

let inline (<-?) (field: _ byref) a =
    if Object.ReferenceEquals(null, a) then
        field <- ValueNone
    else
        field <- ValueSome a

type FileType =
    | Gif
    | Video
    | Picture
    | Sticker
    | Voice
    | Audio

let extension (ft: FileType) =
    match ft with
    | Gif | Video -> ".mp4"
    | Picture -> ".png"
    | Sticker -> ".webp"
    | Voice -> ".ogg"
    | Audio -> ".mp3"

module String =
    let removeFromStart (text: string) (input: string) =
        if input = null || text = null then
            text
        else
            text.Trim().Substring(input.Length)

    let isNullOfWhiteSpace = String.IsNullOrWhiteSpace
    let isNotNulOfWhiteSpace = String.IsNullOrWhiteSpace >> not

    let getCountOfOccurrences (str: string) (substr: string) =
        match str with
        | null | "" ->
            0
        | _ ->
            let res = Array.length ^ str.Split(substr)
            res - 1

module Option =
    let anyOf2 a b =
        a |> Option.orElse b

    let ofBool (v: bool) =
        match v with
        | false ->
            None
        | true ->
            Some ()

module Json =
    open System.Text.Json.Serialization

    let settings() =
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options

    let serializerSettings =
        let settings = settings()
        settings.WriteIndented <- true
        settings.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        settings.Encoder <- System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        settings

    let serialize t =
        JsonSerializer.Serialize(t, serializerSettings)

module It =
    let inline Value a = (^a: (member Value: ^b) a)
    let inline Key a = (^a: (member Key: ^b) a)
    let inline KeyIs v a = ((^a: (member Key: ^b) a) = v)
    let inline Width a = (^a: (member Width: ^b) a)

module File =
    let delete filePath =
        try
            File.Delete(filePath)
            Ok ()
        with
        | e ->
            Error e

    let deleteUnit = delete >> ignore
