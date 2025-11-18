# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY UnitySimBackend.csproj ./
RUN dotnet restore UnitySimBackend.csproj

# Copy the rest of the source code
COPY . ./

# Publish
RUN dotnet publish UnitySimBackend.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://+:5000;http://0.0.0.0:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "UnitySimBackend.dll"]
