module SedBot.ProcessingChannels

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks

open SedBot.Common.TypeExtensions
open SedBot.Common.CliWrap
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
    Fix: bool
    IsPicture: bool
} with
    static member Create(src) =
        { Src = src
          VideoReverse = false
          AudioReverse = false
          RemoveAudio = false
          VerticalFlip = false
          HorizontalFlip = false
          Clock = false
          CClock = false
          IsPicture = false
          Fix = false }

type AudioVideoConcat = {
    VideoFileName: string
    AudioFileName: string
}

type AudioDistortion = {
    AudioFileName: string
}

type StreamsInfo = StreamInfo[]

and StreamInfo = {
    Kv: Dictionary<string, string>
    Index: int option
    CodecName: string option
    CodecLongName: string option
}

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
                |> withArgument "-i pipe: -show_streams"
                |> withValidation CommandResultValidation.None
                |> executeBufferedAsync Console.OutputEncoding

            do! stream.DisposeAsync()

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
                    let res: StreamsInfo = [|
                        for res in result do
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
                             Kv = dict }
                    |]

                    return res |> Result.Ok
                | Failure (errorMsg, _, _) -> return errorMsg |> Result.Error
            else
                return errSb.ToString() |> Result.Error

        }

    let voiceDistortion (data: AudioDistortion) = task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let outputFileName = Path.getSynthName ".ogg"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments [ "-i"; data.AudioFileName; "-ac"; "1 -map 0:a -strict -2 -acodec opus -b:a 128k -af vibrato=f=8:d=1"; outputFileName ]
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        if executionResult.ExitCode = 0 then
            use sr = new StreamReader(outputFileName)
            File.deleteUnit data.AudioFileName
            let ms = new MemoryStream()
            do! sr.BaseStream.CopyToAsync(ms)
            return (ms, outputFileName) |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

    let audioDistortion (data: AudioDistortion) = task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let outputFileName = Path.getSynthName ".mp3"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments [ $"-i {data.AudioFileName} -af vibrato=f=8:d=1 {outputFileName}" ]
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        if executionResult.ExitCode = 0 then
            use sr = new StreamReader(outputFileName)
            File.deleteUnit data.AudioFileName
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

        // TODO: enforce "-movflags +faststart"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArgument $"-an -i {data.VideoFileName} -vn -i {data.AudioFileName} -strict -2 -c:a libopus -c:v libx264 -vf scale=out_range=full -color_range 2 -pix_fmt yuvj420p -af vibrato=f=6:d=1 -shortest {outputFileName}"
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        if executionResult.ExitCode = 0 then
            use sr = new StreamReader(outputFileName)
            File.deleteUnit data.AudioFileName
            File.deleteUnit data.VideoFileName
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
                | true -> " -vf vflip -q:v 0 "
                | _ -> ""

            let hFlip =
                match data.HorizontalFlip with
                | true -> " -vf hflip -q:v 0 "
                | _ -> ""

            let clock =
                match data.Clock with
                | true -> " -vf \"transpose=clock\""
                | _ -> ""

            let cClock =
                match data.CClock with
                | true -> " -vf \"transpose=cclock\""
                | _ -> ""

            let defVf =
                " -vf scale=out_range=full -color_range 2 -pix_fmt yuvj420p"

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

            let args = $"{inputFile}{audioReverse}{videoReverse}{vFlip}{hFlip}{clock}{cClock}{defVf}"

            let contentSpecific =
                if data.IsPicture |> not then
                    // "-f mp4 -movflags frag_keyframe+empty_moov -vcodec libx264"
                    "-f mp4 -movflags frag_keyframe+empty_moov -c:a libopus -c:v libx264"
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

            File.deleteUnit inputFile

            if executionResult.ExitCode = 0 then
                return target |> Result.Ok
            else
                return errSb.ToString() |> Result.Error
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
                // "convert"
                "magick"
                |> wrap
                |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
                |> withArguments [ inputFile; "-scale"; "512x512>";"-liquid-rescale"; "50%"; "-scale"; "200%"; outFile ]
                |> withValidation CommandResultValidation.None
                |> executeBufferedAsync Console.OutputEncoding

            if executionResult.ExitCode = 0 then
                let outStream = new StreamReader(outFile)
                do! outStream.BaseStream.CopyToAsync(target)

                File.deleteUnit inputFile

                return (target, outFile) |> Result.Ok
            else
                return errSb.ToString() |> Result.Error
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
                    | Result.Error _ ->
                        return res
                | Voice ->
                    let inputFile = Path.getSynthName ".ogg"
                    stream.Position <- 0
                    let memSrc = new MemoryStream()
                    do! stream.CopyToAsync(memSrc)
                    let memSrc' = memSrc.ToArray()
                    do! File.WriteAllBytesAsync(inputFile, memSrc')
                    return! FFmpeg.voiceDistortion { AudioFileName = inputFile }
                | Audio ->
                    let inputFile = Path.getSynthName ".mp3"
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

                File.deleteUnit outFileName
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

let rec startVflipFfmpeg () =
    task {
        let log = Logger.get ^ nameof startVflipFfmpeg
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

let rec private spawn (lambda: Unit -> #Task) =
    let logger = Logger.get ^ nameof spawn

    let worker () =
        while cts.Task.IsCanceled |> not do
            try
                lambda().Wait()
            with
            | ex -> logger.LogError("worker task crashed. restarting... ex: {ex}", ex)

    let ts = ThreadStart(worker)
    let thread = Thread(ts)
    thread.IsBackground <- true
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
