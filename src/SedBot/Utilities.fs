module SedBot.Utilities

open SedBot.CliWrap

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open Microsoft.Extensions.Logging
open Serilog
open Serilog.Extensions.Logging

let platformed command args =
    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        "wsl", args |> Seq.append (Seq.singleton command)
    else
        command, args

[<RequireQualifiedAccess>]
module Logger =
    let private factory =
        let logger =
            LoggerConfiguration()
                  .Enrich.FromLogContext()
                  .MinimumLevel.Debug()
                  .WriteTo.Console(outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}")
                  .CreateLogger()
        Log.Logger <- logger

        LoggerFactory.Create(
            fun builder ->
                builder
                    .ClearProviders()
                    .AddProvider(new SerilogLoggerProvider())
                    .SetMinimumLevel(LogLevel.Debug) |> ignore
        )

    let get name =
        factory.CreateLogger(name)

module Path =
    let getSynthName extension =
        Guid.NewGuid().ToString().Replace("-", "") + extension

module Process =
    let private log = Logger.get "SedBot.Utilities.Process"

    let runTextProcess procName (args: string seq) data =
        let procName, args = platformed procName args
        task {
            log.LogDebug(
                "runTextProcess: proccess name: {procName};; args: {args};; data: {data}",
                procName, args, data
            )

            let stdout = StringBuilder()
            let stderr = StringBuilder()

            let! executionResult =
                procName
                |> wrap
                |> withArguments args ValueNone
                |> withStandardInputPipe (data |> PipeSource.FromString)
                |> withStandardErrorPipe (stderr |> PipeTarget.ToStringBuilder)
                |> withStandardOutputPipe (stdout |> PipeTarget.ToStringBuilder)
                |> withValidation ^ CommandResultValidation.None
                |> executeBufferedAsync Encoding.UTF8

            let exitCode = executionResult.ExitCode
            if exitCode = 0 then
                return ValueSome ^ stdout.ToString()
            else
                log.LogError(
                    "runTextProcess: wrong exit code: {exitCode};; stderr: {stdErr}",
                    exitCode, stderr.ToString()
                )
                return ValueNone
        }

    let runStreamProcess procName (args: string seq, escape) outputFileName =
        let procName, args = platformed procName args
        task {
            log.LogDebug(
                "runStreamProcess: proccess name: {procName};; args: {args};; escape: {escape};; outputFileName: {outputFileName}",
                procName, args, escape, outputFileName
            )

            let stderr = StringBuilder()
            let! executionResult =
                procName
                |> wrap
                |> withArguments args (escape |> ValueSome)
                |> withValidation CommandResultValidation.None
                |> withStandardErrorPipe (stderr |> PipeTarget.ToStringBuilder)
                |> executeBufferedAsync Console.OutputEncoding

            let exitCode = executionResult.ExitCode
            if exitCode = 0 then
                let! fileBytes = File.ReadAllBytesAsync(outputFileName)
                let ms = new MemoryStream(fileBytes)
                ms.Position <- 0
                let! res = File.ReadAllBytesAsync(outputFileName)
                return ValueSome res
            else
                log.LogError(
                    "runStreamProcess: wrong exit code: {exitCode};; stderr: {stdErr}",
                    exitCode, stderr.ToString()
                )
                return ValueNone
        }

    let runPipedStreamProcess procName inputStream (args: string seq, escape) =
        (inputStream :> Stream).Position <- 0
        let procName, args = platformed procName args
        task {
            log.LogDebug(
                "runStreamProcess: proccess name: {procName};; args: {args};; escape: {escape}",
                procName, args, escape
            )

            let stderr = StringBuilder()
            let stdout = new MemoryStream()
            let! executionResult =
                procName
                |> wrap
                |> withArguments args (escape |> ValueSome)
                |> withValidation CommandResultValidation.None
                |> withStandardInputPipe (inputStream |> PipeSource.FromStream)
                |> withStandardErrorPipe (stderr |> PipeTarget.ToStringBuilder)
                |> withStandardOutputPipe (PipeTarget.ToStream(stdout, ValueNone))
                |> executeBufferedAsync Console.OutputEncoding

            let exitCode = executionResult.ExitCode
            if exitCode = 0 then
                stdout.Position <- 0
                return stdout.ToArray() |> ValueSome
            else
                log.LogError(
                    "runStreamProcess: wrong exit code: {exitCode};; stderr: {stdErr}",
                    exitCode, stderr.ToString()
                )
                return ValueNone
        }
