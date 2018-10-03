FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 90021

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["auth/auth.sln", "auth/"]
COPY ["auth/Services/Services.csproj", "auth/Services/"]
COPY ["auth/WebService/WebService.csproj", "auth/WebService/"]
COPY ["auth/WebService/azds_entrypoint.sh", "auth/WebService/azds_entrypoint.sh"]

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR "/src/auth/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT  ["sh", "/src/auth/WebService/azds_entrypoint.sh"]