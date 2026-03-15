FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/PantryPlanner.sln backend/
COPY backend/src/PantryPlanner.Api/PantryPlanner.Api.csproj backend/src/PantryPlanner.Api/

RUN dotnet restore backend/src/PantryPlanner.Api/PantryPlanner.Api.csproj

COPY backend/ backend/

WORKDIR /src/backend/src/PantryPlanner.Api
RUN dotnet publish PantryPlanner.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PantryPlanner.Api.dll"]
