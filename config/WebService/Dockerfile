FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 9004

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["config/telemetry.sln", "config/"]
COPY ["config/Services/Services.csproj", "config/Services/"]
COPY ["config/WebService/WebService.csproj", "config/WebService/"]
COPY ["config/WebService/azds_entrypoint.sh", "config/WebService/azds_entrypoint.sh"]

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR "/src/config/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT  ["sh", "/src/config/WebService/azds_entrypoint.sh"]