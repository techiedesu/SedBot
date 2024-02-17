module SedBot.Tests.FfmpegCommandsTests

open System.IO
open NUnit.Framework
open SedBot.ContentProcessing

let private getVideoStream () =
    let sr = new StreamReader("VID_20221007_163400_126.mp4")
    sr.BaseStream

let private ffmpegOs : FFmpeg.FFmpegObjectState = {
    VideoReverse = false
    AudioReverse = false
    RemoveAudio = false
    VerticalFlip = false
    HorizontalFlip = false
    Clock = false
    CClock = false
    Fix = false
    IsPicture = false
    Src = null
}

let [<Test>] ``Non-empty video`` () = task {
    let videoArgs = {
        ffmpegOs with
            Src = getVideoStream()
    }

    let! res = FFmpeg.execute videoArgs
    match res with
    | Error err ->
        Assert.Fail(err)
    | Ok _ -> Assert.Pass()
}

let [<Test>] ``Reverse video`` () = task {
    let videoArgs = {
        ffmpegOs with
            Src = getVideoStream()
            RemoveAudio = true
            VideoReverse = true
    }

    let! res = FFmpeg.execute videoArgs
    match res with
    | Error err ->
        Assert.Fail(err)
    | Ok _ -> Assert.Pass()
}

let [<Test>] ``Normalize video`` () = task {
    let videoArgs = {
        ffmpegOs with
            Src = getVideoStream()
            Fix = true
    }

    let! res = FFmpeg.execute videoArgs
    match res with
    | Error err ->
        Assert.Fail(err)
    | Ok _ -> Assert.Pass()
}
