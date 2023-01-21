module SedBot.ProcessingChannels

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open NUnit.Framework
open SedBot
open SedBot.CliWrap
open SedBot.Utilities
open FParsec

open Microsoft.Extensions.Logging

type FFmpegObjectState = {
    Src: Stream
    VideoReverse: bool
    AudioReverse: bool
    RemoveAudio: bool
    VerticalFlip: bool
    HorizontalFlip: bool
    Clock: bool
    CClock: bool
}
with
    static member Create(src) = {
        Src = src
        VideoReverse = false
        AudioReverse = false
        RemoveAudio = false
        VerticalFlip = false
        HorizontalFlip = false
        Clock = false
        CClock = false
    }

type StreamsInfo = StreamInfo array
and StreamInfo = {
    Kv: Dictionary<string, string>
    Index: int option
    CodecName: string option
    CodecLongName: string option
}

module FFmpeg =
    let getStreamsInfo (stream: Stream) = task {
        let errSb = StringBuilder()
        let resSb = StringBuilder()
        let! executionResult =
            "ffprobe"
            |> wrap
            |> withStandardInputPipe (PipeSource.FromStream stream)
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStringBuilder(resSb))
            |> withArguments ["-i pipe: -show_streams"] (ValueSome false)
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding
        if executionResult.ExitCode = 0 then
            let res = resSb.ToString()
            let letterOrDigitPE c = isNoneOf "=\n\r" c
            let kvPEr = many1Satisfy2 (fun c -> letterOrDigitPE c && (c = '[' |> not)) letterOrDigitPE
            let valuePE = many1SatisfyL letterOrDigitPE "value"

            let kvPE = kvPEr .>> (pstring "=") .>>. valuePE

            let kvPE1 = many (kvPE .>> (optional newline))
            let kvPE2 = (optional newline) >>. (pstring "[STREAM]") >>. newline >>. kvPE1 .>> (pstring "[/STREAM]") .>> (optional newline)
            let kvPE3 = many kvPE2

            match run kvPE3 (res.Trim()) with
            | Success(result, _, _) ->
                let res : StreamsInfo = [|
                    for res in result do
                        let dict = Dictionary(res |> List.map KeyValuePair)
                        {
                            Index = dict |> Seq.tryFind ^ It.KeyIs "index" |> Option.map (It.Value >> int)
                            CodecName = dict |> Seq.tryFind ^ It.KeyIs "codec_name" |> Option.map It.Value
                            CodecLongName = dict |> Seq.tryFind ^ It.KeyIs "codec_long_name" |> Option.map It.Value
                            Kv = dict
                        }
                |]
                return res |> Result.Ok
            | Failure(errorMsg, _, _) ->
                return errorMsg |> Result.Error
        else
            return errSb.ToString() |> Result.Error

    }

    let execute (data: FFmpegObjectState) = task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let audioReverse =
            match data.AudioReverse, data.RemoveAudio with
            | true, false ->
                 " -af areverse"
            | _, true ->
                " -an"
            | _, _ ->
                ""

        let videoReverse =
            match data.VideoReverse with
            | true ->
                 " -vf reverse"
            | _ ->
                ""

        let vFlip =
            match data.VerticalFlip with
            | true ->
                " -vf vflip -qscale 0"
            | _ -> ""

        let hFlip =
            match data.HorizontalFlip with
            | true ->
                " -vf hflip -qscale 0"
            | _ -> ""

        let clock =
            match data.Clock with
            | true ->
                " -vf \"transpose=clock\""
            | _ -> ""

        let cClock =
            match data.CClock with
            | true ->
                " -vf \"transpose=cclock\""
            | _ -> ""

        // FFmpeg can't read moov (MPEG headers) at the end of a file when using a pipe. Have to "dump" to a filesystem.
        data.Src.Position <- 0
        let memSrc = new MemoryStream()
        do! data.Src.CopyToAsync(memSrc)
        let inputFile = Path.getSynthName ".mp4"
        do! File.WriteAllBytesAsync(inputFile, memSrc.ToArray())

        let args = $"{inputFile}{audioReverse}{videoReverse}{vFlip}{hFlip}{clock}{cClock}"
        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments [$"-i {args} -f mp4 -movflags frag_keyframe+empty_moov -vcodec libx264 pipe:1"] (ValueSome false)
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        File.deleteOrIgnore [inputFile]
        if executionResult.ExitCode = 0 then
            return target |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

