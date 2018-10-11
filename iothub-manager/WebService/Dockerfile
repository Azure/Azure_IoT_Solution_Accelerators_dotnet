FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 9002

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["iothub-manager/iothub-manager.sln", "iothub-manager/"]
COPY ["iothub-manager/Services/Services.csproj", "iothub-manager/Services/"]
COPY ["iothub-manager/WebService/WebService.csproj", "iothub-manager/WebService/"]
COPY ["iothub-manager/WebService/azds_entrypoint.sh", "iothub-manager/WebService/azds_entrypoint.sh"]

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR "/src/iothub-manager/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT  ["sh", "/src/iothub-manager/WebService/azds_entrypoint.sh"]