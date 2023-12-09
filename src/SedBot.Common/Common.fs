module [<AutoOpen>] SedBot.Common.TypeExtensions

open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Threading.Tasks
open Microsoft.FSharp.Core
open SedBot.Common.MaybeBuilder

let inline (^) f x = f x

let inline (<-?) (field: _ byref) a =
    if Object.ReferenceEquals(null, a) then
        field <- ValueNone
    else
        field <- ValueSome a

let inline isNotNull<'T when 'T: not struct> (v: 'T) =
    obj.ReferenceEquals (v, null) |> not

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

    let startsWith (text: string) (input: string) =
        text.StartsWith(input)

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
    let inline anyOf2 a b = maybe {
        return! a
        return! b
    }

    let inline ofBool (v: bool) =
        match v with
        | false ->
            None
        | true ->
            Some ()

    let inline ofValueOption (v: 'T ValueOption) =
        if v.IsSome then
            Some v.Value
        else
            None

    let inline ofCSharpTryPattern (status, value) =
        if status then Some value
        else None

    let inline iterIgnore (action: 'a -> 'b) (value: 'a option) =
        match value with
        | None -> ()
        | Some v -> action v |> ignore

[<RequireQualifiedAccess>]
module Result =
    let inline get (r: Result<_, _>) =
        match r with
        | Error err -> raise (Exception(string err))
        | Ok r -> r

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
module Dictionary =
    let inline getValue key (d: #IDictionary<'TKey, 'TValue>) =
        d[key]

    let inline tryGetValue key (d: #IDictionary<'TKey, 'TValue>) =
        d.TryGetValue key |> Option.ofCSharpTryPattern

[<RequireQualifiedAccess>]
module File =
    let delete filePath =
        try
            File.Delete(filePath)
            Ok ()
        with
        | e ->
            Error e

    let deleteign = delete >> ignore

module [<RequireQualifiedAccess>] Path =
    let tryGetPath path =
        if Path.Exists(path) then
            Some ^ Path.GetFullPath(path)
        else
            None

    let tryGetDirectories path =
        if Path.Exists(path) then
            Some ^ Directory.GetDirectories(path)
        else
            None

[<RequireQualifiedAccess>]
module Int32 =
    let inline tryParse (str: string) = str |> Int32.TryParse |> Option.ofCSharpTryPattern

module Task =
    let runSynchronously (t: Task) =
        if isNotNull t && not t.IsCompleted then
            t.ConfigureAwait(false).GetAwaiter().GetResult()

[<RequireQualifiedAccess>]
module TaskVOption =
    let inline taskBind ([<InlineIfLambda>] binding) (v: 'v voption Task) = task {
        match! v with
        | ValueNone ->
            return ValueNone
        | ValueSome v ->
            return! binding v
    }
