FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY identidad-service/identidad-service.csproj identidad-service/
RUN dotnet restore identidad-service/identidad-service.csproj

COPY identidad-service/ identidad-service/
RUN dotnet publish identidad-service/identidad-service.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "identidad-service.dll"]
