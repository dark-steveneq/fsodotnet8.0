FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

RUN mkdir -p /App /out /out/simNFS
COPY . /App
RUN rm -rf /App/.git

WORKDIR /App/TSOClient/FSO.Server

RUN dotnet restore FSO.Server.csproj
RUN dotnet publish FSO.Server.csproj -c Release -o ./out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /App

COPY --from=builder /App/TSOClient/FSO.Server/out /App
COPY ./docker/config.sample.json /App/config.sample.json
COPY ./docker/entrypoint.sh /entrypoint.sh
RUN rm /App/version.txt

RUN chmod +x /entrypoint.sh

VOLUME ["/App/config.json", "/App/version.txt", "/App/Content", "/game", "/simNFS"]
EXPOSE 9000/tcp 33101/tcp 34101/tcp 35101/tcp
ENTRYPOINT [ "/entrypoint.sh" ]