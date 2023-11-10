module SedBot.SpotifyAuthServer.Program

open System
open System.IO
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open SpotifyAPI.Web
open Microsoft.AspNetCore.Authentication
open OpenTelemetry.Trace

type [<CLIMutable>] AppConfig = {
    Spotify: SpotifyConfig
    S3: S3Storage
}
and [<CLIMutable>] SpotifyConfig = {
    ClientId: string
    ClientSecret: string
    CallbackPath: string
    SaveTokens: bool
}
and [<CLIMutable>] S3Storage ={
    UseHttps: bool
    Host: string
}

type [<Sealed>] SpotifyClientBuilder(httpContextAccessor: IHttpContextAccessor,
                                     config: SpotifyClientConfig) =
    member this.BuildClient() = task {
        let hc = httpContextAccessor.HttpContext
        let! token = hc.GetTokenAsync("Spotify", "access_token")
        return SpotifyClient(config.WithToken(token))
    }

let spotifyHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) -> task {
        let spotifyClientBuilder = ctx.RequestServices.GetService<SpotifyClientBuilder>()
        let! client = spotifyClientBuilder.BuildClient()
        let! privateUser = client.UserProfile.Current()

        return! Successful.OK privateUser.Id next ctx
    }

let webApp =
    choose [
        requiresAuthentication (challenge "Spotify" >=> text "please authenticate") >=>
            GET >=>
                choose [
                    route "/" >=> spotifyHandler
                ]
        setStatusCode 404 >=> text "Not Found"
    ]

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(EventId(), ex,
                    "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> setStatusCode 500
    >=> text ex.Message

let configureApp (app: IApplicationBuilder) =
    app
       .UseAuthentication()
       .UseHttpsRedirection()
       .UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseGiraffe(webApp)


let configureServices (configuration: IConfiguration)
    (services: IServiceCollection) =
    let spotifyConfig =
        configuration
            .GetRequiredSection(nameof(SpotifyConfig))
            .Get<SpotifyConfig>()

    services
        .AddOpenTelemetry()
        .WithTracing(fun c ->
            c
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter()
            |> ignore
        ) |> ignore

    services
        .AddHttpContextAccessor() |> ignore
    services
        .AddSingleton(SpotifyClientConfig.CreateDefault())
        .AddScoped<SpotifyClientBuilder>()
        |> ignore

    services
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

            let scopes =
                [|
                    Scopes.UserTopRead
                    Scopes.UserReadEmail
                    Scopes.UserReadPrivate
                    Scopes.PlaylistReadPrivate
                    Scopes.UserLibraryRead
                    Scopes.PlaylistReadCollaborative
                |] |> String.concat ","
            c.Scope.Add(scopes)
        )
    |> ignore
    services.AddGiraffe() |> ignore

open NLog.Web

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    let environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") // TODO: parse from config?

    let configuration =
        ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional = true)
            .Build()

    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseKestrel()
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .ConfigureLogging(fun c -> c.ClearProviders() |> ignore)
                    .Configure(configureApp)
                    .ConfigureServices(configureServices configuration)
                    |> ignore)
        .UseNLog()
        .Build()
        .Run()
    0
