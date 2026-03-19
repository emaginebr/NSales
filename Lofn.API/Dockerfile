#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Docker
WORKDIR /app
EXPOSE 8080

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
COPY ["Lofn.API/Lofn.API.csproj", "Lofn.API/"]
COPY ["Lofn.GraphQL/Lofn.GraphQL.csproj", "Lofn.GraphQL/"]
COPY ["Lofn.Application/Lofn.Application.csproj", "Lofn.Application/"]
COPY ["Lofn.Domain/Lofn.Domain.csproj", "Lofn.Domain/"]
COPY ["Lofn/Lofn.csproj", "Lofn/"]
COPY ["Lofn.Infra.Interfaces/Lofn.Infra.Interfaces.csproj", "Lofn.Infra.Interfaces/"]
COPY ["Lofn.Infra/Lofn.Infra.csproj", "Lofn.Infra/"]
RUN dotnet restore "Lofn.API/Lofn.API.csproj"
COPY . .
WORKDIR "/src/Lofn.API"
RUN dotnet build "Lofn.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lofn.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lofn.API.dll"]
