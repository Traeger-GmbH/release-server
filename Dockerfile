#--------------------------------------------------------------------------------------------------
# <copyright file="Dockerfile" company="Traeger IndustryComponents GmbH">
#     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2026.
# </copyright>
#--------------------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Restore first, so the package layer is cached as long as the project file does not change
COPY ./sources/ReleaseServer/ReleaseServer.WebApi/ReleaseServer.WebApi.csproj ./
RUN dotnet restore

COPY ./sources/ReleaseServer/ReleaseServer.WebApi/ ./
RUN dotnet publish -c Release --no-restore -o /output/ReleaseServer

# The Debian based ASP.NET image contains ICU, which is required for the culture keys of the release notes
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
LABEL maintainer=info@traeger.de

# Existing installations use volumes owned by root. Switching to the non-root "app" user of the base
# image requires adjusting the volume permissions first and is done in a separate step.
USER root

# The port is defined by the entrypoint (--urls), like in all previous versions
ENV ASPNETCORE_HTTP_PORTS=

WORKDIR /app
COPY --from=builder /output ./
COPY ./defaults/appsettings.json /app/

EXPOSE 5000

# Artifacts and backups are stored relative to the working directory: /app/artifacts and /app/backups
ENTRYPOINT [ "dotnet", "ReleaseServer/ReleaseServer.WebApi.dll", "--urls", "http://0.0.0.0:5000"]
