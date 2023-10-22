module SedBot.Tests.YoutubeApiTests

open NUnit.Framework
open SedBot.Common
open SedBot.Common.YoutubeApi

let [<Test>] foo () = task {
    let! q = tryGetCookies @"C:\Users\td\AppData\Roaming\Mozilla\Firefox\Profiles\bowa4fv7.default-release\cookies.sqlite"
    let q = Array.ofSeq q
    Json.serialize q |> System.Console.WriteLine
}
