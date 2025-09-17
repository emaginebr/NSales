#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Docker
WORKDIR /app
EXPOSE 443

# Build libgdiplus with pango
RUN apt-get update && apt-get install  -y --allow-unauthenticated \
     libgif-dev autoconf libtool automake build-essential gettext libglib2.0-dev libcairo2-dev libtiff-dev libexif-dev
RUN apt-get update && apt-get install -y libpango1.0-dev 

RUN apt-get install -y --allow-unauthenticated git
RUN git clone https://github.com/mono/libgdiplus.git /libgdiplus
WORKDIR /libgdiplus
RUN ./autogen.sh --with-pango --prefix=/usr
RUN make  
RUN make install

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MonexUp.API/monexup.com.pfx", "MonexUp.API/"]
COPY ["MonexUp.API/MonexUp.API.csproj", "MonexUp.API/"]
COPY ["MonexUp.Application/MonexUp.Application.csproj", "MonexUp.Application/"]
COPY ["MonexUp.Domain/MonexUp.Domain.csproj", "MonexUp.Domain/"]
COPY ["Core.Domain/Core.Domain.csproj", "Core.Domain/"]
COPY ["MonexUp.DTO/MonexUp.DTO.csproj", "MonexUp.DTO/"]
COPY ["DB.Infra/DB.Infra.csproj", "DB.Infra/"]
RUN dotnet restore "MonexUp.API/MonexUp.API.csproj"
COPY . .
WORKDIR "/src/MonexUp.API"
RUN dotnet build "MonexUp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MonexUp.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MonexUp.API.dll"]