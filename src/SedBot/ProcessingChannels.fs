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

type FFmpegObjectState =
    { Src: Stream
      VideoReverse: bool
      AudioReverse: bool
      RemoveAudio: bool
      VerticalFlip: bool
      HorizontalFlip: bool
      Clock: bool
      CClock: bool
      IsPicture: bool }
    static member Create(src) =
        { Src = src
          VideoReverse = false
          AudioReverse = false
          RemoveAudio = false
          VerticalFlip = false
          HorizontalFlip = false
          Clock = false
          CClock = false
          IsPicture = false }

type AudioVideoConcat = {
    VideoFileName: string
    AudioFileName: string
}

type AudioDistortion = {
    AudioFileName: string
}

type StreamsInfo = StreamInfo array

and StreamInfo =
    { Kv: Dictionary<string, string>
      Index: int option
      CodecName: string option
      CodecLongName: string option }

module FFmpeg =
    let getStreamsInfo (stream: Stream) =
        task {
            let errSb = StringBuilder()
            let resSb = StringBuilder()

            let! executionResult =
                "ffprobe"
                |> wrap
                |> withStandardInputPipe (PipeSource.FromStream stream)
                |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
                |> withStandardOutputPipe (PipeTarget.ToStringBuilder(resSb))
                |> withArguments [ "-i pipe: -show_streams" ]
                |> withValidation CommandResultValidation.None
                |> executeBufferedAsync Console.OutputEncoding

            if executionResult.ExitCode = 0 then
                let res = resSb.ToString()
                let letterOrDigitPE c = isNoneOf "=\n\r" c

                let kvPEr =
                    many1Satisfy2 (fun c -> letterOrDigitPE c && (c = '[' |> not)) letterOrDigitPE

                let valuePE = many1SatisfyL letterOrDigitPE "value"

                let kvPE = kvPEr .>> (pstring "=") .>>. valuePE

                let kvPE1 = many (kvPE .>> (optional newline))

                let kvPE2 =
                    (optional newline)
                    >>. (pstring "[STREAM]")
                    >>. newline
                    >>. kvPE1
                    .>> (pstring "[/STREAM]")
                    .>> (optional newline)

                let kvPE3 = many kvPE2

                match run kvPE3 (res.Trim()) with
                | Success (result, _, _) ->
                    let res: StreamsInfo =
                        [| for res in result do
                               let dict = Dictionary(res |> List.map KeyValuePair)

                               { Index =
                                   dict
                                   |> Seq.tryFind ^ It.KeyIs "index"
                                   |> Option.map (It.Value >> int)
                                 CodecName =
                                   dict
                                   |> Seq.tryFind ^ It.KeyIs "codec_name"
                                   |> Option.map It.Value
                                 CodecLongName =
                                   dict
                                   |> Seq.tryFind ^ It.KeyIs "codec_long_name"
                                   |> Option.map It.Value
                                 Kv = dict } |]

                    return res |> Result.Ok
                | Failure (errorMsg, _, _) -> return errorMsg |> Result.Error
            else
                return errSb.ToString() |> Result.Error

        }

    let audioDistortion (data: AudioDistortion) = task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let outputFileName = Path.getSynthName ".ogg"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments [ $"-i {data.AudioFileName} -ac 1 -map 0:a -strict -2 -acodec opus -b:a 128k -af vibrato=f=6:d=1 {outputFileName}" ]
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        if executionResult.ExitCode = 0 then
            use sr = new StreamReader(outputFileName)
            File.deleteOrIgnore [ data.AudioFileName ]
            let ms = new MemoryStream()
            do! sr.BaseStream.CopyToAsync(ms)
            return (ms, outputFileName) |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

    let appendAudioToVideoDistortion (data: AudioVideoConcat) = task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let outputFileName = Path.getSynthName ".mp4"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments [ $"-an -i {data.VideoFileName} -vn -i {data.AudioFileName} -c:a libopus -c:v copy -af vibrato=f=6:d=1 -shortest {outputFileName}" ]
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        if executionResult.ExitCode = 0 then
            use sr = new StreamReader(outputFileName)
            File.deleteOrIgnore [ data.AudioFileName; data.VideoFileName ]
            let ms = new MemoryStream()
            do! sr.BaseStream.CopyToAsync(ms)
            return (ms, outputFileName) |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

    let execute (data: FFmpegObjectState) =
        task {
            let target = new MemoryStream()
            let errSb = StringBuilder()

            let audioReverse =
                match data.AudioReverse, data.RemoveAudio with
                | true, false -> " -af areverse"
                | _, true -> " -an"
                | _, _ -> ""

            let videoReverse =
                match data.VideoReverse with
                | true -> " -vf reverse"
                | _ -> ""

            let vFlip =
                match data.VerticalFlip with
                | true -> " -vf vflip -qscale 0"
                | _ -> ""

            let hFlip =
                match data.HorizontalFlip with
                | true -> " -vf hflip -qscale 0"
                | _ -> ""

            let clock =
                match data.Clock with
                | true -> " -vf \"transpose=clock\""
                | _ -> ""

            let cClock =
                match data.CClock with
                | true -> " -vf \"transpose=cclock\""
                | _ -> ""

            // FFmpeg can't read moov (MPEG headers) at the end of a file when using a pipe. Have to "dump" to a filesystem.
            data.Src.Position <- 0
            let memSrc = new MemoryStream()
            do! data.Src.CopyToAsync(memSrc)

            let inputFile =
                if data.IsPicture then
                    ".jpg"
                else
                    ".mp4"
                |> Path.getSynthName

            do! File.WriteAllBytesAsync(inputFile, memSrc.ToArray())

            let args = $"{inputFile}{audioReverse}{videoReverse}{vFlip}{hFlip}{clock}{cClock}"

            let contentSpecific =
                if data.IsPicture |> not then
                    "-f mp4 -movflags frag_keyframe+empty_moov -vcodec libx264"
                else
                    "-f mjpeg"

            let! executionResult =
                "ffmpeg"
                |> wrap
                |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
                |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
                |> withArguments [ $"-i {args} {contentSpecific} pipe:1" ]
                |> withValidation CommandResultValidation.None
                |> executeBufferedAsync Console.OutputEncoding

            File.deleteOrIgnore [ inputFile ]

            if executionResult.ExitCode = 0 then
                return target |> Result.Ok
            else
                return errSb.ToString() |> Result.Error
        }

