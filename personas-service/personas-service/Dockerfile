FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY personas-service/personas-service.csproj personas-service/
RUN dotnet restore personas-service/personas-service.csproj

COPY personas-service/ personas-service/
RUN dotnet publish personas-service/personas-service.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "personas-service.dll"]
