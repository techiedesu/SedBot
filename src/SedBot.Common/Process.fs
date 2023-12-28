[<RequireQualifiedAccess>]
module SedBot.Common.Process

open System.Text

open SedBot.Common.TypeExtensions
open Tdesu.CliWrap.Fsharp
open SedBot.Common.Utilities

open Microsoft.Extensions.Logging

type internal Marker = interface end
let private log = Logger.get ^ typeof<Marker>.DeclaringType.Name

let rec runTextProcess procName args data = task {
    log.LogDebug(
        $"{nameof runTextProcess}: process name: {{procName}};; args: {{args}};; data: {{data}}",
        procName, args, data
    )

    let stdout = StringBuilder()
    let stderr = StringBuilder()

    let! executionResult =
        procName
        |> wrap
        |> withEscapedArguments args
        |> withStandardInputPipe  ^ PipeSource.FromString data
        |> withStandardErrorPipe  ^ PipeTarget.ToStringBuilder stderr
        |> withStandardOutputPipe ^ PipeTarget.ToStringBuilder stdout
        |> withValidation CommandResultValidation.None
        |> executeBufferedAsync Encoding.UTF8

    let exitCode = executionResult.ExitCode
    if exitCode = 0 then
        return string stdout |> ValueSome
    else
        log.LogError($"{nameof runTextProcess}: wrong exit code: {{exitCode}}, stderr: {{stdErr}}", exitCode, stderr)
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


