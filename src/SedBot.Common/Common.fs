module [<AutoOpen>] SedBot.Common.TypeExtensions

open System
open System.IO
open System.Text.Json
open System.Threading.Tasks
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

[<RequireQualifiedAccess>]
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

[<RequireQualifiedAccess>]
module Option =
    let anyOf2 a b =
        a |> Option.orElse b

    let ofBool (v: bool) =
        match v with
        | false ->
            None
        | true ->
            Some ()

[<RequireQualifiedAccess>]
module Json =
    open System.Text.Json.Serialization

    let applySettings (options: JsonSerializerOptions) =
        options.Converters.Add(JsonFSharpConverter())
        options.WriteIndented <- true
        options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        options.Encoder <- System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        options

    let private settings() =
        let options = JsonSerializerOptions()
        applySettings options

    let serializerSettings = settings()

    let serialize t =
        JsonSerializer.Serialize(t, serializerSettings)

[<RequireQualifiedAccess>]
module It =
    let inline Value a = (^a: (member Value: ^b) a)
    let inline Key a = (^a: (member Key: ^b) a)
    let inline KeyIs v a = ((^a: (member Key: ^b) a) = v)
    let inline Width a = (^a: (member Width: ^b) a)

[<RequireQualifiedAccess>]
module File =
    let delete filePath =
        try
            File.Delete(filePath)
            Ok ()
        with
        | e ->
            Error e

    let deleteUnit = delete >> ignore

[<RequireQualifiedAccess>]
module Array =
    let inline any<'T> (a: 'T array) =
        Array.length a = 0

    let inline emptyToNone<'T> (a: 'T array) =
        if Object.ReferenceEquals(a, null) || any a = false then
            None
        else
            Some a

[<RequireQualifiedAccess>]
module TaskSeq =
    let inline map ([<InlineIfLambda>] projection: 'T -> 'U) (source: 'T seq Task) = task {
        let! source = source
        return source |> Seq.map projection
    }

    let inline groupBy ([<InlineIfLambda>] projection: 'T -> 'TKey) (source: 'T seq Task) = task {
        let! source = source
        return source |> Seq.groupBy projection
    }

    let inline fold ([<InlineIfLambda>] folder: 'TAcc -> 'T -> 'TAcc) (acc: 'TAcc) (source: 'T seq Task) = task {
        let! source = source
        return source |> Seq.fold folder acc
    }

    let inline reduce ([<InlineIfLambda>] reducer: 'T -> 'T -> 'T) (source: 'T seq Task) = task {
        let! source = source
        return source |> Seq.reduce reducer
    }

type OperationSystem =
    | Windows
    | Linux
    | FreeBSD
    | Android
    | Browser
    | Other

let getOperationSystem () =
    if OperatingSystem.IsAndroid() then
        OperationSystem.Android
    elif OperatingSystem.IsLinux() then
        OperationSystem.Linux
    elif OperatingSystem.IsWindows() then
        OperationSystem.Windows
    elif OperatingSystem.IsFreeBSD() then
        OperationSystem.FreeBSD
    elif OperatingSystem.IsBrowser() then
        OperationSystem.Browser
    else
        OperationSystem.Other
