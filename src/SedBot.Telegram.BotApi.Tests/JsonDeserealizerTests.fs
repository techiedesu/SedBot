module SedBot.Telegram.BotApi.Tests.JsonDeserealizerTests

open NUnit.Framework
open SedBot.Json
open SedBot.Telegram.BotApi.Types
open SedBot.Telegram.BotApi.Types.CoreTypes

let UpdateResponseJson = """
{
  "ok": true,
  "result": [
    {
      "update_id": 298008175,
      "message": {
        "message_id": 565,
        "from": {
          "id": 1731422365,
          "is_bot": false,
          "first_name": "Vlad",
          "username": "tdesu",
          "language_code": "ru",
          "is_premium": true
        },
        "chat": {
          "id": 1731422365,
          "first_name": "Vlad",
          "username": "tdesu",
          "type": "private"
        },
        "date": 1704489108,
        "text": "\ud83e\udd21"
      }
    }
  ]
}

"""

[<Test>]
let ``getUpdates response`` () =
    let result = SedJsonDeserializer.deserializeStatic<ApiResponse<Update[]>> UpdateResponseJson
    Assert.NotNull(result)
