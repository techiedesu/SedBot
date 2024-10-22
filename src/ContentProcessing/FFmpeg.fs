[<RequireQualifiedAccess>]
module SedBot.ContentProcessing.FFmpeg

open System.IO

open System
open System.Collections.Generic
open System.Text

open SedBot.Common.TypeExtensions
open Tdesu.CliWrap.Fsharp
open FParsec

type FFmpegObjectState =
    { Src: Stream
      VideoReverse: bool
      AudioReverse: bool
      RemoveAudio: bool
      VerticalFlip: bool
      HorizontalFlip: bool
      Clock: bool
      CClock: bool
      Fix: bool
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
          IsPicture = false
          Fix = false }

type AudioVideoConcat =
    { VideoFileName: string
      AudioFileName: string }

type AudioDistortion = { AudioFileName: string }

type StreamsInfo = StreamInfo[]

and StreamInfo =
    { Kv: Dictionary<string, string>
      Index: int option
      CodecName: string option
      CodecLongName: string option }


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
                (optional newline) >>. (pstring "[STREAM]") >>. newline >>. kvPE1
                .>> (pstring "[/STREAM]")
                .>> (optional newline)

            let kvPE3 = many kvPE2

            match run kvPE3 (res.Trim()) with
            | Success(result, _, _) ->
                let res: StreamsInfo =
                    [| for res in result do
                           let dict = Dictionary(res |> List.map KeyValuePair)

                           { Index = dict |> Dictionary.tryGetValue "index" |> Option.bind Int32.tryParse
                             CodecName = dict |> Dictionary.tryGetValue "codec_name"
                             CodecLongName = dict |> Dictionary.tryGetValue "codec_long_name"
                             Kv = dict } |]

                return res |> Result.Ok
            | Failure(errorMsg, _, _) -> return errorMsg |> Result.Error
        else
            return string errSb |> Result.Error
    }

let voiceDistortion (data: AudioDistortion) =
    task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let outputFileName = Path.getSynthName ".ogg"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArguments
                [ "-i"
                  data.AudioFileName
                  "-ac"
                  "1 -map 0:a -strict -2 -acodec opus -b:a 128k -af vibrato=f=8:d=1"
                  outputFileName ]
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        if executionResult.ExitCode = 0 then
            use sr = new StreamReader(outputFileName)
            File.deleteign data.AudioFileName
            let ms = new MemoryStream()
            do! sr.BaseStream.CopyToAsync(ms)
            return (ms, outputFileName) |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

let audioDistortion (data: AudioDistortion) =
    task {
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
            File.deleteign data.AudioFileName
            let ms = new MemoryStream()
            do! sr.BaseStream.CopyToAsync(ms)
            return (ms, outputFileName) |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }

let appendAudioToVideoDistortion (data: AudioVideoConcat) =
    task {
        let target = new MemoryStream()
        let errSb = StringBuilder()

        let outputFileName = Path.getSynthName ".mp4"

        // TODO: enforce "-movflags +faststart"

        let! executionResult =
            "ffmpeg"
            |> wrap
            |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
            |> withStandardOutputPipe (PipeTarget.ToStream(target, ValueNone))
            |> withArgument
                $"-an -i {data.VideoFileName} -vn -i {data.AudioFileName} -strict -2 -c:a libopus -c:v libx264 -vf scale=out_range=full -color_range 2 -pix_fmt yuvj420p -af vibrato=f=6:d=1 -shortest {outputFileName}"
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        match executionResult.ExitCode with
        | 0 ->
            use sr = new StreamReader(outputFileName)
            File.deleteign data.AudioFileName
            File.deleteign data.VideoFileName
            let ms = new MemoryStream()
            do! sr.BaseStream.CopyToAsync(ms)
            return (ms, outputFileName) |> Result.Ok
        | _ -> return errSb.ToString() |> Result.Error
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

        let normalizeSize = ",scale=iw:ih:force_original_aspect_ratio=increase:force_divisible_by=2"

        let videoReverse =
            match data.VideoReverse with
            | true -> $" -vf \"reverse{normalizeSize}\" -color_range 2 -pix_fmt yuvj420p"
            | _ -> ""

        let vFlip =
            match data.VerticalFlip with
            | true -> $" -vf \"vflip{normalizeSize}\" -q:v 0 -color_range 2 -pix_fmt yuvj420p"
            | _ -> ""

        let hFlip =
            match data.HorizontalFlip with
            | true -> $" -vf \"hflip{normalizeSize}\" -q:v 0 -color_range 2 -pix_fmt yuvj420p"
            | _ -> ""

        let clock =
            match data.Clock with
            | true -> $" -vf \"transpose=clock{normalizeSize}\" -color_range 2 -pix_fmt yuvj420p"
            | _ -> ""

        let cClock =
            match data.CClock with
            | true -> $" -vf \"transpose=cclock{normalizeSize}\" -color_range 2 -pix_fmt yuvj420p"
            | _ -> ""

        // FFmpeg can't read moov (MPEG headers) at the end of a file when using a pipe. Have to "dump" to a filesystem.
        data.Src.Position <- 0
        let memSrc = new MemoryStream()
        do! data.Src.CopyToAsync(memSrc)

        let inputFile =
            if data.IsPicture then ".jpg" else ".mp4"
            |> Path.getSynthName

        do! File.WriteAllBytesAsync(inputFile, memSrc.ToArray())

        let args = $"{inputFile}{audioReverse}{videoReverse}{vFlip}{hFlip}{clock}{cClock}"

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
            |> withArguments [ $"-i {args} -strict -2 {contentSpecific} pipe:1" ]
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Console.OutputEncoding

        File.deleteign inputFile

        if executionResult.ExitCode = 0 then
            return target |> Result.Ok
        else
            return errSb.ToString() |> Result.Error
    }
