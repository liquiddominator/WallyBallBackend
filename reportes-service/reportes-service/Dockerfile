FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY reportes-service/reportes-service.csproj reportes-service/
RUN dotnet restore reportes-service/reportes-service.csproj

COPY reportes-service/ reportes-service/
RUN dotnet publish reportes-service/reportes-service.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "reportes-service.dll"]
