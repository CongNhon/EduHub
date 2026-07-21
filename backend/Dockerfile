FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore EduHub.sln
RUN dotnet publish src/EduHub.WebApi/EduHub.WebApi.csproj --configuration Release --output /app/publish --no-restore
RUN dotnet publish tools/EduHub.DatabaseManager/EduHub.DatabaseManager.csproj --configuration Release --output /app/database-manager --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0
EXPOSE 8080

COPY --from=build --chown=app:app /app/publish .
COPY --from=build --chown=app:app /app/database-manager ./database-manager
RUN mkdir -p /app/data/profile-evidence /app/data/reports \
    && chown -R app:app /app

USER $APP_UID
ENTRYPOINT ["dotnet", "EduHub.WebApi.dll"]
