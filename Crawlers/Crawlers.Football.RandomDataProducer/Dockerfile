FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Crawlers/Crawlers.Football.BenchmarkProducer/Crawlers.Football.BenchmarkProducer.csproj", "Crawlers/Crawlers.Football.BenchmarkProducer/"]
RUN dotnet restore "Crawlers/Crawlers.Football.BenchmarkProducer/Crawlers.Football.BenchmarkProducer.csproj"
COPY . .
WORKDIR "/src/Crawlers/Crawlers.Football.BenchmarkProducer"
RUN dotnet build "Crawlers.Football.BenchmarkProducer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawlers.Football.BenchmarkProducer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crawlers.Football.BenchmarkProducer.dll"]
