FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["BUTR.CrashReportViewer/Server/BUTR.CrashReportViewer.Server.csproj", "BUTR.CrashReportViewer/Server/"]
COPY ["BUTR.CrashReportViewer/Shared/BUTR.CrashReportViewer.Shared.csproj", "BUTR.CrashReportViewer/Shared/"]

RUN dotnet restore "BUTR.CrashReportViewer/Server/BUTR.CrashReportViewer.Server.csproj"

COPY . .

FROM build AS publish
RUN dotnet publish "BUTR.CrashReportViewer.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BUTR.CrashReportViewer.Server.dll"]