module SedBot.Tests.CommandHandlerTests

open NUnit.Framework
open SedBot.Commands
open SedBot.Telegram.BotApi.Types

[<TestCase("sample_bot", "/send@sample_bot horny bonk", "supergroup", "send", "horny,bonk")>]
[<TestCase("sample_bot", "/send@sample_bot horny bonk", "private", "send", "horny,bonk")>]
[<TestCase("sample_bot", "t!send horny bonk", "private", "send", "horny,bonk")>]
[<TestCase("sample_bot", "t!send", "private", "send", "")>]
[<TestCase("sample_bot", "/send@sample_bot", "private", "send", "")>]
[<TestCase("sample_bot", "/send@sample_bot", "private", "send", "")>]
[<TestCase("sample_bot", "/send", "private", "send", "")>]
[<TestCase("sample_bot", "/send ", "private", "send", "")>]
[<Test>]
let ``Command handler works properly`` (botName, command, chatType, expectedCommand, expectedArgs) =
    let chatType =
        match chatType with
        | "supergroup" -> ChatType.SuperGroup
        | "unknown" -> ChatType.Unknown
        | "sender" -> ChatType.Sender
        | "private" -> ChatType.Private
        | "group" -> ChatType.Group
        | "channel" -> ChatType.Channel
        | s -> failwith $"Can't handle -> {s}"

    match ActivePatterns.commandPatternInternal botName command chatType with
    | Some command, Some args ->
        Assert.That(command, Is.EqualTo expectedCommand)
        Assert.That(System.String.Join(",", args), Is.EqualTo expectedArgs)
    | Some command, None ->
        Assert.That(command, Is.EqualTo expectedCommand)
        Assert.That("", Is.EqualTo expectedArgs)
    | _ -> Assert.Fail("Broken")
