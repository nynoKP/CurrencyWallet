#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CurrencyWallet/CurrencyWallet.csproj", "CurrencyWallet/"]
COPY ["Infrstructure/Infrstructure/Infrastructure.csproj", "Infrstructure/Infrstructure/"]
COPY ["Infrstructure/Infrastructure.Domain/Infrastructure.Domain.csproj", "Infrstructure/Infrastructure.Domain/"]
COPY ["Infrstructure/Infrastructure.Repository/Infrastructure.Repository.csproj", "Infrstructure/Infrastructure.Repository/"]
RUN dotnet restore "./CurrencyWallet/./CurrencyWallet.csproj"
COPY . .
WORKDIR "/src/CurrencyWallet"
RUN dotnet build "./CurrencyWallet.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CurrencyWallet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CurrencyWallet.dll"]