module SedBot.Commands.Handlers

open System.IO
open System.Threading.Tasks
open SedBot.ProcessingChannels
open SedBot.Common

let sed data expression =
    Process.runTextProcess "sed" [| "-E"; expression |] data

let jq data expression =
    Process.runTextProcess "jq" [| "-M"; expression |] data

let reverse fileType (stream: Stream) = task {
    use ms = new MemoryStream()
    do! stream.CopyToAsync(ms)
    let tcs = TaskCompletionSource<byte[] voption>()
    do! ffmpegChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
    do! stream.DisposeAsync()
    return! tcs.Task
}

let hFlip fileType (stream: Stream) = task {
    use ms = new MemoryStream()
    do! stream.CopyToAsync(ms)
    let tcs = TaskCompletionSource<byte[] voption>()
    do! ffmpegHflipChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
    do! stream.DisposeAsync()
    return! tcs.Task
}

let vFlip fileType (stream: Stream) = task {
    use ms = new MemoryStream()
    do! stream.CopyToAsync(ms)
    let tcs = TaskCompletionSource<byte[] voption>()
    do! ffmpegVflipChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
    do! stream.DisposeAsync()
    return! tcs.Task
}

let distort fileType (stream: Stream) = task {
    use ms = new MemoryStream()
    do! stream.CopyToAsync(ms)
    let tcs = TaskCompletionSource<byte[] voption>()
    do! magicChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
    do! stream.DisposeAsync()
    return! tcs.Task
}

let clock fileType (stream: Stream) = task {
    use ms = new MemoryStream()
    do! stream.CopyToAsync(ms)
    let tcs = TaskCompletionSource<byte[] voption>()
    do! clockChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
    do! stream.DisposeAsync()
    return! tcs.Task
}

let cclock fileType (stream: Stream) = task {
    use ms = new MemoryStream()
    do! stream.CopyToAsync(ms)
    let tcs = TaskCompletionSource<byte[] voption>()
    do! cclockChannel.Writer.WriteAsync({ Stream = ms; Tcs = tcs; FileType = fileType })
    do! stream.DisposeAsync()
    return! tcs.Task
}
