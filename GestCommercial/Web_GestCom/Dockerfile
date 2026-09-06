# syntax=docker/dockerfile:1
# ================================================================
# Multi-stage build — Web GestCom (Blazor Server .NET 8)
#
# Web_GestCom.csproj has a ProjectReference to the sibling
# ../Web_GestCom.Core project, so it's supplied here as a separate
# named build context ("core") rather than widening the main context —
# see deploy/prod/deploy_to_vps.ps1's `--build-context core=...` flag.
# ================================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (layer cached separately for faster rebuilds)
COPY Web_GestCom.csproj .
COPY --from=core . ../Web_GestCom.Core/
RUN dotnet restore

# Publish
COPY . .
RUN dotnet publish Web_GestCom.csproj -c Release -o /app/publish \
    --no-restore

# ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Kestrel listens on 8080 (non-root port, compatible with nginx-proxy VIRTUAL_PORT)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web_GestCom.dll"]
