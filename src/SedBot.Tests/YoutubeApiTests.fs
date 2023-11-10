module SedBot.Tests.YoutubeApiTests

open System.Net.Http
open NUnit.Framework
open SedBot.Common

let [<Test>] download() = task {
    let hc = HttpClient()
    let! x = YoutubeApi.downloadTrack hc "https://music.youtube.com/watch?v=PLAHsB7EJpA&si=RR07PvNLLjfi4alt"
    ()
}

// let [<Test>] foo () = task {
//     let! q = tryGetCookies @"C:\Users\td\AppData\Roaming\Mozilla\Firefox\Profiles\bowa4fv7.default-release\cookies.sqlite"
//     let q = Array.ofSeq q
//     Json.serialize q |> Console.WriteLine
// }
//
// let [<Test>] tryFetchVideoInfo () = task {
//     let! cookieItems = tryGetCookies @"C:\Users\td\AppData\Roaming\Mozilla\Firefox\Profiles\eveth9jr.default-release\cookies.sqlite"
//     let cookieItems = Array.ofSeq cookieItems
//
//     let hch = new HttpClientHandler()
//     // cookieItems |> Array.iter (fun x -> hch.CookieContainer.Add(x.CastToCookie()))
//
//     for cookie in cookieItems do
//         let host =
//             let host =
//                 if cookie.Host.StartsWith(".") then
//                     cookie.Host.Substring(1)
//                 else
//                     cookie.Host
//             $"https://{host}"
//
//         try
//             hch.CookieContainer.SetCookies(Uri($"{host}{cookie.Path}"), cookie.Value)
//         with e ->
//             Console.WriteLine(e)
//             Console.WriteLine(host)
//
//     Console.WriteLine("Read cookies: {0}", Array.length cookieItems)
//
//     let hc = new HttpClient(hch)
//
//     // VideoLibrary.Exceptions.UnavailableStreamException : Error caused by Youtube.(Видео недоступно))
//     // TODO: cookie don't apply. fix?
//     let! x = downloadTrack hc "https://music.youtube.com/watch?v=L_uhcuSXlTM&si=5sFw8VqLG21pQDb_" // "https://youtu.be/4TwVYrPN_DM"
//     ()
// }
