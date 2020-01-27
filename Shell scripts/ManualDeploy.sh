cd ..
docker ps -a | awk '{ print $1,$2 }' | grep -E 'nginx-idp|idp-api|idp-certmanager|nginx-newrelic' | \
awk '{ print $1 }' | xargs -I % sh -c 'docker stop % && docker rm %' || true
docker stop idp-api-unittest && docker rm idp-api-unittest || true
docker rmi -f nginx-idp idp-api idp-certmanager nginx-newrelic || true
docker-compose down
docker-compose build
docker run --name idp-api-unittest idp-api bash -c "set -eu -o pipefail && \
dotnet test ../src/IdentityProvider.API.Tests/IdentityProvider.API.Tests.csproj"
if [ "$?" -ne "0" ]; then
  echo "Unit tests for IdentityProvider.API project failed!"
  exit 1
fi
docker-compose up -d --no-recreate --scale idp-api=4