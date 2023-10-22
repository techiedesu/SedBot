module SedBot.Common.YoutubeApi

open System.IO
open System.Net
open System.Threading.Tasks
open Microsoft.Data.Sqlite
open Dapper.FSharp.SQLite
open SedBot.Common.MaybeBuilder

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

type FirefoxProfile = {
    Name: string
    Path: string
    SqliteDbPath: string
}

// TODO: windows only. make linux/darwin support?
// TODO: whitelist/blacklist firefox profiles?
// TODO: containers support!

let private tryGetFirefoxProfiles () = maybe {
    let profilesPath = @"%APPDATA%\Mozilla\Firefox\Profiles\"
    let! profilesPath = Path.tryGetPath profilesPath
    let! profileDirs =
        Path.tryGetDirectories profilesPath
        |> Option.map ^ Array.filter (fun p -> File.Exists(Path.Combine(p, "cookies.sqlite")))

    return [|
        for dir in profileDirs do
            {
                Name = Path.GetDirectoryName(dir)
                Path = dir
                SqliteDbPath = Path.Combine(dir, "cookies.sqlite")
            }
    |] |> Array.emptyToNone
}

Dapper.FSharp.SQLite.OptionTypes.register()

type CookieItem = {
    Id: int64
    Name: string
    Value: string
    Path: string
    Host: string
    Expiry: int64
}
    with
    member x.CastToCookie() : Cookie =
        // TODO: Expiry? :yao_ming_face"
        Cookie(x.Name, x.Value, x.Path, x.Host)


let tryGetCookies (path: string) =
    use connection = new SqliteConnection($"Data Source={path};")
    let cookieTable = table'<CookieItem> "moz_cookies"

    let selectQuery = select {
        for _ in cookieTable do ()
    }

    selectQuery |> connection.SelectAsync<CookieItem>

let applyCookiesToHttpClient (items: CookieItem seq) (cc: CookieContainer) =
    for item in items do
        cc.Add(item.CastToCookie())
    cc

type DownloadedTrack = {
    Stream: Stream
    Author: string
    Name: string
    Length: int64 option
}

let downloadTrackMaxQuality (uri: string) : DownloadedTrack option Task = task {
    return None
}
