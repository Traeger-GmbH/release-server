#--------------------------------------------------------------------------------------------------
# <copyright file="Dockerfile" company="Traeger IndustryComponents GmbH">
#     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
# </copyright>
# <author>Fabian Traeger</author>
# <author>Timo Walter</author>
#--------------------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
LABEL maintainer=info@traeger.de

WORKDIR /app

# Copy resources and restore
COPY ./sources/ReleaseServer/ReleaseServer.WebApi .
RUN dotnet restore

# Build the app
RUN dotnet publish -c Release -o output/ReleaseServer

# TODO use the asp .net core runtime & fix the certificate issue #40
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS runtime
WORKDIR /app
COPY --from=builder /app/output ./
ADD ./defaults/appsettings.json /app/

ENTRYPOINT [ "dotnet", "ReleaseServer/ReleaseServer.WebApi.dll", "--urls", "http://0.0.0.0:5000"]
