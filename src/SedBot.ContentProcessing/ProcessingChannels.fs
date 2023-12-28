module SedBot.ProcessingChannels

open System.IO
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks

open SedBot.Common.TypeExtensions
open SedBot.ContentProcessing
open SedBot.Common.Utilities

open Microsoft.Extensions.Logging

let private cts = TaskCompletionSource()

type FfmpegGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte [] voption>
}

let ffmpegChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let rec reverseContent () =
    let log = Logger.get ^ nameof reverseContent
    log.LogDebug("Spawned!")

    let rec loop () = task {
        let! { Tcs = tcs
               Stream = stream
               FileType = fileType } = ffmpegChannel.Reader.ReadAsync()

        let args =
            { FFmpeg.FFmpegObjectState.Create(stream) with
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
        if cts.Task.IsCanceled |> not then
            do! loop()
    }
    loop()

let clockChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let rec startClockFfmpeg () =
    let log = Logger.get ^ nameof startClockFfmpeg
    log.LogDebug("Spawned!")

    let rec loop () =  task {
        let! { Tcs = tcs
               Stream = stream
               FileType = fileType } = clockChannel.Reader.ReadAsync()

        let args =
            { FFmpeg.FFmpegObjectState.Create(stream) with
                Clock = true
                RemoveAudio = fileType = FileType.Gif
                IsPicture = fileType = FileType.Picture }

        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            log.LogDebug("Reverse success: {length} bytes", res.Length)
            tcs.SetResult(res.ToArray() |> ValueSome)

        | Result.Error err ->
            log.LogDebug("Reverse fail: {err}", err)
            tcs.SetResult(ValueNone)

        do! Task.Delay(40)
        if cts.Task.IsCanceled |> not then
            do! loop()
    }
    loop()

let cclockChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let rec startCClockFfmpeg () =
    let log = Logger.get ^ nameof startCClockFfmpeg
    log.LogDebug("Spawned!")

    let rec loop () = task {
        let! { Tcs = tcs
               Stream = stream
               FileType = fileType } = cclockChannel.Reader.ReadAsync()

        let args =
            { FFmpeg.FFmpegObjectState.Create(stream) with
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
        if cts.Task.IsCanceled |> not then
            do! loop()
    }
    loop()


type MagicGifItem =
    { Stream: MemoryStream
      Tcs: TaskCompletionSource<byte [] voption>
      FileType: FileType }

let magicChannel = Channel.CreateUnbounded<MagicGifItem>()

let rec startMagicDistortion () =
    let log = Logger.get ^ nameof startMagicDistortion
    log.LogDebug("Spawned!")

    let rec loop () = task {
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

            File.deleteign outFileName
            tcs.SetResult(res.ToArray() |> ValueSome)
        | Result.Error err ->
            log.LogDebug("Dist fail: {err}", err)
            tcs.SetResult(ValueNone)

        do! Task.Delay(40)
        if cts.Task.IsCanceled |> not then
            do! loop()
    }
    loop()

type FfmpegVflipGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte [] voption>
}

let ffmpegVflipChannel = Channel.CreateUnbounded<FfmpegVflipGifItem>()

let rec startVflipFfmpeg () =
    let log = Logger.get ^ nameof startVflipFfmpeg
    log.LogDebug("Spawned!")

    let rec loop () = task {
        let! { Tcs = tcs
               Stream = stream
               FileType = _ } = ffmpegVflipChannel.Reader.ReadAsync()

        let args = { FFmpeg.FFmpegObjectState.Create(stream) with VerticalFlip = true }
        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            log.LogDebug("Reverse success: {length}", res.Length)
            tcs.SetResult(res.ToArray() |> ValueSome)
        | Result.Error err ->
            log.LogDebug("Reverse fail: {err}", err)
            tcs.SetResult(ValueNone)

        do! Task.Delay(40)
        if cts.Task.IsCanceled |> not then
            do! loop()
    }
    loop()

type FfmpegHflipGifItem = {
    Stream: MemoryStream
    FileType: FileType
    Tcs: TaskCompletionSource<byte [] voption>
}

let ffmpegHflipChannel = Channel.CreateUnbounded<FfmpegHflipGifItem>()

let rec startHflipFfmpeg () =
    let log = Logger.get ^ nameof startHflipFfmpeg
    log.LogDebug("Spawned!")

    let rec loop () = task {
        let! { Tcs = tcs; Stream = stream} = ffmpegHflipChannel.Reader.ReadAsync()

        let args = { FFmpeg.FFmpegObjectState.Create(stream) with HorizontalFlip = true }
        let! res = FFmpeg.execute args

        match res with
        | Result.Ok res ->
            log.LogDebug("Reverse success: {length}", res.Length)
            tcs.SetResult(res.ToArray() |> ValueSome)
        | Result.Error err ->
            log.LogDebug("Reverse fail: {err}", err)
            tcs.SetResult(ValueNone)

        do! Task.Delay(40)
        if cts.Task.IsCanceled |> not then
            do! loop()
    }
    loop()

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
    if cts.Task.Status = TaskStatus.Running |> not then
        [ reverseContent
          startMagicDistortion
          startVflipFfmpeg
          startHflipFfmpeg
          startClockFfmpeg
          startCClockFfmpeg ]
        |> List.iter spawn

let stop () = cts.SetCanceled()
