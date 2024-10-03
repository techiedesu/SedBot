[<RequireQualifiedAccess>]
module SedBot.Common.Process

open System.Text

open SedBot.Common.TypeExtensions
open Tdesu.CliWrap.Fsharp
open SedBot.Common.Utilities

open Microsoft.Extensions.Logging

type internal Marker = interface end
let private log = Logger.get ^ typeof<Marker>.DeclaringType.Name

let rec runTextProcessResult procName args data =
    task {
        log.LogDebug(
            $"{nameof runTextProcessResult}: process name: {{procName}};; args: {{args}};; data: {{data}}",
            procName,
            args,
            data
        )

        let stdout = StringBuilder()
        let stderr = StringBuilder()

        let! executionResult =
            procName
            |> wrap
            |> withEscapedArguments args
            |> withStandardInputPipe ^ PipeSource.FromString data
            |> withStandardErrorPipe ^ PipeTarget.ToStringBuilder stderr
            |> withStandardOutputPipe ^ PipeTarget.ToStringBuilder stdout
            |> withValidation CommandResultValidation.None
            |> executeBufferedAsync Encoding.UTF8

        let exitCode = executionResult.ExitCode

        if exitCode = 0 then
            return string stdout |> Ok
        else
            log.LogError(
                $"{nameof runTextProcessResult}: wrong exit code: {{exitCode}}, stderr: {{stdErr}}",
                exitCode,
                stderr
            )

            return string stderr |> Error
    }

let runTextProcess procName args data =
    let res = runTextProcessResult procName args data
    TaskOption.ofResult res

let getStatusCode procName args data =
    let executionResult =
        procName
        |> wrap
        |> withEscapedArguments args
        |> withStandardInputPipe ^ PipeSource.FromString data
        |> withValidation CommandResultValidation.None
        |> executeBuffered

    executionResult.ExitCode