module Tests =

    [<Test>]
    let ``Reverse audio and video works properly`` () =
        task {
            let args =
                { FFmpegObjectState.Create(
                      (new StreamReader("VID_20221007_163400_126.mp4"))
                          .BaseStream
                  ) with
                    AudioReverse = true
                    VideoReverse = true }

            let! res = FFmpeg.execute args

            match res with
            | Result.Ok res ->
                let resFile = "works.mp4"
                do! File.WriteAllBytesAsync(resFile, res.ToArray())

                res.Position <- 0
                let! res = FFmpeg.getStreamsInfo res

                match res with
                | Result.Ok _ ->
                    Assert.True(
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
                | Result.Ok res -> Assert.True(File.Exists(resFile) && res.Length = 1)
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
                | Result.Ok res -> Assert.True(File.Exists(resFile) && res.Length = 2)
                | Result.Error err -> Assert.Fail(err)
            | Result.Error err -> Assert.Fail(err)
        }

    [<Test>]
    let ``No audio with reverse works properly`` () =
        task {
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
                | Result.Ok res -> Assert.True(File.Exists(resFile) && res.Length = 1)
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
                | Result.Ok res -> Assert.True(File.Exists(resFile) && res.Length = 1)
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
            | Result.Ok res -> Assert.AreEqual(2, res.Length)
            | Result.Error err -> Assert.Fail(err)
        }

type ImageMagickObjectState = { Src: Stream
                                FileType: FileType }

module ImageMagick =
    let convert (data: ImageMagickObjectState) =
        task {
            let target = new MemoryStream()
            let errSb = StringBuilder()

            let fileType =
                match data.FileType with
                | FileType.Picture -> ".jpg"
                | _ -> ".mp4"

            let inputFile = "in" + Path.getSynthName fileType
            let outFile = "out" + Path.getSynthName fileType

            data.Src.Position <- 0
            let memSrc = new MemoryStream()
            do! data.Src.CopyToAsync(memSrc)
            do! File.WriteAllBytesAsync(inputFile, memSrc.ToArray())

            let! executionResult =
                "convert"
                |> wrap
                |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
                // |> withArguments [ $"{inputFile} -scale\", \"512x512> {outFile}" ] (ValueSome false)
                |> withArguments [ inputFile; "-scale"; "512x512>";"-liquid-rescale"; "50%"; "-scale"; "200%"; outFile ]
                |> withValidation CommandResultValidation.None
                |> executeBufferedAsync Console.OutputEncoding

            if executionResult.ExitCode = 0 then
                let outStream = new StreamReader(outFile)
                do! outStream.BaseStream.CopyToAsync(target)

                File.deleteOrIgnore [ inputFile ]

                return (target, outFile) |> Result.Ok
            else
                return errSb.ToString() |> Result.Error
        }

module ImageMagickTests =
    [<Test>]
    let ``liquid rescale works properly`` () =
        task {
            let sr = new StreamReader("VID_20221007_163400_126.mp4")

            let state =
                { Src = sr.BaseStream
                  FileType = FileType.Video }

            let! res = ImageMagick.convert state

            match res with
            | Result.Ok (res, fileName) ->
                File.deleteOrIgnore [fileName]
                let res = res.ToArray()
                do! File.WriteAllBytesAsync("liquid_out.mp4", res)
                Assert.True(res.Length > 0)
            | Result.Error err -> Assert.Fail(err)
        }

type FfmpegGifItem =
    { Stream: MemoryStream
      FileType: FileType
      Tcs: TaskCompletionSource<byte [] voption> }

let ffmpegChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let reverseContent () =
    task {
        let log = Logger.get "startReverse"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs
                   Stream = stream
                   FileType = fileType } = ffmpegChannel.Reader.ReadAsync()

            let args =
                { FFmpegObjectState.Create(stream) with
                    VideoReverse = true
                    AudioReverse = true
                    RemoveAudio = fileType = FileType.Gif
                    IsPicture = fileType = FileType.Picture }

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

let startClockFfmpeg () =
    task {
        let log = Logger.get "startClock"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs
                   Stream = stream
                   FileType = fileType } = clockChannel.Reader.ReadAsync()

            let args =
                { FFmpegObjectState.Create(stream) with
                    Clock = true
                    RemoveAudio = fileType = FileType.Gif
                    IsPicture = fileType = FileType.Picture }

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

let startCClockFfmpeg () =
    task {
        let log = Logger.get "startCclock"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs
                   Stream = stream
                   FileType = fileType } = cclockChannel.Reader.ReadAsync()

            let args =
                { FFmpegObjectState.Create(stream) with
                    CClock = true
                    RemoveAudio = fileType = FileType.Gif
                    IsPicture = fileType = FileType.Picture }

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


type MagicGifItem =
    { Stream: MemoryStream
      Tcs: TaskCompletionSource<byte [] voption>
      FileType: FileType }

let magicChannel = Channel.CreateUnbounded<MagicGifItem>()

let startMagicDistortion () =
    task {
        let log = Logger.get "startMagicDistortion"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs
                   Stream = stream
                   FileType = fileType } = magicChannel.Reader.ReadAsync()

            let! res = task {
                match fileType with
                | FileType.Video ->
                    let inputFile = Path.getSynthName ".mp4"
                    stream.Position <- 0
                    let memSrc = new MemoryStream()
                    do! stream.CopyToAsync(memSrc)
                    let memSrc' = memSrc.ToArray()
                    do! File.WriteAllBytesAsync(inputFile, memSrc')
                    let! res = ImageMagick.convert { Src = stream; FileType = fileType }
                    match res with
                    | Result.Ok (_, distResultFileName) ->
                        let! res = FFmpeg.appendAudioToVideoDistortion { VideoFileName = distResultFileName; AudioFileName = inputFile }
                        return res
                    | _ ->
                        return res
                | Voice ->
                    let inputFile = Path.getSynthName ".ogg"
                    stream.Position <- 0
                    let memSrc = new MemoryStream()
                    do! stream.CopyToAsync(memSrc)
                    let memSrc' = memSrc.ToArray()
                    do! File.WriteAllBytesAsync(inputFile, memSrc')
                    return! FFmpeg.audioDistortion { AudioFileName = inputFile }
                | _ ->
                    return! ImageMagick.convert { Src = stream; FileType = fileType }
            }

            match res with
            | Result.Ok (res, outFileName) ->
                log.LogDebug("Dist success: {length}", res.Length)

                File.deleteOrIgnore [outFileName]
                tcs.SetResult(res.ToArray() |> ValueSome)
            | Result.Error err ->
                log.LogDebug("Dist fail: {err}", err)
                tcs.SetResult(ValueNone)

            do! Task.Delay(40)
    }

type FfmpegVflipGifItem =
    { Stream: MemoryStream
      FileType: FileType
      Tcs: TaskCompletionSource<byte [] voption> }

let ffmpegVflipChannel = Channel.CreateUnbounded<FfmpegVflipGifItem>()

let startVflipFfmpeg () =
    task {
        let log = Logger.get "startVflipGifFfmpeg"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs
                   Stream = stream
                   FileType = _ } = ffmpegVflipChannel.Reader.ReadAsync()

            let args = { FFmpegObjectState.Create(stream) with VerticalFlip = true }
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

type FfmpegHflipGifItem =
    { Stream: MemoryStream
      FileType: FileType
      Tcs: TaskCompletionSource<byte [] voption> }

let ffmpegHflipChannel = Channel.CreateUnbounded<FfmpegHflipGifItem>()

let startHflipFfmpeg () =
    task {
        let log = Logger.get "HorizontalFlipProcessor"
        log.LogDebug("Spawned!")

        while true do
            let! { Tcs = tcs
                   Stream = stream
                   FileType = _ } = ffmpegHflipChannel.Reader.ReadAsync()

            let args = { FFmpegObjectState.Create(stream) with HorizontalFlip = true }
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

let private spawn (lambda: Unit -> #Task) =
    let worker () =
        while cts.Task.IsCanceled |> not do
            lambda().Wait()

    let ts = ThreadStart(worker)
    let thread = Thread(ts)
    thread.Start()

let start () =
    if cts.Task.IsCompleted then
        cts <- TaskCompletionSource()

    if cts.Task.Status = TaskStatus.Running |> not then
        [ reverseContent
          startMagicDistortion
          startVflipFfmpeg
          startHflipFfmpeg
          startClockFfmpeg
          startCClockFfmpeg ]
        |> List.iter spawn

let stop () = cts.SetCanceled()
