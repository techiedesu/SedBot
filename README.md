# Sed bot

## License

See [COPYING](COPYING)

## How to use

Sample bot [@nikita_mnogo_deneg_bot](https://t.me/nikita_mnogo_deneg_bot)

### Commands

### Usage

There are two ways of bot usage:

1. Via prefix 't!'. For example, 't!dist'
2. Default telegram usage '/dist@nikita_mnogo_deneg_bot' of '/dist' for private chats

#### Caveats

Sed command could be parsed only with access to all messages in public groups. Visit [privacy mode](https://core.telegram.org/bots/features#privacy-mode) for more information.

#### Available commands

| Command             | Description               | GIF   | Pictures | Videos | Voices | Music | Text |
|---------------------|---------------------------|-------|----------|--------|--------|-------|------|
| `t!rev`             | reverse                   | +     | -        | +      | +      | +     | -    |
| `t!dist`            | distortion                | +     | +        | +      | +      | +     | -    |
| `t!clock`           | clockwise rotation        | +     | +        | +      | -      | -     | -    |
| `t!cclock`          | counterclockwise rotation | +     | +        | +      | -      | -     | -    |
| `t!vflip`           | vertical flip             | +     | +        | +      | -      | -     | -    |
| `t!hflip`           | horizontal flip           | +     | +        | +      | -      | -     | -    |
| `t!hflip`           | horizontal flip           | +     | +        | +      | -      | -     | -    |
| `t!jq <expression>` | apply jq expression        | -     | -        | -      | -      | -     | +    |

sed command will be executed only with "s/" or "s@" prefixes

## How to build

### Docker

Requirements:

* Docker

Build and run:

```bash
docker build -t sed_bot .
docker run --rm -it -e BOT_TOKEN='tgtoken' sed_bot
```

### Host

Requirements:

* sed
* ffmpeg
* jq
* imagemagick
* dotnet 8

```bash
cd src/
dotnet tool restore
dotnet paket install
```

then, open `SedBot.sln` with rider or something else.
