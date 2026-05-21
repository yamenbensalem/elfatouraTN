# ================================================================
# Multi-stage build — Web T4C GestCom (Blazor Server .NET 8)
# ================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (layer cached separately for faster rebuilds)
COPY Web_T4C_GestCom.csproj .
RUN dotnet restore

# Publish
COPY . .
RUN dotnet publish Web_T4C_GestCom.csproj -c Release -o /app/publish \
    --no-restore

# ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Kestrel listens on 8080 (non-root port, compatible with nginx-proxy VIRTUAL_PORT)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web_T4C_GestCom.dll"]
