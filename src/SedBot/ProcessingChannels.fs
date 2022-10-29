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
    VideoReverse: bool voption
    AudioReverse: bool voption
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
            let letterOrDigitPE c = isLetter c || isDigit c || isAnyOf "[]:.-_/() " c
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
                            Index = dict |> Seq.tryFind (It.KeyIs "index") |> Option.map (It.Value >> int)
                            CodecName = dict |> Seq.tryFind (It.KeyIs "codec_name") |> Option.map It.Key
                            Kv = Dictionary(res |> List.map KeyValuePair)
                            CodecLongName = dict |> Seq.tryFind (It.KeyIs "codec_long_name") |> Option.map It.Key
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
            match data.AudioReverse with
            | ValueSome true ->
                 " -af areverse"
            | _ ->
                ""

        let videoReverse =
            match data.VideoReverse with
            | ValueSome true ->
                 " -vf reverse"
            | _ ->
                ""

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardInputPipe (PipeSource.FromStream data.Src)
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments [$"-f mp4 -i pipe:{audioReverse}{videoReverse} -f mp4 -movflags frag_keyframe+empty_moov pipe:1"] (ValueSome false)
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding
        if executionResult.ExitCode = 0 then
            return target |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

module Tests =

    let [<Test>] ``Reverse audio and video works properly``() = task {
        let args = {
            Src = (new StreamReader("VID_20221007_163400_126.mp4")).BaseStream
            AudioReverse = ValueSome true
            VideoReverse = ValueSome true
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

    let [<Test>] ``Get file info``() = task {
        let! res = FFmpeg.getStreamsInfo (new StreamReader("VID_20221007_163400_126.mp4")).BaseStream

        match res with
        | Result.Ok res ->
            Assert.AreEqual(2, res.Length)
        | Result.Error err ->
            Assert.Fail(err)
    }

type FfmpegGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte[] voption>
}

let ffmpegChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let startGifFfmpeg() = // TODO: Use pipes
    task {
        let log = Logger.get "startGifMagicDistortion"
        log.LogDebug("Spawned!")
        while true do
            let! { Tcs = tcs; Stream = stream; FileType = fileType } = ffmpegChannel.Reader.ReadAsync()
            let extension = extension fileType
            let srcName = Utilities.Path.getSynthName extension
            let resName = Utilities.Path.getSynthName extension
            log.LogDebug("srcName: {srcName};; resName: {resName}", srcName, resName)
            do! File.WriteAllBytesAsync(srcName, stream.ToArray())
            let sound =
                if fileType = FileType.Video then
                    "-af areverse"
                else
                    "-an"
            let prams = ([$"-i {srcName} -y {sound} -vf reverse {resName}"], false)
            let! res = Process.runStreamProcess "ffmpeg" prams resName
            tcs.SetResult(res)
            File.deleteOrIgnore [srcName; resName]
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
            let! { Tcs = tcs; Stream = stream; FileType = fileType } = magicChannel.Reader.ReadAsync()
            stream.Position <- 0

            let extension = extension fileType

            let srcName = Utilities.Path.getSynthName extension
            let resName = Utilities.Path.getSynthName extension
            do! File.WriteAllBytesAsync(srcName, stream.ToArray())
            let prams = ([$"{srcName} -liquid-rescale 320x640 -implode 0.25 {resName}"], false)
            let! res = Process.runPipedStreamProcess "magick" stream prams
            File.deleteOrIgnore [srcName; resName]
            tcs.SetResult(res)
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
            let! { Tcs = tcs; Stream = stream; FileType = fileType } = ffmpegVflipChannel.Reader.ReadAsync()
            let extension = extension fileType
            let srcName = Utilities.Path.getSynthName extension
            let resName = Utilities.Path.getSynthName extension
            log.LogDebug("srcName: {srcName};; resName: {resName}", srcName, resName)
            do! File.WriteAllBytesAsync(srcName, stream.ToArray())
            let sound =
                if fileType = FileType.Gif then
                    "-an"
                else
                    ""
            let prams = ([$"-i {srcName} -y -vf vflip -qscale 0 {sound} {resName}"], false)
            let! res = Process.runStreamProcess "ffmpeg" prams resName
            tcs.SetResult(res)
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
            let! { Tcs = tcs; Stream = stream; FileType = fileType } = ffmpegHflipChannel.Reader.ReadAsync()
            let extension = extension fileType
            let srcName = Utilities.Path.getSynthName extension
            let resName = Utilities.Path.getSynthName extension
            log.LogDebug("srcName: {srcName};; resName: {resName}", srcName, resName)

            do! File.WriteAllBytesAsync(srcName, stream.ToArray())
            let sound =
                match fileType with
                | FileType.Gif ->
                    "-an"
                | FileType.Sticker ->
                    "-lossless true"
                | _ ->
                    ""

            let prams = ([$"-i {srcName} -y -vf hflip -qscale 0 {sound} {resName}"], false)
            let! res = Process.runStreamProcess "ffmpeg" prams resName
            tcs.SetResult(res)
            File.deleteOrIgnore [sound; resName]
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
        [ startGifFfmpeg
          startGifMagicDistortion
          startVflipGifFfmpeg
          startHflipGifFfmpeg
        ] |> List.iter spawn

let stop() =
    cts.SetCanceled()
