FROM --platform=$BUILDARCH mcr.microsoft.com/dotnet/sdk:8.0 AS builder

ARG TARGETARCH

RUN mkdir -p /App /out /out/simNFS
COPY . /App
COPY ./docker/entrypoint.sh /entrypoint.sh
RUN rm -rf /App/.git
RUN chmod +x /entrypoint.sh

WORKDIR /App/TSOClient/FSO.Server

RUN dotnet restore FSO.Server.csproj
RUN dotnet publish FSO.Server.csproj -a $TARGETARCH -c Release -o ./out

FROM --platform=$TARGETARCH mcr.microsoft.com/dotnet/aspnet:8.0

LABEL org.opencontainers.image.authors=https://github.com/dark-steveneq
LABEL org.opencontainers.image.url=https://github.com/dark-steveneq/fsodotnet8.0
LABEL org.opencontainers.image.documentation=https://github.com/dark-steveneq/fsodotnet8.0/blob/master/docker/README.md
LABEL org.opencontainers.image.source="https://github.com/dark-steveneq/fsodotnet8.0"
LABEL org.opencontainers.image.vendor="Dark Steveneq"
LABEL org.opencontainers.image.licenses=MPL-2.0
LABEL org.opencontainers.image.title="FreeSO .NET 8.0 Port Server"
LABEL org.opencontainers.image.description="Container with FreSO .NET 8.0 Server"

WORKDIR /App

COPY --from=builder /App/TSOClient/FSO.Server/out /App
COPY --from=builder /entrypoint.sh /entrypoint.sh
COPY ./docker/config.sample.json /App/config.sample.json

VOLUME ["/App/config.json", "/App/version.txt", "/App/Content", "/game", "/simNFS"]
EXPOSE 9000/tcp 33101/tcp 34101/tcp 35101/tcp
ENTRYPOINT [ "/entrypoint.sh" ]