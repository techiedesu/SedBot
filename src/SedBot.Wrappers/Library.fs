namespace SedBot.Wrappers

open System.IO
open System.Text
open CliWrap
open CliWrap.Buffered
open NUnit.Framework

module FFmpeg =
    let private wslExec (args: string array, escape) = task {
        let stderr = StringBuilder()
        let! executionResult =
            Cli
                .Wrap("wsl")
                .WithArguments(args |> Array.append [| "ffmpeg" |], escape)
                .WithValidation(CommandResultValidation.None)
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
                .ExecuteBufferedAsync()
        ()
    }

    let private wslStreamsExec (args: string array, escape) (stdIn: Stream) = task {
        let stderr = StringBuilder()
        let! executionResult =
            Cli
                .Wrap("wsl")
                .WithArguments(args |> Array.append [| "ffmpeg" |], escape)
                .WithValidation(CommandResultValidation.None)
                .WithStandardInputPipe(PipeSource.FromStream(stdIn))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
                .ExecuteBufferedAsync()
        ()
    }

    let private linuxExec () =
        let array = Array.empty
        array[0] <- 1
        ()

module ``Playground tests`` =
    let [<Test>] [<Platform("Win")>] ``Validate WSL``() =
        let executionResult =
            Cli
                .Wrap("wsl")
                .WithArguments(["uname"; "-o"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync().ConfigureAwait(false).GetAwaiter().GetResult()
        Assert.AreEqual(0, executionResult.ExitCode)
        Assert.AreEqual("GNU/Linux", executionResult.StandardOutput.Trim())

    open SedBot.Wrappers.Cli
    let [<Test>] [<Platform("Win")>] ``Validate WSL wrapped``() =
        let executionResult =
            "wsl"
            |> wrap
            |> withArguments ["uname"; "-o"]
            |> withValidation CommandResultValidation.None
            |> executeBuffered
        Assert.AreEqual(0, executionResult.ExitCode)
        Assert.AreEqual("GNU/Linux", executionResult.StandardOutput.Trim())
