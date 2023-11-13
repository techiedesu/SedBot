module SedBot.Common.YoutubeApi

open System
open System.IO
open System.Net
open System.Linq
open System.Net.Http
open System.Threading.Tasks
open Microsoft.Data.Sqlite
open Dapper.FSharp.SQLite
open Microsoft.FSharp.Collections
open SedBot.Common.MaybeBuilder
open VideoLibrary

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

let tryGetFirefoxProfiles () = maybe {
    let profilesPath =
        match getOperationSystem() with
        | OperationSystem.Windows ->
            @"%APPDATA%\Mozilla\Firefox\Profiles\"
        | OperationSystem.Linux ->
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mozilla/firefox/")
        | _ ->
            failwith "Not supported platform"

    let! profilesPath = Path.tryGetPath profilesPath
    let! profileDirs =
        Path.tryGetDirectories profilesPath
        |> Option.map ^ Array.filter (fun p -> File.Exists(Path.Combine(p, "cookies.sqlite")))

    return! [|
        for dir in profileDirs do
            {
                Name = Path.GetDirectoryName(dir)
                Path = dir
                SqliteDbPath = Path.Combine(dir, "cookies.sqlite")
            }
    |] |> Array.emptyToNone
}

OptionTypes.register()

type FirefoxCookieItem = {
    Id: int64
    Name: string
    Value: string
    Path: string
    Host: string
    Expiry: int64
}
    with
    member x.CastToCookie() : Cookie =
        // def __init__(self,
        //      version: int | None,
        //      name: str,
        //      value: str | None,
        //      port: str | None,
        //      port_specified: bool,
        //      domain: str,
        //      domain_specified: bool,
        //      domain_initial_dot: bool,
        //      path: str,
        //      path_specified: bool,
        //      secure: bool,
        //      expires: int | None,
        //      discard: bool,
        //      comment: str | None,
        //      comment_url: str | None,
        //      rest: dict[str, str],
        //      rfc2109: bool = ...) -> None

        // c = cookie_lib.Cookie(0,
        //               name,
        //               value,
        //               None,
        //               False,
        //               host,
        //               host.startswith('.'),
        //               host.startswith('.'),
        //               path,
        //               False,
        //               isSecure,
        //               expiry,
        //               expiry == "",
        //               None, None, {})

        // TODO: Expiry? :yao_ming_face"
        Cookie(x.Name, WebUtility.UrlEncode(x.Value), x.Path, x.Host)
        // Cookie(x.Name, x.Value, x.Path, x.Host)

type CookieItem = {
    Url: string
    Header: string
}

let tryGetCookies (path: string) = task {
        use connection = new SqliteConnection($"Data Source={path};")
        let cookieTable = table'<FirefoxCookieItem> "moz_cookies"

        let selectQuery = select {
            for _ in cookieTable do ()
        }

        let! res = selectQuery
                   |> connection.SelectAsync<FirefoxCookieItem>
                   // |> TaskSeq.groupBy (fun c -> c.Host)
                   // |> TaskSeq.fold (fun acc -> snd acc)
        return res.ToArray()
    }

let applyCookiesToHttpClient (items: FirefoxCookieItem seq) (cc: CookieContainer) =
    for item in items do
        cc.Add(item.CastToCookie())
    cc

type DownloadedTrack = {
    Stream: Stream
    Author: string
    Name: string
    Length: int64 option
}

// TODO: Fix max quality
let downloadTrack (httpClient: HttpClient) (uri: string) : DownloadedTrack option Task = task {
    let youTube = YouTube(httpClient)
    // let youTube = YouTube()
    let! videos = youTube.GetAllVideosAsync(uri)
    let videos = Array.ofSeq videos
    let tracks = videos
                |> Seq.filter (fun yv -> yv.AdaptiveKind = AdaptiveKind.Audio && yv.AudioFormat = AudioFormat.Opus)
                |> Seq.sortByDescending (fun yv -> yv.AudioBitrate)
                |> Array.ofSeq

    let res1 = Json.serialize videos

    let youTube = YouTube()
    let! videos = youTube.GetAllVideosAsync(uri)
    let videos = Array.ofSeq videos
    let tracks = videos
                |> Seq.filter (fun yv -> yv.AdaptiveKind = AdaptiveKind.Audio && yv.AudioFormat = AudioFormat.Opus)
                |> Seq.sortByDescending (fun yv -> yv.AudioBitrate)
                |> Array.ofSeq

    let res2 = Json.serialize videos

    let res = res1 = res2
    Console.WriteLine(res)

    // let track = Seq.head tracks
    // let! bytes = track.GetBytesAsync()
    // File.WriteAllBytes("foo.mp3", bytes)

    return None
}

let rand = new Random()

let private chromeVersions =
    [|
        "90.0.4430.212"
        "90.0.4430.24"
        "90.0.4430.70"
        "90.0.4430.72"
        "90.0.4430.85"
        "90.0.4430.93"
        "91.0.4472.101"
        "91.0.4472.106"
        "91.0.4472.114"
        "91.0.4472.124"
        "91.0.4472.164"
        "91.0.4472.19"
        "91.0.4472.77"
        "92.0.4515.107"
        "92.0.4515.115"
        "92.0.4515.131"
        "92.0.4515.159"
        "92.0.4515.43"
        "93.0.4556.0"
        "93.0.4577.15"
        "93.0.4577.63"
        "93.0.4577.82"
        "94.0.4606.41"
        "94.0.4606.54"
        "94.0.4606.61"
        "94.0.4606.71"
        "94.0.4606.81"
        "94.0.4606.85"
        "95.0.4638.17"
        "95.0.4638.50"
        "95.0.4638.54"
        "95.0.4638.69"
        "95.0.4638.74"
        "96.0.4664.18"
        "96.0.4664.45"
        "96.0.4664.55"
        "96.0.4664.93"
        "97.0.4692.20"
    |]

let getChromeVer () = chromeVersions[rand.Next(0, Array.length chromeVersions - 1)]
