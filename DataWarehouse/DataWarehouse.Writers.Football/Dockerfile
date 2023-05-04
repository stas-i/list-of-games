FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DataWarehouse/DataWarehouse.Writers.Football/DataWarehouse.Writers.Football.csproj", "DataWarehouse/DataWarehouse.Writers.Football/"]
RUN dotnet restore "DataWarehouse/DataWarehouse.Writers.Football/DataWarehouse.Writers.Football.csproj"
COPY . .
WORKDIR "/src/DataWarehouse/DataWarehouse.Writers.Football"
RUN dotnet build "DataWarehouse.Writers.Football.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataWarehouse.Writers.Football.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataWarehouse.Writers.Football.dll"]
