module rec SedBot.Telegram.BotApi.Tests.TypeSerializerTests

open System.IO
open System.Net.Http
open NUnit.Framework
open SedBot.Telegram.BotApi.Types
open SedBot.Common.TypeExtensions
open SedBot.Telegram.BotApi.Types.CoreTypes
open TypeShape.Core
open TypeShape.Core.Utils

let builderExp<'T, 'Req when 'Req :> IRequestBase<'T>> dataContent msg =
    SedBot.Telegram.RequestBuilder.build<'T> dataContent msg

[<Test>]
let ``Testing TypeShape converter`` () =
    let stream =
        new StreamReader(@"C:\Users\td\Pictures\Снимок экрана 2022-07-19 012939.png")

    let msg: IBotRequest =
        Req.SendMessage.Make(228, "Hello", allowSendingWithoutReply = true)

    let dataContent = new MultipartFormDataContent()
    // let p = SedBot.Telegram.RequestBuilder.build<Req.SendPhoto> dataContent msg
    let x = msg.GetType()

    let p =
        SedBot.Telegram.RequestBuilder.builderDynamic (msg.GetType()) dataContent msg

    printfn $"{p}"
