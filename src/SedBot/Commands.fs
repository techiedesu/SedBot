module SedBot.Commands.Handlers

open System.IO
open System.Threading.Tasks
open SedBot.ProcessingChannels

let sed data expression =
    Process.runTextProcess "sed" [| "-E"; expression |] data

let jq data expression =
    Process.runTextProcess "jq" [| "-M"; expression |] data

let reverse (stream: Stream) fileType =
    task {
        if stream.CanRead then
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            let tcs = TaskCompletionSource<byte[] voption>()
            do! ffmpegChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
            do! stream.DisposeAsync()
            return! tcs.Task
        else
            return ValueNone
    }

let hFlip (stream: Stream) fileType =
    task {
        if stream.CanRead then
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            let tcs = TaskCompletionSource<byte[] voption>()
            do! ffmpegHflipChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
            do! stream.DisposeAsync()
            return! tcs.Task
        else
            return ValueNone
    }

let vFlip (stream: Stream) fileType =
    task {
        if stream.CanRead then
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            let tcs = TaskCompletionSource<byte[] voption>()
            do! ffmpegVflipChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
            do! stream.DisposeAsync()
            return! tcs.Task
        else
            return ValueNone
    }

let distort (stream: Stream) fileType =
    task {
        if stream.CanRead then
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            let tcs = TaskCompletionSource<byte[] voption>()
            do! magicChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
            do! stream.DisposeAsync()
            return! tcs.Task
        else
            return ValueNone
    }

let clock (stream: Stream) fileType =
    task {
        if stream.CanRead then
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            let tcs = TaskCompletionSource<byte[] voption>()
            do! clockChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
            do! stream.DisposeAsync()
            return! tcs.Task
        else
            return ValueNone
    }

let cclock (stream: Stream) fileType =
    task {
        if stream.CanRead then
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            let tcs = TaskCompletionSource<byte[] voption>()
            do! cclockChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
            do! stream.DisposeAsync()
            return! tcs.Task
        else
            return ValueNone
    }
