FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DataSinks/DataSinks.Api.Football/DataSinks.Api.Football.csproj", "DataSinks/DataSinks.Api.Football/"]
RUN dotnet restore "DataSinks/DataSinks.Api.Football/DataSinks.Api.Football.csproj"
COPY . .
WORKDIR "/src/DataSinks/DataSinks.Api.Football"
RUN dotnet build "DataSinks.Api.Football.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataSinks.Api.Football.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataSinks.Api.Football.dll"]
