﻿module SedBot.Tests.ImageMagickTests

open System.IO
open NUnit.Framework

open SedBot.Common
open SedBot.Telegram.Types.Extensions
open SedBot.ContentProcessing

[<Test>]
let ``liquid rescale works properly`` () =
    task {
        let sr = new StreamReader("VID_20221007_163400_126.mp4")

        let state: ImageMagick.ImageMagickObjectState =
            { Src = sr.BaseStream
              FileType = FileType.Video }

        let! res = ImageMagick.convert state

        match res with
        | Result.Ok(res, fileName) ->
            File.deleteign fileName
            let res = res.ToArray()
            do! File.WriteAllBytesAsync("liquid_out.mp4", res)
            Assert.That(res.Length > 0)
        | Result.Error err -> Assert.Fail(err)
    }
