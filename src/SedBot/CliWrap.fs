module SedBot.CliWrap

open System.IO
open System.Threading
open System.Threading.Tasks

type CliCommand = CliWrap.Command
type CliPipeSource = CliWrap.PipeSource
type CliPipeTarget = CliWrap.PipeTarget

let wrap target =
    CliWrap.Cli.Wrap(target)

let withArguments (args: string seq) (escape: bool voption) (command: CliCommand) =
    command.WithArguments(args, escape |> ValueOption.defaultValue true)

let withArgumentsEscape (args: string seq) escape (command: CliCommand) =
    command.WithArguments(args, escape)

type PipeSource =
    | Null
    | Create of action: (Stream -> unit)
    | FromBytes of data: byte array
    | FromCommand of command: CliWrap.Command
    | FromFile of filePath: string
    | FromMemory of data: System.ReadOnlyMemory<byte>
    | FromStream of stream: Stream
    | FromString of str: string

let withStandardInputPipe (source: PipeSource) (command: CliCommand) =
    let pipeSource =
        match source with
        | Null ->
            CliPipeSource.Null
        | Create action ->
            CliPipeSource.Create(action)
        | FromBytes data ->
            CliPipeSource.FromBytes(data)
        | FromCommand command ->
            CliPipeSource.FromCommand(command)
        | FromFile filePath ->
            CliPipeSource.FromFile(filePath)
        | FromMemory data ->
            CliPipeSource.FromMemory(data)
        | FromStream stream ->
            CliPipeSource.FromStream(stream)
        | FromString str ->
            CliPipeSource.FromString(str)

    command.WithStandardInputPipe(pipeSource)

type PipeTarget =
    | Null
    | Create of pipeHandler: (Stream -> unit)
    | CreateAsync of pipeHandler: (Stream -> CancellationToken -> Task)
    | ToFile of path: string
    | ToStream of stream: Stream * flush: bool voption
    | ToStringBuilder of sb: System.Text.StringBuilder

let withStandardErrorPipe (target: PipeTarget) (command: CliCommand) =
    let target =
        match target with
        | Null ->
            CliPipeTarget.Null
        | Create pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | CreateAsync pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | ToFile path ->
            CliPipeTarget.ToFile(path)
        | ToStream(stream, ValueNone) ->
            CliPipeTarget.ToStream(stream)
        | ToStream(stream, ValueSome flush) ->
            CliPipeTarget.ToStream(stream, flush)
        | ToStringBuilder sb ->
            CliPipeTarget.ToStringBuilder(sb)

    command.WithStandardErrorPipe(target)

let withStandardOutputPipe (target: PipeTarget) (command: CliCommand) =
    let target =
        match target with
        | Null ->
            CliPipeTarget.Null
        | Create pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | CreateAsync pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | ToFile path ->
            CliPipeTarget.ToFile(path)
        | ToStream(stream, ValueNone) ->
            CliPipeTarget.ToStream(stream)
        | ToStream(stream, ValueSome flush) ->
            CliPipeTarget.ToStream(stream, flush)
        | ToStringBuilder sb ->
            CliPipeTarget.ToStringBuilder(sb)

    command.WithStandardOutputPipe(target)

open CliWrap.Buffered

type CliCommandResultValidation = CliWrap.CommandResultValidation

type CommandResultValidation =
    | ZeroExitCode
    | None

let withValidation (validation: CommandResultValidation) (command: CliCommand) =
    let validation =
        match validation with
        | ZeroExitCode ->
            CliCommandResultValidation.ZeroExitCode
        | None ->
            CliCommandResultValidation.None
    command.WithValidation(validation)

let executeBuffered (command: CliCommand) =
    command.ExecuteBufferedAsync().ConfigureAwait(false).GetAwaiter().GetResult()

let executeBufferedAsync (encoding: System.Text.Encoding) (command: CliCommand) =
    command.ExecuteBufferedAsync(encoding)
