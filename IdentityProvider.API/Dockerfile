FROM microsoft/dotnet:2.2.103-sdk

RUN apt-get update && apt-get install -y --no-install-recommends dos2unix

WORKDIR /src
COPY IdentityProvider.API/. IdentityProvider.API/
COPY IdentityProvider.Common/. IdentityProvider.Common/
COPY IdentityProvider.API.Tests/. IdentityProvider.API.Tests/
COPY api-newrelic/. api-newrelic/

RUN dpkg -i ./api-newrelic/newrelic-netcore20-agent*.deb
RUN chmod +x ./IdentityProvider.API/run_api.sh
RUN dos2unix ./IdentityProvider.API/run_api.sh

WORKDIR /src/IdentityProvider.API
RUN dotnet publish IdentityProvider.API.csproj -c Release -o /app;

WORKDIR /src/IdentityProvider.API.Tests
RUN dotnet restore IdentityProvider.API.Tests.csproj

WORKDIR /app