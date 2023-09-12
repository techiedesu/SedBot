module SedBot.ChannelLayer

open System.Collections.Generic
open SedBot.Common.ActivePatterns.String

type ChannelType =
    | Telegram
    | Discord
    | Unknown

let channelTypeToString = function
    | Telegram ->
        "Telegram"
    | Discord ->
        "Discord"
    | Unknown ->
        "Unknown"

let stringToChannelType = function
    | EqualsInvariantCultureIgnoreCase "telegram" ->
        ChannelType.Telegram
    | EqualsInvariantCultureIgnoreCase "discord" ->
        ChannelType.Discord
    | _ ->
        ChannelType.Unknown

type FileId = private FileId of channelId: string * channelType: ChannelType * meta: Dictionary<string, string>

let resolveTelegramToken channelId : string option =
    None

let getFile (fileId: FileId) = task {
    match fileId with
    | FileId (channelId, ChannelType.Telegram, _) ->
        return resolveTelegramToken channelId
    | _ ->
        return None
}
