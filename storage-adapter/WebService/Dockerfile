FROM microsoft/aspnetcore-build:2.0-jessie AS Base
WORKDIR /app
EXPOSE 9022

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["storage-adapter/storage-adapter.sln", "storage-adapter/"]
COPY ["storage-adapter/Services/Services.csproj", "storage-adapter/Services/"]
COPY ["storage-adapter/WebService/WebService.csproj", "storage-adapter/WebService/"]
COPY ["storage-adapter/WebService/azds_entrypoint.sh", "storage-adapter/WebService/azds_entrypoint.sh"]

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR "/src/storage-adapter/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT  ["sh", "/src/storage-adapter/WebService/azds_entrypoint.sh"]