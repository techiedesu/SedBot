module SedBot.ProcessingChannels

open System.IO
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks

type FfmpegGifItem = {
    Stream: MemoryStream
    Tcs: TaskCompletionSource<byte[] voption>
}

let ffmpegChannel = Channel.CreateUnbounded<FfmpegGifItem>()

let startGifFfmpeg() = // TODO: Use pipes
    task {
        while true do
            let! { Tcs = tcs; Stream = stream } = ffmpegChannel.Reader.ReadAsync()
            let srcName = Utilities.Path.getSynthName ".mp4"
            let resName = Utilities.Path.getSynthName ".mp4"
            do! File.WriteAllBytesAsync(srcName, stream.ToArray())
            let prams = ([$"-i {srcName} -y -qscale 0 -an -vf reverse {resName}"], false)
            let! res = Utilities.runStreamProcess "ffmpeg" prams resName
            tcs.SetResult(res)
            do! Task.Delay(40)
    }

type MagicGifItem = {
    Stream: MemoryStream
    Tcs: TaskCompletionSource<byte[] voption>
}

let magicChannel = Channel.CreateUnbounded<MagicGifItem>()

let startGifMagicDistortion() =
    task {
        while true do
            let! { Tcs = tcs; Stream = stream } = magicChannel.Reader.ReadAsync()
            let srcName = Utilities.Path.getSynthName ".mp4"
            let resName = Utilities.Path.getSynthName ".mp4"
            do! File.WriteAllBytesAsync(srcName, stream.ToArray())

            let prams = ([$"{srcName} -liquid-rescale 320x640 -implode 0.25 {resName}"], false)
            let! res = Utilities.runStreamProcess "magick" prams resName
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
        [ startGifFfmpeg; startGifMagicDistortion ] |> List.iter spawn

let stop() =
    cts.SetCanceled()
