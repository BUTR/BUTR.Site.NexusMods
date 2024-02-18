FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS restore
WORKDIR /build

COPY ["src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj", "src/BUTR.Site.NexusMods.Server/"]
COPY ["src/BUTR.Site.NexusMods.Shared/BUTR.Site.NexusMods.Shared.csproj", "src/BUTR.Site.NexusMods.Shared/"]
COPY ["src/BUTR.Site.NexusMods.Server.ValueObjects.Vogen/BUTR.Site.NexusMods.Server.ValueObjects.Vogen.csproj", "src/BUTR.Site.NexusMods.Server.ValueObjects.Vogen/"]
COPY ["src/BUTR.Site.NexusMods.Server.ValueObjects.TVO/BUTR.Site.NexusMods.Server.ValueObjects.TVO.csproj", "src/BUTR.Site.NexusMods.Server.ValueObjects.TVO/"]
COPY ["src/BUTR.Site.NexusMods.DependencyInjection/BUTR.Site.NexusMods.DependencyInjection.csproj", "src/BUTR.Site.NexusMods.DependencyInjection/"]
COPY ["src/nuget.config", "src/"]

RUN dotnet restore "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" -r linux-x64;
#RUN dotnet restore "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" -r linux-x64 -p:PublishReadyToRun=true;

COPY ["src/BUTR.Site.NexusMods.Server/", "src/BUTR.Site.NexusMods.Server/"]
COPY ["src/BUTR.Site.NexusMods.Shared/", "src/BUTR.Site.NexusMods.Shared/"]
COPY ["src/BUTR.Site.NexusMods.Server.ValueObjects.Vogen/", "src/BUTR.Site.NexusMods.Server.ValueObjects.Vogen/"]
COPY ["src/BUTR.Site.NexusMods.Server.ValueObjects.TVO/", "src/BUTR.Site.NexusMods.Server.ValueObjects.TVO/"]
COPY ["src/BUTR.Site.NexusMods.DependencyInjection/", "src/BUTR.Site.NexusMods.DependencyInjection/"]

COPY [".git/HEAD", ".git/HEAD"]
COPY [".git/config", ".git/config"]
COPY [".git/refs/heads/", ".git/refs/heads/"]


FROM restore AS publish
WORKDIR /build
RUN dotnet publish "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" --no-restore -c Release -r linux-x64 -o /app/publish;
#RUN dotnet publish "src/BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj" --no-restore -c Release -r linux-x64 -o /app/publish -p:PublishReadyToRun=true;


FROM base AS final

RUN apt-get update \
    && apt-get install -y wget \
    && wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y libmsquic unzip \
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