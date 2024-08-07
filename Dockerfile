# syntax=docker/dockerfile:1.2

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY ./src/ ./app
WORKDIR /app/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages\
    dotnet publish SedBot/SedBot.fsproj -c Release -o output --sc --os linux

FROM debian:bullseye as host
RUN echo "deb http://www.deb-multimedia.org bullseye main" >> /etc/apt/sources.list.d/multimedia.list
RUN apt update -oAcquire::AllowInsecureRepositories=true && apt install -y deb-multimedia-keyring --allow-unauthenticated
RUN apt update && apt install -y ca-certificates ffmpeg imagemagick-7 jq
COPY --from=build /app/output .
ENTRYPOINT ./SedBot $BOT_TOKEN
