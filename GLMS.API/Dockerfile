FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GLMS.API/GLMS.API.csproj", "GLMS.API/"]
COPY ["GLMS.Shared/GLMS.Shared.csproj", "GLMS.Shared/"]
RUN dotnet restore "GLMS.API/GLMS.API.csproj"
COPY . .
WORKDIR "/src/GLMS.API"
RUN dotnet build "GLMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GLMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GLMS.API.dll"]