module Tests =

    let [<Test>] ``Reverse audio and video works properly``() = task {
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
                Assert.True(File.Exists(resFile) && File.ReadAllBytes(resFile).Length > 0)
            | Result.Error err ->
                Assert.Fail(err)
        | Result.Error err ->
            Assert.Fail(err)
    }

    let [<Test>] ``Remove audio with reverse works properly``() = task {
        let args = {
            FFmpegObjectState.Create((new StreamReader("VID_20221007_163400_126.mp4")).BaseStream) with
                VideoReverse = true
                RemoveAudio = true
        }
        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_nosound.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res
            match res with
            | Result.Ok res ->
                Assert.True(File.Exists(resFile) && res.Length = 1)
            | Result.Error err ->
                Assert.Fail(err)
        | Result.Error err ->
            Assert.Fail(err)
    }

    let [<Test>] ``Vflip and hflip works properly``() = task {
        let args = {
            FFmpegObjectState.Create((new StreamReader("VID_20221007_163400_126.mp4")).BaseStream) with
                VerticalFlip = true
                HorizontalFlip = true
        }
        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_nosound_vhfliped.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res
            match res with
            | Result.Ok res ->
                Assert.True(File.Exists(resFile) && res.Length = 2)
            | Result.Error err ->
                Assert.Fail(err)
        | Result.Error err ->
            Assert.Fail(err)
    }

    let [<Test>] ``No audio with reverse works properly``() = task {
        let args = {
            FFmpegObjectState.Create((new StreamReader("cb3fce1ba6ad45309515cbaf323ba18b.mp4")).BaseStream) with
                VideoReverse = true
                RemoveAudio = true
        }
        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_nosound.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res
            match res with
            | Result.Ok res ->
                Assert.True(File.Exists(resFile) && res.Length = 1)
            | Result.Error err ->
                Assert.Fail(err)
        | Result.Error err ->
            Assert.Fail(err)
    }

    let [<Test>] ``Clock works properly``() = task {
        let args = {
            FFmpegObjectState.Create((new StreamReader("cb3fce1ba6ad45309515cbaf323ba18b.mp4")).BaseStream) with
                Clock = true
        }
        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            let resFile = "works_clock.mp4"
            do! File.WriteAllBytesAsync(resFile, res.ToArray())

            res.Position <- 0
            let! res = FFmpeg.getStreamsInfo res
            match res with
            | Result.Ok res ->
                Assert.True(File.Exists(resFile) && res.Length = 1)
            | Result.Error err ->
                Assert.Fail(err)
        | Result.Error err ->
            Assert.Fail(err)
    }

    let [<Test>] ``Get file info``() = task {
        let! res = FFmpeg.getStreamsInfo (new StreamReader("VID_20221007_163400_126.mp4")).BaseStream

        match res with
        | Result.Ok res ->
            Assert.AreEqual(2, res.Length)
        | Result.Error err ->
            Assert.Fail(err)
    }

type ImageMagickObjectState = {
    Src: Stream
}

module ImageMagick =
    let convert (data: ImageMagickObjectState) = task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let inputFile = Path.getSynthName ".mp4"
        let outFile = Path.getSynthName ".mp4"

        data.Src.Position <- 0
        let memSrc = new MemoryStream()
        do! data.Src.CopyToAsync(memSrc)
        do! File.WriteAllBytesAsync(inputFile, memSrc.ToArray())

        let! executionResult =
            "magick"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withArguments [$"{inputFile} -liquid-rescale 320x640 -implode 0.25 {outFile}"] (ValueSome false)
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        let outStream = new StreamReader(outFile)
        do! outStream.BaseStream.CopyToAsync(target)
        File.deleteOrIgnore [inputFile; outFile]

        if executionResult.ExitCode = 0 then
            return target |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

module ImageMagickTests =
    let [<Test>] ``liquid rescale works properly``() = task {
        let sr = new StreamReader("VID_20221007_163400_126.mp4")
        let state = { Src = sr.BaseStream }
        let! res = ImageMagick.convert state
        match res with
        | Result.Ok res ->
            let res = res.ToArray()
            do! File.WriteAllBytesAsync("liquid_out.mp4", res)
            Assert.True(res.Length > 0)
        | Result.Error err ->
            Assert.Fail(err)
    }

type FfmpegGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte[] voption>
}

let ffmpegChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let reverseContent() =
    task {
        let log = Logger.get "startGifMagicDistortion"
        log.LogDebug("Spawned!")
        while true do
            let! { Tcs = tcs; Stream = stream; FileType = fileType } = ffmpegChannel.Reader.ReadAsync()
            let args = {
                FFmpegObjectState.Create(stream) with
                    VideoReverse = true
                    AudioReverse = true
                    RemoveAudio = fileType = FileType.Gif
            }

            let! res = FFmpeg.execute args
            match res with
            | Result.Ok res ->
                log.LogDebug("Reverse success: {length}", res.Length)
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Reverse fail: {err}", err)
                tcs.SetResult(ValueNone)
            do! Task.Delay(40)
    }

let clockChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let startClockFfmpeg() =
    task {
        let log = Logger.get "startClock"
        log.LogDebug("Spawned!")
        while true do
            let! { Tcs = tcs; Stream = stream; FileType = _ } = clockChannel.Reader.ReadAsync()
            let args = {
                FFmpegObjectState.Create(stream) with
                    Clock = true
            }
            let! res = FFmpeg.execute args
            match res with
            | Result.Ok res ->
                log.LogDebug("Reverse success: {length}", res.Length)
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Reverse fail: {err}", err)
                tcs.SetResult(ValueNone)
            do! Task.Delay(40)
    }

let cclockChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let startCClockFfmpeg() =
    task {
        let log = Logger.get "startCclock"
        log.LogDebug("Spawned!")
        while true do
            let! { Tcs = tcs; Stream = stream; FileType = _ } = cclockChannel.Reader.ReadAsync()
            let args = {
                FFmpegObjectState.Create(stream) with
                    CClock = true
            }
            let! res = FFmpeg.execute args
            match res with
            | Result.Ok res ->
                log.LogDebug("Reverse success: {length}", res.Length)
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Reverse fail: {err}", err)
                tcs.SetResult(ValueNone)
            do! Task.Delay(40)
    }


type MagicGifItem = {
    Stream: MemoryStream
    Tcs: TaskCompletionSource<byte[] voption>
    FileType: FileType
}

let magicChannel = Channel.CreateUnbounded<MagicGifItem>()

let startGifMagicDistortion() =
    task {
        let log = Logger.get "startGifMagicDistortion"
        log.LogDebug("Spawned!")
        while true do
            let! { Tcs = tcs; Stream = stream; FileType = _ } = magicChannel.Reader.ReadAsync()
            let! res = ImageMagick.convert { Src = stream }
            match res with
            | Result.Ok res ->
                log.LogDebug("Dist success: {length}", res.Length)
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Dist fail: {err}", err)
                tcs.SetResult(ValueNone)
            do! Task.Delay(40)
    }

type FfmpegVflipGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte[] voption>
}

let ffmpegVflipChannel = Channel.CreateUnbounded<FfmpegVflipGifItem>()

let startVflipGifFfmpeg() =
    task {
        let log = Logger.get "startVflipGifFfmpeg"
        log.LogDebug("Spawned!")
        while true do
            let! { Tcs = tcs; Stream = stream; FileType = _ } = ffmpegVflipChannel.Reader.ReadAsync()
            let args = {
                FFmpegObjectState.Create(stream) with
                    VerticalFlip = true
            }
            let! res = FFmpeg.execute args
            match res with
            | Result.Ok res ->
                log.LogDebug("Reverse success: {length}", res.Length)
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Reverse fail: {err}", err)
                tcs.SetResult(ValueNone)
            do! Task.Delay(40)
    }

type FfmpegHflipGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte[] voption>
}

let ffmpegHflipChannel = Channel.CreateUnbounded<FfmpegHflipGifItem>()

let startHflipGifFfmpeg() =
    task {
        let log = Logger.get "startHflipGifFfmpeg"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs; Stream = stream; FileType = _ } = ffmpegHflipChannel.Reader.ReadAsync()
            let args = {
                FFmpegObjectState.Create(stream) with
                    HorizontalFlip = true
            }
            let! res = FFmpeg.execute args
            match res with
            | Result.Ok res ->
                log.LogDebug("Reverse success: {length}", res.Length)
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Reverse fail: {err}", err)
                tcs.SetResult(ValueNone)
            do! Task.Delay(40)
    }

let mutable private cts = TaskCompletionSource()

let private spawn (lambda: Unit -> Task<_>) =
    let worker() =
        while cts.Task.IsCanceled |> not do
            lambda().Wait()
    let ts = ThreadStart(worker)
    let thread = Thread(ts)
    thread.Start()

let start() =
    if cts.Task.IsCompleted then
        cts <- TaskCompletionSource()
    if cts.Task.Status = TaskStatus.Running |> not then
        [ reverseContent
          startGifMagicDistortion
          startVflipGifFfmpeg
          startHflipGifFfmpeg
          startClockFfmpeg
          startCClockFfmpeg
        ] |> List.iter spawn

let stop() =
    cts.SetCanceled()
