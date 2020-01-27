# To use this script, you should first intall jq JSON processor
# (see this article: https://stackoverflow.com/questions/33184780/install-jq-json-processor-on-ubuntu-10-04)

function jsonValue() {
KEY=$1
num=$2
awk -F"[,:}]" '{for(i=1;i<=NF;i++){if($i~/'$KEY'\042/){print $(i+1)}}}' | tr -d '"' | sed -n ${num}p
}

decode_base64_url() {
  local len=$((${#1} % 4))
  local result="$1"
  if [ $len -eq 2 ]; then result="$1"'=='
  elif [ $len -eq 3 ]; then result="$1"'=' 
  fi
  echo "$result" | tr '_-' '/+' | openssl enc -d -base64
}

decode_jwt(){
   decode_base64_url $(echo -n $2 | cut -d "." -f $1) | jq .
}

# Decode JWT header
alias jwth="decode_jwt 1"

# Decode JWT Payload
alias jwtp="decode_jwt 2"

clientId="<client_id>"
clientSecret="<client_secret>"

basicToken=$(echo -n "${clientId}:${clientSecret}" | base64 | tr -d '\r' | tr -d '\n')
token=$(curl -s -d \
"grant_type=client_credentials&scope=<scope>" \
-H "Content-Type: application/x-www-form-urlencoded" \
-H "Authorization: Basic $basicToken" \
-X POST https://<server-domain>.auth.us-east-1.amazoncognito.com/oauth2/token | jsonValue access_token)
jti=$(decode_jwt 2 $token | jsonValue jti | tr -d '[:space:]')
echo "{\"access_token\":\"$token\",\"session_id\":\"$jti\"}"
