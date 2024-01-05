module [<AutoOpen>] SedBot.Common.TypeExtensions

open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Threading.Tasks
open Microsoft.FSharp.Core
open Microsoft.FSharp.Reflection

#nowarn "0077"
#nowarn "0042" // Allow fsharp core lib specific things

let inline (^) f x = f x

/// Ignores return value. Usable for chain APIs
let inline (~%) x = ignore x

let inline (<-?) (field: _ byref) a =
    if Object.ReferenceEquals(null, a) then
        field <- ValueNone
    else
        field <- ValueSome a

let inline isNotNull<'T when 'T: not struct> (v: 'T) =
    obj.ReferenceEquals (v, null) |> not

let inline delay ([<InlineIfLambda>] act: 'a -> unit) (v: 'a) =
    act v
    v

let inline delay2 ([<InlineIfLambda>] act: 'a -> unit) ([<InlineIfLambda>] act2: 'a -> unit) (v: 'a) =
    act v
    act2 v
    v

let inline swap ([<InlineIfLambda>] act: 'b -> 'a -> _) (a: 'a) (b: 'b) =
    act b a

let snackCaseToCamelCase (str: string) =
    let mutable prev = '_'
    [|
        for c in str do
            let c =
                if prev = '_' then
                    Char.ToUpper c
                else if prev = '0' then
                    Char.ToLower c
                else
                    c

            if c <> '_' then c
            prev <- c
    |]
    |> String

let inline apply2 ([<InlineIfLambda>] f) (a, b) = f a b
let inline apply3 ([<InlineIfLambda>] f) (a, b, c) = f a b c

let isOptionType (t: Type) =
    if FSharpType.IsUnion t then
        let cases = FSharpType.GetUnionCases t
        cases.Length = 2 && cases[0].Name = "None" && cases[1].Name = "Some"
    else
        false

let isValueOptionType (t: Type) =
    if FSharpType.IsUnion t then
        let cases = FSharpType.GetUnionCases t
        cases.Length = 2 && cases[0].Name = "ValueNone" && cases[1].Name = "ValueSome"
    else
        false

let inline inc (a: 'a byref) = a <- a + LanguagePrimitives.GenericOne

/// unsafe cast like in C#
let inline ucast<'a, 'b> (a: 'a): 'b = (# "" a: 'b #)

/// Implicit cast
let inline icast< ^a, ^b when (^a or ^b): (static member op_Implicit: ^a -> ^b)> (value: ^a): ^b = ((^a or ^b): (static member op_Implicit: ^a -> ^b) value)

/// Explicit cast
let inline ecast< ^a, ^b when (^a or ^b): (static member op_Explicit: ^a -> ^b)> (value: ^a): ^b = ((^a or ^b): (static member op_Explicit: ^a -> ^b) value)

/// Handle value and continue
let inline tap<'a> ([<InlineIfLambda>] act: 'a -> unit) (obj: 'a) =
    act obj
    obj

/// Handle value with any result and continue
/// Not recommended for use
let inline tapi<'a> (obj: 'a) ([<InlineIfLambda>] act: 'a -> _) =
    act obj |> ignore
    obj

let inline tap2 ([<InlineIfLambda>] act: 'a -> unit) ([<InlineIfLambda>] act2: 'a -> unit) (obj: 'a) =
    act obj
    act2 obj

[<RequireQualifiedAccess>]
module String =
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
    let inline ofBool (v: bool) =
        match v with
        | false ->
            None
        | true ->
            Some ()

    let inline ofCSharpTryPattern (status, value) =
        if status then Some value
        else None

    let inline iterIgnore (action: 'a -> 'b) (value: 'a option) =
        match value with
        | None -> ()
        | Some v -> %action v

[<RequireQualifiedAccess>]
module Result =
    let inline get (r: Result<_, _>) =
        match r with
        | Error err -> raise (Exception(string err))
        | Ok r -> r

let inline fromFun f = Action<'a> f
let inline fromFun2 f = Action<'a, 'b> f
let inline fromFun3 f = Action<'a, 'b, 'c> f

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

[<RequireQualifiedAccess>]
module Int32 =
    let inline tryParse (str: string) = str |> Int32.TryParse |> Option.ofCSharpTryPattern

module Task =
    let runSynchronously (t: Task) =
        if isNotNull t && not t.IsCompleted then
            t.ConfigureAwait(false).GetAwaiter().GetResult()

    let getResult (t: Task<_>) =
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

module TaskOption =
    let inline ofResult (v: Result<_, _> Task) = task {
        match! v with
        | Error _ ->
            return None
        | Ok v ->
            return Some v
    }

    let inline map (a: 'a -> 'b) v : 'b option Task = task {
        let! v = v
        return v |> Option.map a
    }

[<RequireQualifiedAccess>]
module Path =
    let getSynthName extension =
        Guid.NewGuid().ToString().Replace("-", "") + extension


module ActivePatterns =
    let inline (|Null|_|) x = Object.ReferenceEquals(x, null) |> Option.ofBool
