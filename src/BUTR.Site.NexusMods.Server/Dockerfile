FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj", "BUTR.Site.NexusMods.Server/"]
COPY ["BUTR.Site.NexusMods.Shared/BUTR.Site.NexusMods.Shared.csproj", "BUTR.Site.NexusMods.Shared/"]

RUN dotnet restore "BUTR.Site.NexusMods.Server/BUTR.Site.NexusMods.Server.csproj"

COPY . .

WORKDIR "/src/BUTR.Site.NexusMods.Server"
RUN dotnet build "BUTR.Site.NexusMods.Server.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "BUTR.Site.NexusMods.Server.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

LABEL org.opencontainers.image.source="https://github.com/BUTR/BUTR.Site.NexusMods"
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "BUTR.Site.NexusMods.Server.dll"]