module SedBot.Utilities

open SedBot.CliWrap

open System
open System.Text
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

module Path =
    let getSynthName extension =
        Guid.NewGuid().ToString().Replace("-", "") + extension

module Process =
    let private log = Logger.get "SedBot.Utilities.Process"

    let runTextProcess procName args data =
        task {
            log.LogDebug(
                "runTextProcess: process name: {procName};; args: {args};; data: {data}",
                procName, args, data
            )

            let stdout = StringBuilder()
            let stderr = StringBuilder()

            let! executionResult =
                procName
                |> wrap
                |> withEscapedArguments args
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

    let getStatusCode procName args data =
        let executionResult =
            procName
            |> wrap
            |> withEscapedArguments args
            |> withStandardInputPipe ^ PipeSource.FromString data
            |> withValidation CommandResultValidation.None
            |> executeBuffered

        executionResult.ExitCode
