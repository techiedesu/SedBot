module SedBot.Telegram.Types.Extensions

type FileType =
    | Gif
    | Video
    | Picture
    | Sticker
    | Voice
    | Audio

let extension (ft: FileType) = // TODO: Move?
    match ft with
    | Gif
    | Video -> ".mp4"
    | Picture -> ".png"
    | Sticker -> ".webp"
    | Voice -> ".ogg"
    | Audio -> ".mp3"
