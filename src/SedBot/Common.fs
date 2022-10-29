module [<AutoOpen>] SedBot.Common

open System.IO
open System.Reflection
open System.Runtime.InteropServices
open System.Text.Json
open Microsoft.FSharp.Core
open NUnit.Framework

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
    let rec deleteOrIgnore (files: string list) =
        match files with
        | [] -> ()
        | head :: tail ->
            try
                File.Delete(head)
            with
            | _ -> ()
            deleteOrIgnore tail

    let windowsPathToWsl (path: string) =
        if path = null then
            path
        else
            match path.Split(":\\") |> List.ofArray with
            | [disk; other] -> $"""/mnt/{disk.ToLowerInvariant()}/{other.Replace("\\", "/")}"""
            | _ -> path.Replace("\\", "/")

    let private isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

    let fixPath =
        match isWindows with
        | true -> windowsPathToWsl
        | _ -> id

    let absPath path =
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), path)

module ``File tests`` =
    let [<Test>] ``windowsPathToWsl works properly``() =
        Assert.AreEqual("/mnt/c/Python27", File.windowsPathToWsl "C:\\Python27")
        Assert.AreEqual("Python27/main.fs", File.windowsPathToWsl "Python27\\main.fs")
        Assert.AreEqual(null, File.windowsPathToWsl null)
