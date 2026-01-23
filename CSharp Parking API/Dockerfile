# Dockerfile for CSharp Parking API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CSharp Parking API/CSharp Parking API.csproj", "."]
RUN dotnet restore "CSharp Parking API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet publish "CSharp Parking API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CSharp Parking API.dll"]
