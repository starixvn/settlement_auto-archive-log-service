FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder
WORKDIR /src

COPY ./Sources/AutoArchiveLog ./AutoArchiveLog
COPY ./Sources/AutoArchiveLog.Service/AutoArchiveLog.Service.csproj ./AutoArchiveLog.Service/AutoArchiveLog.Service.csproj

WORKDIR /src/AutoArchiveLog.Service
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/AutoArchiveLog.Service .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENV TZ="America/La_Paz"
ENTRYPOINT ["dotnet", "AutoArchiveLog.Service.dll"]
