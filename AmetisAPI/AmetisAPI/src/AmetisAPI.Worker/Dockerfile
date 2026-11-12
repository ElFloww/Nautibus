# Construit depuis AmetisAPI/AmetisAPI (contexte défini dans docker-compose.yml).
# Image SDK alignée sur global.json (10.0.201) : une autre bande de SDK serait refusée par global.json.
FROM mcr.microsoft.com/dotnet/sdk:10.0.201 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/AmetisAPI.Worker/AmetisAPI.Worker.csproj -c Release -o /app

# Image ASP.NET : l'infrastructure référence le framework ASP.NET Core (Identity, client HTTP).
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /app/logs && chown app:app /app/logs
USER app
ENTRYPOINT ["dotnet", "AmetisAPI.Worker.dll"]
