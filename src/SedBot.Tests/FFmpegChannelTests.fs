module SedBot.Tests.FFmpegChannelTests

open System.IO
open NUnit.Framework
open SedBot.ProcessingChannels

[<Test>]
let ``Reverse audio and video works properly`` () = task {
    let args = {
        FFmpegObjectState.Create((new StreamReader("VID_20221007_163400_126.mp4")).BaseStream) with
            AudioReverse = true
            VideoReverse = true
    }

    let! res = FFmpeg.execute args

    match res with
    | Result.Ok res ->
        let resFile = "works.mp4"
        do! File.WriteAllBytesAsync(resFile, res.ToArray())

        res.Position <- 0
        let! res = FFmpeg.getStreamsInfo res

        match res with
        | Result.Ok _ ->
            Assert.That(
                File.Exists(resFile)
                && File.ReadAllBytes(resFile).Length > 0
            )
        | Result.Error err -> Assert.Fail(err)
    | Result.Error err -> Assert.Fail(err)
}

[<Test>]
let ``Remove audio with reverse works properly`` () =
    task {
        let args =
            { FFmpegObjectState.Create(
                  (new StreamReader("VID_20221007_163400_126.mp4"))
                      .BaseStream
              ) with
                VideoReverse = true
                RemoveAudio = true }

        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_nosound.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res

            match res with
            | Result.Ok res -> Assert.That(File.Exists(resFile) && res.Length = 1)
            | Result.Error err -> Assert.Fail(err)
        | Result.Error err -> Assert.Fail(err)
    }

[<Test>]
let ``Vflip and hflip works properly`` () =
    task {
        let args =
            { FFmpegObjectState.Create(
                  (new StreamReader("VID_20221007_163400_126.mp4"))
                      .BaseStream
              ) with
                VerticalFlip = true
                HorizontalFlip = true }

        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_nosound_vhfliped.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res

            match res with
            | Result.Ok res -> Assert.That(File.Exists(resFile) && res.Length = 2)
            | Result.Error err -> Assert.Fail(err)
        | Result.Error err -> Assert.Fail(err)
    }

[<Test>]
let ``No audio with reverse works properly`` () = task {
    let args =
        { FFmpegObjectState.Create(
              (new StreamReader("cb3fce1ba6ad45309515cbaf323ba18b.mp4"))
                  .BaseStream
          ) with
            VideoReverse = true
            RemoveAudio = true }

    let! res = FFmpeg.execute args

    match res with
    | Result.Ok res ->
        let resFile = "works_nosound.mp4"
        do! File.WriteAllBytesAsync(resFile, res.ToArray())

        res.Position <- 0
        let! res = FFmpeg.getStreamsInfo res

        match res with
        | Result.Ok res -> Assert.That(File.Exists(resFile) && res.Length = 1)
        | Result.Error err -> Assert.Fail(err)
    | Result.Error err -> Assert.Fail(err)
}

[<Test>]
let ``Clock works properly`` () =
    task {
        let args =
            { FFmpegObjectState.Create(
                  (new StreamReader("cb3fce1ba6ad45309515cbaf323ba18b.mp4"))
                      .BaseStream
              ) with Clock = true }

        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_clock.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res

            match res with
            | Result.Ok res -> Assert.That(File.Exists(resFile) && res.Length = 1)
            | Result.Error err -> Assert.Fail(err)
        | Result.Error err -> Assert.Fail(err)
    }

[<Test>]
let ``Get file info`` () =
    task {
        let! res =
            FFmpeg.getStreamsInfo
                (new StreamReader("VID_20221007_163400_126.mp4"))
                    .BaseStream

        match res with
        | Result.Ok res -> Assert.That(res.Length, Is.EqualTo 2)
        | Result.Error err -> Assert.Fail(err)
    }
