﻿FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DataWarehouse/DataWarehouse.DbMigrator/DataWarehouse.DbMigrator.csproj", "DataWarehouse/DataWarehouse.DbMigrator/"]
RUN dotnet restore "DataWarehouse/DataWarehouse.DbMigrator/DataWarehouse.DbMigrator.csproj"
COPY . .
WORKDIR "/src/DataWarehouse/DataWarehouse.DbMigrator"
RUN dotnet build "DataWarehouse.DbMigrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataWarehouse.DbMigrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "DataWarehouse.DbMigrator.dll"]
CMD ["sh", "-c", "dotnet DataWarehouse.DbMigrator.dll \"${DB_CONNECTION}\""]
