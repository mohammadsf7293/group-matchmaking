FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServerEngineerTest.sln ./
COPY Rovio.MatchMaking.Console/Rovio.MatchMaking.Console.csproj ./Rovio.MatchMaking.Console/
COPY Rovio.MatchMaking/Rovio.MatchMaking.csproj ./Rovio.MatchMaking/
COPY Rovio.MatchMaking.Repositories/Rovio.MatchMaking.Repositories.csproj ./Rovio.MatchMaking.Repositories/

RUN dotnet restore ./Rovio.MatchMaking.Console/Rovio.MatchMaking.Console.csproj

COPY . .

WORKDIR /src/Rovio.MatchMaking.Console
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Rovio.MatchMaking.Console.dll"]