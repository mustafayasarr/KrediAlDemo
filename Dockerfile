# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/KrediAl.API/KrediAl.API.csproj", "KrediAl.API/"]
COPY ["src/KrediAl.Application/KrediAl.Application.csproj", "KrediAl.Application/"]
COPY ["src/KrediAl.Domain/KrediAl.Domain.csproj", "KrediAl.Domain/"]
COPY ["src/KrediAl.Infrastructure/KrediAl.Infrastructure.csproj", "KrediAl.Infrastructure/"]

RUN dotnet restore "KrediAl.API/KrediAl.API.csproj"

# Copy everything else and build
COPY src/ .
RUN dotnet build "KrediAl.API/KrediAl.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "KrediAl.API/KrediAl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KrediAl.API.dll"]
