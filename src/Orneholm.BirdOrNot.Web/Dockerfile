#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Orneholm.BirdOrNot.Web/Orneholm.BirdOrNot.Web.csproj", "Orneholm.BirdOrNot.Web/"]
COPY ["src/Orneholm.BirdOrNot.Core/Orneholm.BirdOrNot.Core.csproj", "Orneholm.BirdOrNot.Core/"]
RUN dotnet restore "Orneholm.BirdOrNot.Web/Orneholm.BirdOrNot.Web.csproj"
COPY /src .
WORKDIR "/src/Orneholm.BirdOrNot.Web"
RUN dotnet build "Orneholm.BirdOrNot.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orneholm.BirdOrNot.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orneholm.BirdOrNot.Web.dll"]
