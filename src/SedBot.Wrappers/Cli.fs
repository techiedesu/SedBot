module SedBot.Wrappers.Cli

open System.IO

type CliCommand = CliWrap.Command
type CliPipeSource = CliWrap.PipeSource

let wrap target =
    CliWrap.Cli.Wrap(target)

let withArguments (args: string seq) (command: CliCommand) =
    command.WithArguments(args)

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

let executeBufferedAsync (command: CliCommand) =
    command.ExecuteBufferedAsync()
