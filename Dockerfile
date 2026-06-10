FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PingApp.Domain/PingApp.Domain.csproj", "PingApp.Domain/"]
COPY ["PingApp.Application/PingApp.Application.csproj", "PingApp.Application/"]
COPY ["PingApp.Infrastructure/PingApp.Infrastructure.csproj", "PingApp.Infrastructure/"]
COPY ["PingApp.Worker/PingApp.Worker.csproj", "PingApp.Worker/"]

RUN dotnet restore "PingApp.Worker/PingApp.Worker.csproj"

COPY . .
WORKDIR "/src/PingApp.Worker"
RUN dotnet build "PingApp.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PingApp.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PingApp.Worker.dll"]