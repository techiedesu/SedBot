module SedBot.Tests.CommandHandlerTests

open Funogram.Telegram.Types
open NUnit.Framework
open SedBot.Commands

[<TestCase("sample_bot", "/send@sample_bot horny bonk", "supergroup", "send", "horny,bonk")>]
[<TestCase("sample_bot", "/send@sample_bot horny bonk", "private", "send", "horny,bonk")>]
[<TestCase("sample_bot", "t!send horny bonk", "private", "send", "horny,bonk")>]
[<TestCase("sample_bot", "t!send", "private", "send", "")>]
[<TestCase("sample_bot", "/send@sample_bot", "private", "send", "")>]
[<TestCase("sample_bot", "/send@sample_bot", "private", "send", "")>]
let [<Test>] ``Command handler works properly`` (botName, command, chatType, expectedCommand, expectedArgs) =
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
        Assert.AreEqual(command, expectedCommand)
        Assert.AreEqual(expectedArgs, System.String.Join(",", args))
    | Some command, None ->
        Assert.AreEqual(command, expectedCommand)
        Assert.AreEqual(expectedArgs, "")
    | _ ->
        Assert.Fail("Broken")
