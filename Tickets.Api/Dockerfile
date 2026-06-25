#stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

#copy project file and restore dependencies
COPY Tickets.Api.csproj .
RUN dotnet restore

#copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

#stage 2: Runtime (smaller image, no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Tickets.Api.dll"]