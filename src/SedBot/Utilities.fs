module SedBot.Utilities

open System
open System.IO
open System.Text
open CliWrap
open CliWrap.Buffered
open Microsoft.Extensions.Logging
open Serilog
open Serilog.Extensions.Logging

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

module String =
    let private logger = Logger.get "SedBot.Utilities.String"

    let removeFromStart (input: string) (text: string) =
        if input = null || text = null then
            logger.LogError("removeFromStart: unextected null. input: {input} ;; text: {text}", input, text)
            text
        else
            text.Trim().Substring(input.Length)

module Path =
    let getSynthName extension =
        Guid.NewGuid().ToString().Replace("-", "") + extension

module Process =
    let private log = Logger.get "SedBot.Utilities.Process"

    let runTextProcess procName (args: string seq) data =
        task {
            log.LogDebug(
                "runTextProcess: proccess name: {procName};; args: {args};; data: {data}",
                procName, args, data
            )

            let stdout = StringBuilder()
            let stderr = StringBuilder()
            let! executionResult =
                Cli
                    .Wrap(procName)
                    .WithArguments(args)
                    .WithStandardInputPipe(PipeSource.FromString(data))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync(Encoding.UTF8)

            let exitCode = executionResult.ExitCode
            if exitCode = 0 then
                return Some ^ stdout.ToString()
            else
                log.LogError(
                    "runTextProcess: wrong exit code: {exitCode};; stderr: {stdErr}",
                    exitCode, stderr.ToString()
                )
                return None
        }

    let runStreamProcess procName (args: string seq, escape) outputFileName =
        task {
            log.LogDebug(
                "runStreamProcess: proccess name: {procName};; args: {args};; escape: {escape};; outputFileName: {outputFileName}",
                procName, args, escape, outputFileName
            )

            let stderr = StringBuilder()
            let! executionResult =
                Cli
                    .Wrap(procName)
                    .WithArguments(args, escape)
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
                    .ExecuteBufferedAsync()

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
