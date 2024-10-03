module SedBot.Json.Tests.ParseTreeTests

open NUnit.Framework
open SedBot.Json

[<TestCase(JsonSamples.TestJson)>]
[<TestCase(JsonSamples.GptJson)>]
[<TestCase(JsonSamples.SampleUpdatesResponse)>]
[<TestCase(JsonSamples.SampleQuotedJson)>]
let ``Get parse tree from raw json`` (json: string) =
    match SedJsonTreeParser.parse json true with
    | Some json -> printfn $"%A{json}"
    | None -> Assert.Fail "JSON PARSE FAIL"
