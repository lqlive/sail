# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["NuGet.config", "."]
COPY ["src/Sail/Sail.csproj", "src/Sail/"]
COPY ["src/Shared/ServiceDefaults/ServiceDefaults.csproj", "src/Shared/ServiceDefaults/"]
COPY ["src/Sail.Core/Sail.Core.csproj", "src/Sail.Core/"]
COPY ["src/Sail.Compass/Sail.Compass.csproj", "src/Sail.Compass/"]
COPY ["src/Sail.Database.MongoDB/Sail.Database.MongoDB.csproj", "src/Sail.Database.MongoDB/"]
RUN dotnet restore "./src/Sail/Sail.csproj"
COPY . .
WORKDIR "/src/src/Sail"
RUN dotnet build "./Sail.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Sail.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sail.dll"]