[<RequireQualifiedAccess>]
module SedBot.ContentProcessing.ImageMagick

open System.IO

open System
open System.Text

open SedBot.Common.TypeExtensions
open Tdesu.CliWrap.Fsharp

type ImageMagickObjectState = { Src: Stream
                                FileType: FileType }

let private preprocessWithFfmpeg isPicture (stream: Stream) = task {
    let args = { FFmpeg.FFmpegObjectState.Create(stream) with IsPicture = isPicture }
    let! res = FFmpeg.execute args
    return res
}

let convert (data: ImageMagickObjectState) = task {
    let target = new MemoryStream()
    let errSb = StringBuilder()

    let fileType =
        match data.FileType with
        | FileType.Picture -> ".jpg"
        | _ -> ".mp4"

    let inputFile = "in" + Path.getSynthName fileType
    let outFile = "out" + Path.getSynthName fileType

    data.Src.Position <- 0
    let! src = preprocessWithFfmpeg (data.FileType = FileType.Picture) data.Src
    let src = Result.get src
    src.Position <- 0

    let memSrc = new MemoryStream()
    do! src.CopyToAsync(memSrc)
    do! File.WriteAllBytesAsync(inputFile, memSrc.ToArray())

    let! executionResult =
        "magick"
        |> wrap
        |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
        |> withArguments [
            inputFile
            "-coalesce"
            "-scale"; "512x512"
            "-liquid-rescale"; "50%"
            "-scale"; "200%"
            outFile
        ]
        |> withValidation CommandResultValidation.None
        |> executeBufferedAsync Console.OutputEncoding

    if executionResult.ExitCode = 0 then
        let outStream = new StreamReader(outFile)
        do! outStream.BaseStream.CopyToAsync(target)

        File.deleteign inputFile

        return (target, outFile) |> Result.Ok
    else
        return string errSb |> Result.Error
}
