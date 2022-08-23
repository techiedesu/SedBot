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