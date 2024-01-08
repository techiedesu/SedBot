module SedBot.Json.Tests.ParseTreeWalkerTests

open Microsoft.FSharp.Reflection
open NUnit.Framework
open SedBot.Json.SedNetTreeTypeBuilder

type SomeRecursiveShit = {
    Foo: AnotherGay
    Foo1: AnotherGay
    Foo2: AnotherGay
    Foo3: AnotherGay
    Foo4: AnotherGay
}
and AnotherGay = {
    Bar: SomeRecursiveShit
    Bar2: SomeRecursiveShit
    Bar3: SomeRecursiveShit
    Bar4: SomeRecursiveShit
    Bar5: SomeRecursiveShit
}

type TestUpdate = {
    UpdateId: int64
    Message: TestMessage option
}
and TestMessage = {
    ReplyToMessage: TestMessage option
    EditedMessage: TestMessage option
}

[<Test>]
let ``Test typeTreeBuilder`` () =
    let typeTreeRootNode = typeof<SomeRecursiveShit>
    let nodes, rootNode = buildTypeTree typeTreeRootNode
    Assert.That(nodes.Count = 5 + 5 + 1)
    Assert.That(rootNode.Children.Count = FSharpType.GetRecordFields(typeTreeRootNode).Length)

[<Test>]
let ``Test nodeWalker`` () =
    let q = typeof<SomeRecursiveShit>
    let typeTreeRootNode = buildTypeTree q |> snd
    let path = [
        for _ in 0 .. 100 do
            yield "Foo"
            yield "Bar"
        yield "Foo"
    ]
    let resNode = treeTypeWalkerList path typeTreeRootNode
    Assert.That(resNode.Children.Count = 5)

[<Test>]
let ``Test recursive entity`` () =
    let tUpdate = typeof<TestUpdate>
    let allNodes, rootNode = buildTypeTree tUpdate
    let path = [
        yield "Message"
        yield "Value"
        for _ in 0 .. 100 do
            yield "EditedMessage"
            yield "Value"
            yield "ReplyToMessage"
            yield "Value"
    ]

    let resNode = treeTypeWalkerList path rootNode
    printfn "%A %A" resNode allNodes
