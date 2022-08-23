module SedBot.Utilities

open System
open System.IO
open System.Text
open CliWrap
open CliWrap.Buffered

module String =
    let removeFromStart (input: string) (text: string) =
        text.Trim().Substring(input.Length)

module Path =
    let getSynthName extension =
        Guid.NewGuid().ToString().Replace("-", "") + extension

let runTextProcess procName (args: string seq) data =
    task {
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

        if executionResult.ExitCode = 0 then
            return stdout.ToString() |> Some
        else
            return None
    }

let runStreamProcess procName (args: string seq, escape) outputFileName =
    task {
        let! executionResult =
            Cli
                .Wrap(procName)
                .WithArguments(args, escape)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()

        if executionResult.ExitCode = 0 then
            let! fileBytes = File.ReadAllBytesAsync(outputFileName)
            let ms = new MemoryStream(fileBytes)
            ms.Position <- 0
            let! res = File.ReadAllBytesAsync(outputFileName)
            return ValueSome res
        else
            return ValueNone
    }