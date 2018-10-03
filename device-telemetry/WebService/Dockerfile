FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 9004

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["device-telemetry/telemetry.sln", "device-telemetry/"]
COPY ["device-telemetry/Services/Services.csproj", "device-telemetry/Services/"]
COPY ["device-telemetry/WebService/WebService.csproj", "device-telemetry/WebService/"]
COPY ["device-telemetry/WebService/azds_entrypoint.sh", "device-telemetry/WebService/azds_entrypoint.sh"]

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR "/src/device-telemetry/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT  ["sh", "/src/device-telemetry/WebService/azds_entrypoint.sh"]