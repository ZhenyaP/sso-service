#!/bin/bash

echo "Running run_api.sh at $(date)"

COUNTER=0;
echo "Waiting when Cognito Secrets and JWKS will be stored in secrets Docker volume - attempt $((COUNTER+1))"
until $([[ -f /run/secrets/cognito-secrets.json ]] && [[ -f /run/secrets/rsa.json ]]);
do
   if [ $((++COUNTER)) -eq 30 ]; then
       break
   fi
   echo "Waiting when Cognito Secrets will be stored in secrets Docker volume - attempt $((COUNTER+1))"
   sleep 10;
done
if [ $((COUNTER)) -lt 30 ]; then
   echo 'Cognito Secrets were uploaded to Docker'
   echo 'Starting Identity Provider API project...'
   dotnet IdentityProvider.API.dll; 
   tail -f /dev/null
   echo 'Identity Provider API project was started'         
else
   echo 'Error occurred when creating Cognito Secrets'  
fi