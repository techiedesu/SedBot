module SedBot.ProcessingChannels

open System.IO
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open SedBot
open SedBot.Utilities
open Microsoft.Extensions.Logging

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
            let prams = ([$"-i {srcName} -y -qscale 0 {sound} -vf reverse {resName}"], false)
            let! res = Process.runStreamProcess "ffmpeg" prams resName
            tcs.SetResult(res)
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

            if fileType = Video |> not then
                let prams = (["mp4:- -liquid-rescale 320x640 -implode 0.25 mp4:-"], false)
                let! res = Process.runPipedStreamProcess "magick" stream prams
                tcs.SetResult(res)
            else
                let srcName = Utilities.Path.getSynthName extension
                let resNoSoundName = Utilities.Path.getSynthName extension
                let resSoundName = Utilities.Path.getSynthName extension
                do! File.WriteAllBytesAsync(srcName, stream.ToArray())

                let prams = ([$"{srcName} -liquid-rescale 320x640 -implode 0.25 {resNoSoundName}"], false)
                let! res = Process.runStreamProcess "magick" prams resNoSoundName
                match res with
                | ValueSome _ ->
                    // Extract sound
                    let soundName = Utilities.Path.getSynthName ".mp3"
                    let! resSound = Process.runStreamProcess "ffmpeg" ([|$"-i {srcName} -vn -map a {soundName}"|], false) soundName
                    match resSound with
                    | ValueSome _ ->
                        let! res = Process.runStreamProcess "ffmpeg" ([|$"-i {resNoSoundName} -i {soundName} -map 0 -map 1:a -c:v copy -shortest {resSoundName}"|], false) resSoundName
                        tcs.SetResult(res)
                    | _ ->
                        tcs.SetResult(ValueNone)
                | _ ->
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
            log.LogDebug("srcName: {srcName};; resName: {resName}")
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
            log.LogDebug("srcName: {srcName};; resName: {resName}")

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
