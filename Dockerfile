FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS builder
LABEL maintainer=info@traeger.de

WORKDIR /app

# Copy resources and restore
COPY ./Sources/ReleaseServer/ReleaseServer.WebApi .
RUN dotnet restore

# Build the app
RUN dotnet publish -c Release -o output/ReleaseServer

# TODO .../core/runtime does not work (framework not found) -> investigate the issue and fix it
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS runtime
WORKDIR /app
COPY --from=builder /app/output ./

ENTRYPOINT [ "dotnet", "ReleaseServer/ReleaseServer.WebApi.dll", "--urls", "https://0.0.0.0:5001"]