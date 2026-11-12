# Construit depuis AmetisAPI/AmetisAPI (contexte défini dans docker-compose.yml).
# Image SDK alignée sur global.json (10.0.201) : une autre bande de SDK serait refusée par global.json.
FROM mcr.microsoft.com/dotnet/sdk:10.0.201 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/AmetisAPI.API/AmetisAPI.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /app/logs /app/keys && chown app:app /app/logs /app/keys
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "AmetisAPI.API.dll"]
