FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TawtAi.Api.csproj .
RUN dotnet restore TawtAi.Api.csproj

COPY . .
RUN dotnet publish TawtAi.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "TawtAi.Api.dll"]
