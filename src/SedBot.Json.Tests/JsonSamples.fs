module SedBot.Json.Tests.JsonSamples

let [<Literal>] SampleQuotedJson = """
{
   "ok": true,
   "result": {
     "text": "{ \"message_id\": \"644\", \"from\": \"{ \"id\": \"6675788706\", \"is_bot\": \"true\", \"first_name\": \"Debug Sed\", \"username\": \"debug_sed_bot\" }\", \"date\": \"1704685275\", \"chat\": \"{ \"id\": \"1731422365\", \"type\": \"Private\", \"username\": \"tdesu\", \"first_name\": \"Vlad\" }\", \"animation\": \"{ \"file_id\": \"CgACAgIAAxkDAAIChGWbbtqil64oWlCU4eHpgds6_Px2AAKTOgAC0RvhSDWJP4jNeRiONAQ\", \"file_unique_id\": \"AgADkzoAAtEb4Ug\", \"width\": \"672\", \"height\": \"848\", \"duration\": \"7\", \"thumbnail\": \"{ \"file_id\": \"AAMCAgADGQMAAgKEZZtu2qKXrihaUJTh4emB2zr8_HYAApM6AALRG-FINYk_iM15GI4BAAdtAAM0BA\", \"file_unique_id\": \"AQADkzoAAtEb4Uhy\", \"width\": \"254\", \"height\": \"320\", \"file_size\": \"14630\" }\", \"file_name\": \"c9c4144e4cdc477fae80f2e534e0645c.mp4\", \"mime_type\": \"video/mp4\", \"file_size\": \"938395\" }\", \"document\": \"{ \"file_id\": \"CgACAgIAAxkDAAIChGWbbtqil64oWlCU4eHpgds6_Px2AAKTOgAC0RvhSDWJP4jNeRiONAQ\", \"file_unique_id\": \"AgADkzoAAtEb4Ug\", \"thumbnail\": \"{ \"file_id\": \"AAMCAgADGQMAAgKEZZtu2qKXrihaUJTh4emB2zr8_HYAApM6AALRG-FINYk_iM15GI4BAAdtAAM0BA\", \"file_unique_id\": \"AQADkzoAAtEb4Uhy\", \"width\": \"254\", \"height\": \"320\", \"file_size\": \"14630\" }\", \"file_name\": \"c9c4144e4cdc477fae80f2e534e0645c.mp4\", \"mime_type\": \"video/mp4\", \"file_size\": \"938395\" }\" }"
   }
}
"""

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
