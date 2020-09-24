FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

COPY . /build

RUN cd /build && \
    dotnet restore -r linux-musl-x64 && \
    dotnet publish -c Release -r linux-musl-x64 --self-contained true -o /build-out HdhrProxy.sln

FROM alpine:latest

RUN apk update && \
    apk add icu libintl libstdc++ && \
    rm -rf /var/cache/apk/*

COPY --from=build /build-out /app

WORKDIR "/app"
ENTRYPOINT ["/app/HdhrProxy.Cli"]
