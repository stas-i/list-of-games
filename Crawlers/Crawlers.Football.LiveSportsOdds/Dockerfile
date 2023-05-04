FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Crawlers/Crawlers.Football.LiveSportsOdds/Crawlers.Football.LiveSportsOdds.csproj", "Crawlers/Crawlers.Football.LiveSportsOdds/"]
RUN dotnet restore "Crawlers/Crawlers.Football.LiveSportsOdds/Crawlers.Football.LiveSportsOdds.csproj"
COPY . .
WORKDIR "/src/Crawlers/Crawlers.Football.LiveSportsOdds"
RUN dotnet build "Crawlers.Football.LiveSportsOdds.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawlers.Football.LiveSportsOdds.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crawlers.Football.LiveSportsOdds.dll"]
