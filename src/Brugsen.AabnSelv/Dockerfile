# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS base
RUN apk add --no-cache tzdata
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Brugsen.AabnSelv/Brugsen.AabnSelv.csproj", "src/Brugsen.AabnSelv/"]
RUN dotnet restore "./src/Brugsen.AabnSelv/Brugsen.AabnSelv.csproj"
COPY . .
WORKDIR "/src/src/Brugsen.AabnSelv"
RUN dotnet build "./Brugsen.AabnSelv.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN apk add --no-cache clang
RUN dotnet publish "./Brugsen.AabnSelv.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# This stage is used to build the frontend
FROM node:24-alpine AS vite
WORKDIR /src
COPY ["src/Brugsen.AabnSelv/package*.json", "./"]
RUN npm install --no-audit
WORKDIR /src
COPY ["src/Brugsen.AabnSelv/src/", "./src/"]
COPY ["src/Brugsen.AabnSelv/typings/", "./typings/"]
COPY ["src/Brugsen.AabnSelv/*.html", "./"]
COPY ["src/Brugsen.AabnSelv/tsconfig*.json", "./"]
COPY ["src/Brugsen.AabnSelv/vite.config.js", "./"]
RUN npm run build

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=vite /src/wwwroot/. wwwroot
ENTRYPOINT ["/app/Brugsen.AabnSelv"]