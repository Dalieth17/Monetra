# Imagen multietapa para la API .NET 8 de Monetra.
# Build context esperado: la carpeta backend/ (contiene Monetra.sln).

# --- Etapa 1: compilación y publicación ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurar primero solo con los .csproj aprovecha la caché de capas de Docker.
COPY Monetra.sln ./
COPY src/Monetra.Api/Monetra.Api.csproj src/Monetra.Api/
COPY src/Monetra.Application/Monetra.Application.csproj src/Monetra.Application/
COPY src/Monetra.Domain/Monetra.Domain.csproj src/Monetra.Domain/
COPY src/Monetra.Infrastructure/Monetra.Infrastructure.csproj src/Monetra.Infrastructure/
COPY tests/Monetra.UnitTests/Monetra.UnitTests.csproj tests/Monetra.UnitTests/
COPY tests/Monetra.IntegrationTests/Monetra.IntegrationTests.csproj tests/Monetra.IntegrationTests/
RUN dotnet restore src/Monetra.Api/Monetra.Api.csproj

COPY . .
RUN dotnet publish src/Monetra.Api/Monetra.Api.csproj -c Release -o /app/publish --no-restore

# --- Etapa 2: ejecución (imagen ligera, solo runtime) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5127
EXPOSE 5127

ENTRYPOINT ["dotnet", "Monetra.Api.dll"]
