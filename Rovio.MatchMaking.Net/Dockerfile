FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServerEngineerTest.sln ./
COPY Rovio.MatchMaking.Net/Rovio.MatchMaking.Net.csproj Rovio.MatchMaking.Net/
COPY Rovio.MatchMaking/Rovio.MatchMaking.csproj Rovio.MatchMaking/
COPY Rovio.MatchMaking.Repositories/Rovio.MatchMaking.Repositories.csproj Rovio.MatchMaking.Repositories/

RUN dotnet restore Rovio.MatchMaking.Net/Rovio.MatchMaking.Net.csproj

COPY . .

WORKDIR /src/Rovio.MatchMaking.Net

RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Rovio.MatchMaking.Net.dll"]