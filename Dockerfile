# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
    
# Copy csproj and restore as distinct layers
COPY *.csproj ./
COPY *.cs ./
COPY items.json ./
COPY Controllers ./
COPY DiscordModules ./
COPY Services ./
COPY Models ./
COPY Util ./
COPY Views ./
COPY wwwroot ./
RUN dotnet restore
    
RUN ls /app
# Copy everything else and build
RUN dotnet publish -c Release -o out
RUN ls out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY Views ./Views/
COPY wwwroot ./wwwroot/
ENTRYPOINT ["dotnet", "RomDiscord.dll"]
#ENTRYPOINT "/bin/bash"

EXPOSE 1992/tcp
ENV ASPNETCORE_URLS http://+:1992