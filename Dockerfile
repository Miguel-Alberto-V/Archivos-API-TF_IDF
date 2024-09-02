# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

# Crea un nuevo proyecto web API
RUN dotnet new webapi -o MyMicroservice --no-https

# Copiar la API creada
COPY Program.cs MyMicroservice/Program.cs

WORKDIR /app/MyMicroservice

# Restaura, compila y publica
RUN dotnet restore
RUN dotnet publish -c release -o /app/publish

# Etapa 2: Imagen de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "MyMicroservice.dll"]
