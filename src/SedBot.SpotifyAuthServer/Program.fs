module SedBot.SpotifyAuthServer.Program

open System
open System.IO
open System.Security.Claims
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

// ---------------------------------
// Web app
// ---------------------------------

// TODO: copy from https://github.com/JohnnyCrazy/SpotifyAPI-NET/blob/master/SpotifyAPI.Web.Examples/Example.ASP/Startup.cs
// https://johnnycrazy.github.io/SpotifyAPI-NET/docs/getting_started/
// TODO: add nlog & Loki + Promtail

type [<CLIMutable>] AppConfig = {
    Spotify: SpotifyConfig
}
and [<CLIMutable>] SpotifyConfig = {
    ClientId: string
    ClientSecret: string
    CallbackPath: string
    SaveTokens: bool
}

type SimpleClaim = {
    Type: string
    Value: string
}

let greet =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let claim = ctx.User.FindFirst "name"
        let name = claim.Value
        text ("Hello " + name) next ctx

let showClaims =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let claims = ctx.User.Claims
        let simpleClaims = Seq.map (fun (i : Claim) -> {Type = i.Type; Value = i.Value}) claims
        json simpleClaims next ctx

let webApp =
    choose [
        GET >=> choose [
            route "/" >=> text "Public endpoint."
        ]
        setStatusCode 404 >=> text "Not Found"
    ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> setStatusCode 500
    >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app.UseAuthentication()
       .UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseGiraffe webApp

let configureServices (configuration: IConfiguration) (services: IServiceCollection) =
    let spotifyConfig = configuration.GetRequiredSection(nameof(SpotifyConfig)).Get<SpotifyConfig>()

    services
        .AddHttpContextAccessor()
        // services.AddSingleton(SpotifyClientConfig.CreateDefault());
        // services.AddScoped<SpotifyClientBuilder>();

        .AddAuthorization(fun c ->
            c.AddPolicy("spotify",
                        fun policy ->
                            policy.AuthenticationSchemes.Add("Spotify")
                            policy.RequireAuthenticatedUser() |> ignore
            )
        )
        .AddAuthentication(fun c ->
            c.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
        )
        .AddCookie(fun c ->
            c.ExpireTimeSpan <- TimeSpan.FromMinutes(50)
        )
        .AddSpotify(fun c ->
            c.ClientId <- spotifyConfig.ClientId
            c.ClientSecret <- spotifyConfig.ClientSecret
            c.CallbackPath <- spotifyConfig.CallbackPath
            c.SaveTokens <- spotifyConfig.SaveTokens
        )
    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    let filter (l: LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")

    let configuration =
        ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .AddJsonFile("appsettings.json")
            .Build()

    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseKestrel()
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(configureApp)
                    .ConfigureServices(configureServices configuration)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0
