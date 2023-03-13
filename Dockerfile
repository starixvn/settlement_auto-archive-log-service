FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder
WORKDIR /src

COPY ./Sources/AutoEarly ./AutoEarly
COPY ./Sources/AutoEarlyOtherSports.Service/AutoEarlyOtherSports.Service.csproj ./AutoEarlyOtherSports.Service/AutoEarlyOtherSports.Service.csproj

WORKDIR /src/AutoEarlyOtherSports.Service
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/AutoEarlyOtherSports.Service .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0 AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "AutoEarlyOtherSports.Service.dll"]
