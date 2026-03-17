# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TanHueVien.sln", "./"]
COPY ["src/SPage.Web/SPage.Web.csproj", "src/SPage.Web/"]
COPY ["src/SPage.Application/SPage.Application.csproj", "src/SPage.Application/"]
COPY ["src/SPage.Domain/SPage.Domain.csproj", "src/SPage.Domain/"]
COPY ["src/SPage.Infrastructure/SPage.Infrastructure.csproj", "src/SPage.Infrastructure/"]

RUN dotnet restore "TanHueVien.sln"

COPY . .
RUN dotnet publish "src/SPage.Web/SPage.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "SPage.Web.dll"]
