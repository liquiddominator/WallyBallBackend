FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY wally-service/WallyBallBackend.csproj wally-service/
RUN dotnet restore wally-service/WallyBallBackend.csproj

COPY wally-service/ wally-service/
RUN dotnet publish wally-service/WallyBallBackend.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "WallyBallBackend.dll"]
