# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /publish \
    -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0
WORKDIR /app

COPY --from=build /publish .

# HopperHomeAssignment is the single binary produced by dotnet publish
ENTRYPOINT ["./HopperHomeAssignment"]
