#!/bin/bash

echo "Running run_nginx.sh at $(date)"
echo "Substituting environment-specific values to nginx.conf file"
sed -i "s|\$sed\.NGINX_SERVER_NAME|$NGINX_SERVER_NAME|g; \
  s|\$sed\.NGINX_STATUS_ACCESS|$NGINX_STATUS_ACCESS|g; \
  s|\$sed\.NGINX_SET_REAL_IP_FROM|$NGINX_SET_REAL_IP_FROM|g" /etc/nginx/nginx.conf

echo "Removing all content from the secrets Docker volume"
COUNTER=0;
rm -rf /run/secrets/*;
echo "Uploading all secrets to Docker - attempt $((COUNTER+1))"
until $(curl -d '' -X POST -v http://idp-certmanager:5000/api/Secrets/Upload);
do
  if [ $((++COUNTER)) -eq 10 ]; then
     break
  fi
  sleep 10;
  echo "Uploading all secrets to Docker - attempt $((COUNTER+1))"
done
if [ $((COUNTER)) -lt 10 ]; then
   COUNTER=0;
   echo "Waiting when all secrets will be stored in secrets Docker volume - attempt $((COUNTER+1))"
   until $([[ -f /run/secrets/server.cert.pem ]] && [[ -f /run/secrets/server.nopass.key.pem ]] && \
   [[ -f /run/secrets/ca-chain.crl.pem ]] && [[ -f /run/secrets/cognito-secrets.json ]] && \
   [[ -f /run/secrets/rsa.json ]]);
   do
      if [ $((++COUNTER)) -eq 10 ]; then
         break
      fi
      echo "Waiting when all secrets will be stored in secrets Docker volume - attempt $((COUNTER+1))"
      sleep 10;
   done
   if [ $((COUNTER)) -lt 10 ]; then
      echo 'Secrets were uploaded to Docker'
      ln -sf /dev/stdout /etc/nginx/nginx.access.log && ln -sf /dev/stderr /etc/nginx/nginx.error.log;
      echo 'NGINX version:'
      nginx -v
      echo 'NGINX configure arguments:'
      nginx -V 2>&1 | grep arguments
      echo 'Starting NGINX...'
      nginx -g 'daemon off;';
      echo 'NGINX was started'         
   else
      echo 'Error occurred when uploading secrets to Docker'  
   fi
else
   echo 'Error occurred when uploading secrets to Docker'  
fi