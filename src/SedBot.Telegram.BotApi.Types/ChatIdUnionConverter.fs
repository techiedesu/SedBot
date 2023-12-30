namespace SedBot.Telegram.BotApi.Types

open System.Text.Json
open System.Text.Json.Serialization

[<Sealed>]
type ChatIdUnionConverter() =
    inherit JsonConverter<ChatId>()

    override this.Read(reader, _, _) =
        match reader.TokenType with
        | JsonTokenType.String -> reader.GetString() |> int64 |> ChatId.Int
        | _ -> reader.GetInt64() |> ChatId.Int

    override this.Write(writer, value, _) =
        match value with
        | Int int64 -> writer.WriteNumberValue(int64)
        | String s -> writer.WriteNumberValue(s |> int64)

    override _.CanConvert(t) = t = typeof<ChatId>
