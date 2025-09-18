FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

COPY ./Sources/AutoArchiveLog ./AutoArchiveLog
COPY ./Sources/AutoArchiveLog.Worker/AutoArchiveLog.Worker.csproj ./AutoArchiveLog.Worker/

WORKDIR /src/AutoArchiveLog.Worker
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \ 
	dotnet restore -v n

COPY ./Sources/AutoArchiveLog.Worker .

RUN --mount=type=cache,target=/root/.nuget/packages \
dotnet publish -r linux-x64 --self-contained false -c release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "AutoArchiveLog.Worker.dll"]
