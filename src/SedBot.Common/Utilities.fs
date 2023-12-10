module SedBot.Common.Utilities

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
