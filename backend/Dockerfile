FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY GameOfLife.sln ./

COPY src/GameOfLife.Api/GameOfLife.Api.csproj ./src/GameOfLife.Api/
RUN dotnet restore ./src/GameOfLife.Api/GameOfLife.Api.csproj

COPY src ./src

WORKDIR /app/src/GameOfLife.Api
RUN dotnet publish -c release -o /out

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS final
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GameOfLife.Api.dll"]