FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview-alpine AS restore
WORKDIR /src

COPY ["BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj", "BUTR.Site.NexusMods.Server/"]
COPY ["BUTR.Site.NexusMods.Shared/BUTR.Site.NexusMods.Shared.csproj", "BUTR.Site.NexusMods.Shared/"]

RUN dotnet restore "BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj"

COPY . .


FROM restore AS publish
WORKDIR "/src/BUTR.Site.NexusMods.Server"
RUN dotnet publish "BUTR.Site.NexusMods.Server.csproj" -c Release -r linux-musl-x64 -p:PublishReadyToRun=true -o /app/publish


FROM base AS final

RUN apk update && apk add --no-cache wget unzip \
    && wget https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_2.4.7/depotdownloader-2.4.7.zip \
    && unzip depotdownloader-2.4.7.zip -d /depotdownloader \
    && rm depotdownloader-2.4.7.zip \
    && apk del wget unzip

WORKDIR /app
COPY --from=publish /app/publish .

LABEL org.opencontainers.image.source="https://github.com/BUTR/BUTR.Site.NexusMods"
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "BUTR.Site.NexusMods.Server.dll"]