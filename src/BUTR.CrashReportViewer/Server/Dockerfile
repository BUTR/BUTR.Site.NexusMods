FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["BUTR.CrashReportViewer/Server/BUTR.CrashReportViewer.Server.csproj", "BUTR.CrashReportViewer/Server/"]
COPY ["BUTR.CrashReportViewer/Shared/BUTR.CrashReportViewer.Shared.csproj", "BUTR.CrashReportViewer/Shared/"]

RUN dotnet restore "BUTR.CrashReportViewer/Server/BUTR.CrashReportViewer.Server.csproj"

COPY . .

WORKDIR "/src/BUTR.CrashReportViewer/Server"
RUN dotnet build "BUTR.CrashReportViewer.Server.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "BUTR.CrashReportViewer.Server.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

LABEL org.opencontainers.image.source="https://github.com/BUTR/BUTR.CrashReportViewer"
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "BUTR.CrashReportViewer.Server.dll"]
