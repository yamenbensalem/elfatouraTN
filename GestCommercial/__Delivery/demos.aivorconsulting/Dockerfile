# ─── Stage 1 : build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (couche cachée séparée)
COPY Web_T4C_GestCom/Web_T4C_GestCom.csproj Web_T4C_GestCom/
RUN dotnet restore Web_T4C_GestCom/Web_T4C_GestCom.csproj

# Copier le reste et publier
COPY Web_T4C_GestCom/ Web_T4C_GestCom/
WORKDIR /src/Web_T4C_GestCom
RUN dotnet publish Web_T4C_GestCom.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ─── Stage 2 : runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Utilisateur non-root
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build --chown=appuser /app/publish .

# Port interne Kestrel
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "Web_T4C_GestCom.dll"]
