namespace SedBot

type [<CLIMutable>] AppConfig = {
    Channels: Channel[]
}

and [<CLIMutable>] Channel = {
    Type: string
    Token: string
}
