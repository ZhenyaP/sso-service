[
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_API_NAME}_1",
    "cpu": 256,
    "image": "${IDP_API_REPOSITORY_URL}",
    "entryPoint": ["/src/IdentityProvider.API/run_api.sh"],
    "environment" : [
       { "name" : "ASPNETCORE_URLS", "value" : "http://+:5000" },
       { "name" : "ASPNETCORE_ENVIRONMENT", "value" : "${DEPLOY_ENV}" },
       { "name" : "CORECLR_ENABLE_PROFILING", "value" : "1" },
       { "name" : "CORECLR_PROFILER", "value" : "${CORECLR_PROFILER}" },
       { "name" : "CORECLR_NEWRELIC_HOME", "value" : "${CORECLR_NEWRELIC_HOME}" },
       { "name" : "CORECLR_PROFILER_PATH", "value" : "${CORECLR_PROFILER_PATH}" },
       { "name" : "NEW_RELIC_LICENSE_KEY", "value" : "${NEWRELIC_LICENSE}" },
       { "name" : "NEW_RELIC_APP_NAME", "value" : "${IDP_API_NEWRELIC_APP_NAME}" }
    ],
    "volumesFrom":[{ "sourceContainer": "${IDP_CERT_NAME}", "readOnly": false }]
  },
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_API_NAME}_2",
    "cpu": 256,
    "image": "${IDP_API_REPOSITORY_URL}",
    "entryPoint": ["/src/IdentityProvider.API/run_api.sh"],
    "environment" : [
       { "name" : "ASPNETCORE_URLS", "value" : "http://+:5000" },
       { "name" : "ASPNETCORE_ENVIRONMENT", "value" : "${DEPLOY_ENV}" },
       { "name" : "CORECLR_ENABLE_PROFILING", "value" : "1" },
       { "name" : "CORECLR_PROFILER", "value" : "${CORECLR_PROFILER}" },
       { "name" : "CORECLR_NEWRELIC_HOME", "value" : "${CORECLR_NEWRELIC_HOME}" },
       { "name" : "CORECLR_PROFILER_PATH", "value" : "${CORECLR_PROFILER_PATH}" },
       { "name" : "NEW_RELIC_LICENSE_KEY", "value" : "${NEWRELIC_LICENSE}" },
       { "name" : "NEW_RELIC_APP_NAME", "value" : "${IDP_API_NEWRELIC_APP_NAME}" }
    ],
    "volumesFrom":[{ "sourceContainer": "${IDP_CERT_NAME}", "readOnly": false }]
  },
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_API_NAME}_3",
    "cpu": 256,
    "image": "${IDP_API_REPOSITORY_URL}",
    "entryPoint": ["/src/IdentityProvider.API/run_api.sh"],
    "environment" : [
       { "name" : "ASPNETCORE_URLS", "value" : "http://+:5000" },
       { "name" : "ASPNETCORE_ENVIRONMENT", "value" : "${DEPLOY_ENV}" },
       { "name" : "CORECLR_ENABLE_PROFILING", "value" : "1" },
       { "name" : "CORECLR_PROFILER", "value" : "${CORECLR_PROFILER}" },
       { "name" : "CORECLR_NEWRELIC_HOME", "value" : "${CORECLR_NEWRELIC_HOME}" },
       { "name" : "CORECLR_PROFILER_PATH", "value" : "${CORECLR_PROFILER_PATH}" },
       { "name" : "NEW_RELIC_LICENSE_KEY", "value" : "${NEWRELIC_LICENSE}" },
       { "name" : "NEW_RELIC_APP_NAME", "value" : "${IDP_API_NEWRELIC_APP_NAME}" }
    ],
    "volumesFrom":[{ "sourceContainer": "${IDP_CERT_NAME}", "readOnly": false }]
  },
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_API_NAME}_4",
    "cpu": 256,
    "image": "${IDP_API_REPOSITORY_URL}",
    "entryPoint": ["/src/IdentityProvider.API/run_api.sh"],
    "environment" : [
       { "name" : "ASPNETCORE_URLS", "value" : "http://+:5000" },
       { "name" : "ASPNETCORE_ENVIRONMENT", "value" : "${DEPLOY_ENV}" },
       { "name" : "CORECLR_ENABLE_PROFILING", "value" : "1" },
       { "name" : "CORECLR_PROFILER", "value" : "${CORECLR_PROFILER}" },
       { "name" : "CORECLR_NEWRELIC_HOME", "value" : "${CORECLR_NEWRELIC_HOME}" },
       { "name" : "CORECLR_PROFILER_PATH", "value" : "${CORECLR_PROFILER_PATH}" },
       { "name" : "NEW_RELIC_LICENSE_KEY", "value" : "${NEWRELIC_LICENSE}" },
       { "name" : "NEW_RELIC_APP_NAME", "value" : "${IDP_API_NEWRELIC_APP_NAME}" }
    ],
    "volumesFrom":[{ "sourceContainer": "${IDP_CERT_NAME}", "readOnly": false }]
  },
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_CERT_NAME}",
    "cpu": 256,
    "image": "${IDP_CERT_REPOSITORY_URL}",
    "entryPoint": ["bash", "-c", "dotnet IdentityProvider.CertManager.dll; tail -f /dev/null"],
    "environment" : [
       { "name" : "ASPNETCORE_URLS", "value" : "http://+:5000" },
       { "name" : "ASPNETCORE_ENVIRONMENT", "value" : "${DEPLOY_ENV}" }
    ],
    "mountPoints":[{ "sourceVolume": "secrets", "containerPath": "/run/secrets", "readOnly": false }]
  },
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_NGINX_NAME}",
    "cpu": 256,
    "image": "${IDP_NGINX_REPOSITORY_URL}",
    "entryPoint": ["/tmp/run_nginx.sh"],
    "environment" : [
       { "name" : "NGINX_SERVER_NAME", "value" : "${NGINX_SERVER_NAME}" },
       { "name" : "NGINX_STATUS_ACCESS", "value" : "${NGINX_STATUS_ACCESS}" }
    ],
    "links": [
    "${IDP_API_NAME}_1:${IDP_API_NAME}_1","${IDP_API_NAME}_2:${IDP_API_NAME}_2",
    "${IDP_API_NAME}_3:${IDP_API_NAME}_3","${IDP_API_NAME}_4:${IDP_API_NAME}_4",
    "${IDP_CERT_NAME}:${IDP_CERT_NAME}"
    ],
    "portMappings": [
        {
            "containerPort": 443,
            "hostPort": 443
        }
    ],
    "volumesFrom":[{ "sourceContainer": "${IDP_CERT_NAME}", "readOnly": false }]
  },
  {
    "essential": true,
    "memory": 256,
    "name": "${IDP_NGINX_NEWRELIC_NAME}",
    "cpu": 256,
    "image": "${IDP_NGINX_NEWRELIC_REPOSITORY_URL}",
    "entryPoint": ["/opt/start.sh"],
    "links": ["${IDP_NGINX_NAME}:${IDP_NGINX_NAME}"],
    "environment" : [
       { "name" : "NEWRELIC_APP", "value" : "${IDP_NGINX_NEWRELIC_APP_NAME}" },
       { "name" : "NGINX_STATUS_URL", "value" : "http://nginx--idp/nginx_status" },
       { "name" : "NEWRELIC_LICENSE", "value" : "${NEWRELIC_LICENSE}" }
    ]
  }
]