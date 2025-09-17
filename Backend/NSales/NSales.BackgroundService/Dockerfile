#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Docker
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BTCSTXSwap.BackgroundService/BTCSTXSwap.BackgroundService.csproj", "BTCSTXSwap.BackgroundService/"]
COPY ["BTCSTXSwap.Application/BTCSTXSwap.Application.csproj", "BTCSTXSwap.Application/"]
COPY ["BTCSTXSwap.Domain/BTCSTXSwap.Domain.csproj", "BTCSTXSwap.Domain/"]
COPY ["Core.Domain/Core.Domain.csproj", "Core.Domain/"]
COPY ["BTCSTXSwap.DTO/BTCSTXSwap.DTO.csproj", "BTCSTXSwap.DTO/"]
COPY ["DB.Infra/DB.Infra.csproj", "DB.Infra/"]
RUN dotnet restore "BTCSTXSwap.BackgroundService/BTCSTXSwap.BackgroundService.csproj"
COPY . .
WORKDIR "/src/BTCSTXSwap.BackgroundService"
RUN dotnet build "BTCSTXSwap.BackgroundService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BTCSTXSwap.BackgroundService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BTCSTXSwap.BackgroundService.dll"]
