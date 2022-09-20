module [<AutoOpen>] SedBot.Common

let inline (^) f x = f(x)

type FileType =
    | Gif
    | Video
    | Picture
    | Sticker

let extension (ft: FileType) =
    match ft with
    | Gif | Video  -> ".mp4"
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
        prams |> Seq.tryFind (fun p -> p = this) |> Option.isSome

    member this.StartsWithAnyOf(prams: string seq) =
        prams |> Seq.tryFind this.StartsWith |> Option.isSome

    member this.RemoveAnyOf(prams: string seq) =
        let matched = prams |> Seq.tryFind this.StartsWith
        match matched with
        | Some value ->
            String.removeFromStart value this
        | _ -> this

module Option =
    let anyOf (a: 'a option) (b: 'a option) =
        if a |> Option.isSome then
            a
        elif b |> Option.isSome then
            b
        else
            None
