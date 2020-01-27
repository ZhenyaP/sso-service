#!/bin/bash

cd "/media/sf_E_DRIVE//-sso-identity-provider/terraform"
docker-compose build

#You can also do it this way (without docker-compose build):
#docker cp . -idp-terraform:.

docker-compose up -d
docker logs -idp-terraform

