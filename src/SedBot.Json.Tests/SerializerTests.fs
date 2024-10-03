module SedBot.Json.Tests.SerializerTests

open NUnit.Framework
open SedBot.Json

[<Test>]
let ``Serialize json`` () =
    let obj = [| 1; 2; 3 |]
    let expectedJson = "[1,2,3]"

    let actual = SedJsonSerializer.serializeMinimized obj

    Assert.AreEqual(expectedJson, actual)

[<Test>]
let ``Serialize string in object`` () =
    let obj = [| "[1,2,3]" |]
    let expected = """[\"[1,2,3]\"]"""
    let actual = SedJsonSerializer.serializeMinimized obj

    Assert.AreEqual(expected, actual)
