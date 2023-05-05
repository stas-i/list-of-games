FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DataWarehouse/DataWarehouse.Read.Api/DataWarehouse.Read.Api.csproj", "DataWarehouse/DataWarehouse.Read.Api/"]
RUN dotnet restore "DataWarehouse/DataWarehouse.Read.Api/DataWarehouse.Read.Api.csproj"
COPY . .
WORKDIR "/src/DataWarehouse/DataWarehouse.Read.Api"
RUN dotnet build "DataWarehouse.Read.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataWarehouse.Read.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataWarehouse.Read.Api.dll"]
