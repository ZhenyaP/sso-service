FROM microsoft/dotnet:2.2.103-sdk

WORKDIR /src
COPY IdentityProvider.SecretManager/. IdentityProvider.SecretManager/
COPY IdentityProvider.Common/. IdentityProvider.Common/

WORKDIR /src/IdentityProvider.SecretManager
RUN dotnet publish IdentityProvider.SecretManager.csproj -c Release -o /app;

WORKDIR /app