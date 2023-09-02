FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS restore
WORKDIR /build

COPY ["src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj", "src/BUTR.Site.NexusMods.Server/"]
COPY ["src/BUTR.Site.NexusMods.Shared/BUTR.Site.NexusMods.Shared.csproj", "src/BUTR.Site.NexusMods.Shared/"]

RUN dotnet restore "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" -r linux-x64;
#RUN dotnet restore "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" -r linux-x64 -p:PublishReadyToRun=true;

COPY ["src/BUTR.Site.NexusMods.Server/", "src/BUTR.Site.NexusMods.Server/"]
COPY ["src/BUTR.Site.NexusMods.Shared/", "src/BUTR.Site.NexusMods.Shared/"]

COPY [".git/HEAD", ".git/HEAD"]
COPY [".git/config", ".git/config"]
COPY [".git/refs/heads/", ".git/refs/heads/"]


FROM restore AS publish
WORKDIR /build
RUN dotnet publish "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" --no-restore -c Release -r linux-musl-x64 -o /app/publish;
#RUN dotnet publish "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" --no-restore -c Release -r linux-musl-x64 -o /app/publish -p:PublishReadyToRun=true;


FROM base AS final

RUN apt-get update && apt-get install -y wget unzip \
    && wget https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_2.5.0/depotdownloader-2.5.0.zip \
    && unzip depotdownloader-2.5.0.zip -d /depotdownloader \
    && rm depotdownloader-2.5.0.zip \
    && apt-get remove -y wget unzip \
    && apt-get autoremove -y \
    && apt-get clean -y

WORKDIR /app
COPY --from=publish /app/publish .

LABEL org.opencontainers.image.source="https://github.com/BUTR/BUTR.Site.NexusMods"
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "BUTR.Site.NexusMods.Server.dll"]