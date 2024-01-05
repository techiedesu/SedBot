module SedBot.Json.Tests.JsonSamples

let [<Literal>] SampleUpdatesResponse = """
{
  "ok": true,
  "result": [
    {
      "update_id": 30424532,
      "my_chat_member": {
        "chat": {
          "id": -9999999999999,
          "title": "redacted",
          "type": "supergroup"
        },
        "from": {
          "id": 999999999,
          "is_bot": true,
          "first_name": "redacted",
          "username": "redactedBot"
        },
        "date": 1700911047,
        "old_chat_member": {
          "user": {
            "id": 9999999999,
            "is_bot": true,
            "first_name": "redacted",
            "username": "redactedBot"
          },
          "status": "member"
        },
        "new_chat_member": {
          "user": {
            "id": 9999999999,
            "is_bot": true,
            "first_name": "redacted",
            "username": "redacted"
          },
          "status": "restricted",
          "until_date": 1700914646,
          "can_send_messages": false,
          "can_send_media_messages": false,
          "can_send_audios": false,
          "can_send_documents": false,
          "can_send_photos": false,
          "can_send_videos": false,
          "can_send_video_notes": false,
          "can_send_voice_notes": false,
          "can_send_polls": false,
          "can_send_other_messages": false,
          "can_add_web_page_previews": false,
          "can_change_info": false,
          "can_invite_users": false,
          "can_pin_messages": false,
          "can_manage_topics": false,
          "is_member": true
        }
      }
    }
  ]
}
"""

let [<Literal>] TestJson = """
{
  "where": null,
  "number": 14,
  "vv": 3.14,
  "xxq": 332.2,
  "xxqss": -332.2,
  "name": "Alice",

  "city": "Wonderland",
  "homo": ["gay", "reactor", 14, 88,


  false, null],
  "Heelo":{
    "moto": "pew",
    "ыфвфы": "\u041F\u0440\u0438\u0432\u0435\u0442 \u043C\u0438\u0440",
    "null": true,
    "nulsl": true,
    "false":         false             }


}
"""

let [<Literal>] GptJson = """
{
  "addresses": [
    {
      "street": "123 Main St",
      "city": "Anytown",
      "z-i__-p": "+12345",
      "$type": "dead"
    },
    {
      "street": "456 Maple Ave",
      "city": "Somewhere",
      "zip": -67890
    }
  ],
  "contactNumbers": ["123-456-7890", "987-654-3210"],
  "additionalInfo": {
    "hobbies": ["reading", "gardening", "gaming"],
            "maritalStatus": null
  }
}

    """

let [<Literal>] SmolJson = """
{
 "chat": {
          "id": -9999999999999,
          "title": "redacted",
          "type": "supergroup"
        }
}
"""
