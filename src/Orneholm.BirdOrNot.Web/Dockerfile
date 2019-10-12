FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["src/Orneholm.BirdOrNot.Web/Orneholm.BirdOrNot.Web.csproj", "src/Orneholm.BirdOrNot.Web/"]
RUN dotnet restore "src/Orneholm.BirdOrNot.Web/Orneholm.BirdOrNot.Web.csproj"
COPY . .
WORKDIR "/src/src/Orneholm.BirdOrNot.Web"
RUN dotnet build "Orneholm.BirdOrNot.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orneholm.BirdOrNot.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orneholm.BirdOrNot.Web.dll"]