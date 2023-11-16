﻿module SedBot.Tests.YoutubeApiTests

open System
open System.Net.Http
open NUnit.Framework
open SedBot.Common
open SedBot.Common.YoutubeApi

let [<Test>] download() = task {
    let hc = new HttpClient()
    let! x = YoutubeApi.downloadTrack hc "https://music.youtube.com/watch?v=PLAHsB7EJpA&si=RR07PvNLLjfi4alt"
    ()
}

let [<Test>] ``Download high quality music`` () = task {
    let firefoxProfiles = tryGetFirefoxProfiles()
    let! cookies = tryGetCookies firefoxProfiles.Value[0].SqliteDbPath

    let httpClientHandler = new HttpClientHandler()
    cookies |> Array.iter (fun cookie -> httpClientHandler.CookieContainer.Add(cookie.CastToCookie()))
    let hc = new HttpClient(httpClientHandler)
    hc.DefaultRequestHeaders.Add("User-Agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{getChromeVer()} Safari/537.36")
    let! res = downloadTrack hc "https://youtu.be/cOv1GoWVcY4"
    ()
}
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
