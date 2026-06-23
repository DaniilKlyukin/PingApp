FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PingApp.Domain/PingApp.Domain.csproj", "PingApp.Domain/"]
COPY ["PingApp.Application/PingApp.Application.csproj", "PingApp.Application/"]
COPY ["PingApp.Infrastructure/PingApp.Infrastructure.csproj", "PingApp.Infrastructure/"]
COPY ["PingApp.Api/PingApp.Api.csproj", "PingApp.Api/"]

RUN dotnet restore "PingApp.Api/PingApp.Api.csproj"

COPY . .
WORKDIR "/src/PingApp.Api"

RUN dotnet build "PingApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PingApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PingApp.Api.dll"]