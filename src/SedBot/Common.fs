module [<AutoOpen>] SedBot.Common

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

    let isNulOfWhiteSpace = String.IsNullOrWhiteSpace
    let isNotNulOfWhiteSpace = String.IsNullOrWhiteSpace >> not

module Result =
    let get<'a, 'b> (res: Result<'a, 'b>) =
        match res with
        | Ok res -> res
        | Error _ -> failwith "Can't get result value"

module Option =
    let anyOfList<'a> (items: Option<'a> list) =
        items |> List.find Option.isSome

    let anyOf2 a b =
        [a; b] |> anyOfList

module Json =
    open System.Text.Json.Serialization

    let settings() =
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options

    let private serializeSettings =
        settings()

    let serialize t =
        JsonSerializer.Serialize(t, serializeSettings)

    let private serializeWithIndentationsSettings =
        let settings = settings()
        settings.WriteIndented <- true
        settings

    let serializeWithIndentations t =
        JsonSerializer.Serialize(t, serializeWithIndentationsSettings)

    let private serializeWithIndentationsIgnoreEmptyFieldsSettings =
        let settings = settings()
        settings.WriteIndented <- true
        settings.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        settings

    let serializeWithIndentationsIgnoreEmptyFields (t: 'a) =
        JsonSerializer.Serialize(t, serializeWithIndentationsIgnoreEmptyFieldsSettings)

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
