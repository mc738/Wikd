FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wikd.App/Wikd.App.fsproj", "Wikd.App/"]
RUN dotnet restore "Wikd.App/Wikd.App.fsproj"
COPY . .
WORKDIR "/src/Wikd.App"
RUN dotnet build "Wikd.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wikd.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wikd.App.dll"]
