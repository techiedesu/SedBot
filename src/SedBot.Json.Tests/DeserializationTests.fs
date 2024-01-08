module SedBot.Json.Tests.DeserializationTests

open NUnit.Framework

open SedBot.Json

[<TestCase("1", typeof<int>, 1)>]
[<TestCase("true", typeof<bool>, true)>]
[<TestCase("999", typeof<uint>, 999u)>]
let ``Primitive type deserialization`` (json: string, t: System.Type, expected) =
    let actual = SedJsonDeserializer.deserialize t json
    if (actual <> expected) then Assert.Fail($"Expected: {expected} but got {actual}")

type SmolUpdate = {
    UpdateId: int64
    Chat: Chat option
}
and MyChatMember = {
    Chat: Chat
    NewChatMember: NewChatMember voption
}
and Chat = {
    Id: int64
    Type: string
    Title: string
}
and Update = {
    UpdateId: int64
    MyChatMember: MyChatMember option

}
and NewChatMember = {
    Status: string
    User: User voption
}
and InlineQuery = {
    Id: string
    From: User
    Query: string
    Offset: string
}
and Message = {
    MessageId: int64
    MessageThreadId: int64 option
    From: User option
    SenderChat: Chat option
    Date: int64
    ReplyToMessage: Message option
    MigrateToChatId: int64 option
    MigrateFromChatId: int64 option
    PinnedMessage: Message option
}
and User = {
    Id: int64
    IsBot: bool
    FirstName: string
    LastName: string option
    Username: string option
    LanguageCode: string option
    IsPremium: bool option
    AddedToAttachmentMenu: bool option
    CanJoinGroups: bool option
    CanReadAllGroupMessages: bool option
    SupportsInlineQueries: bool option
}

type TestApiResponse<'a> = {
    Ok: bool
    Result: 'a
    Description: string option
    ErrorCode: int option
}


[<Test>]
let ``Smol object deserialization`` () =
    let expected : MyChatMember = {
        Chat = {
            Id = -9999999999999L
            Type = "supergroup"
            Title = "redacted"
        }
        NewChatMember = ValueNone
    }

    let deserialized = SedJsonDeserializer.deserialize typeof<MyChatMember> JsonSamples.SmolJson
    Assert.AreEqual (expected, deserialized)

    let deserialized = SedJsonDeserializer.deserializeStatic<MyChatMember> JsonSamples.SmolJson
    Assert.AreEqual (expected, deserialized)

[<Test>]
let ``option deserialization`` () =
    let deserialized = SedJsonDeserializer.deserializeStatic<Option<int>> "null"
    Assert.AreEqual (None, deserialized)
    let deserialized = SedJsonDeserializer.deserializeStatic<Option<int>> "5"
    Assert.AreEqual (Some 5, deserialized)

let ``Nested option deserialization`` () =
    let deserialized = SedJsonDeserializer.deserializeStatic<Option<Option<int>>> "null"
    Assert.AreEqual (None, deserialized)
    let deserialized = SedJsonDeserializer.deserializeStatic<Option<Option<Option<int>>>> "5"
    Assert.AreEqual (Some 5, deserialized)

[<Test>]
let ``Deserialize simple array`` () =
    let expected = [| 1; 2; 3 |]
    let json = "[1, 2, 3]"

    let deserialized = SedJsonDeserializer.deserialize (expected.GetType()) json
    Assert.AreEqual (expected, deserialized)


[<Test>]
let ``Complex object deserialization`` () =
    let tUpdate = typeof<TestApiResponse<Update[]>>
    let res =
        SedJsonDeserializer.deserialize tUpdate JsonSamples.SampleUpdatesResponse
    let res = res :?> TestApiResponse<Update[]>

    Assert.AreEqual(true, res.Ok)
    Assert.AreEqual(None, res.ErrorCode)
    Assert.AreEqual(None, res.Description)

    Assert.AreEqual(30424532, res.Result[0].UpdateId)
    Assert.AreEqual("restricted", res.Result[0].MyChatMember.Value.NewChatMember.Value.Status)
    Assert.AreEqual(true, res.Result[0].MyChatMember.Value.NewChatMember.Value.User.Value.IsBot)
    Assert.AreEqual(-9999999999999L, res.Result[0].MyChatMember.Value.Chat.Id)

    Assert.NotNull(res)

