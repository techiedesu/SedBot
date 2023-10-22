module SedBot.Tests.YoutubeApiTests

open System.Net.Http
open NUnit.Framework
open SedBot.Common
open SedBot.Common.YoutubeApi

let [<Test>] foo () = task {
    let! q = tryGetCookies @"C:\Users\td\AppData\Roaming\Mozilla\Firefox\Profiles\bowa4fv7.default-release\cookies.sqlite"
    let q = Array.ofSeq q
    Json.serialize q |> System.Console.WriteLine
}

let [<Test>] tryFetchVideoInfo () = task {
    let! cookieItems = tryGetCookies @"C:\Users\td\AppData\Roaming\Mozilla\Firefox\Profiles\eveth9jr.default-release\cookies.sqlite"
    let cookieItems = Array.ofSeq cookieItems

    let hch = new HttpClientHandler()
    cookieItems |> Array.iter (fun x -> hch.CookieContainer.Add(x.CastToCookie()))

    let hc = new HttpClient(hch)
    let! x = downloadTrackMaxQuality hc "https://youtu.be/4TwVYrPN_DM"
    ()
}
