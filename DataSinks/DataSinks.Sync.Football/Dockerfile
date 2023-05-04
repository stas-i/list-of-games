FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DataSinks/DataSinks.Sync.Football/DataSinks.Sync.Football.csproj", "DataSinks/DataSinks.Sync.Football/"]
RUN dotnet restore "DataSinks/DataSinks.Sync.Football/DataSinks.Sync.Football.csproj"
COPY . .
WORKDIR "/src/DataSinks/DataSinks.Sync.Football"
RUN dotnet build "DataSinks.Sync.Football.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataSinks.Sync.Football.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataSinks.Sync.Football.dll"]
