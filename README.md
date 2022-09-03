# Sed bot

Host requirements:

* sed
* ffmpeg
* jq
* imagemagick
* dotnet 6

## Docker

Build and run:

```bash
docker build -t sed_bot .
docker run --rm -it -e BOT_TOKEN='tgtoken' sed_bot
```
