#!/bin/bash
function checkoutput {
    output=$?
    if [ output == 0 ] 
    then
        echo "Successfully created file (no change)"
    elif [ output == 1 ]
    then
        echo "Could not create file"
        exit 1 
    else 
        echo "Successfully created file (with change)"
    fi

}

function jsonValue {
    KEY=$1
    num=$2
    awk -F"[,:}]" '{for(i=1;i<=NF;i++){if($i~/'$KEY'\042/){print $(i+1)}}}' | tr -d '"' | sed -n ${num}p
}

function runDeploy {
    zip=$1
    newRelicLicenseKey=$2
    imageTag=$3
    deployEnv=$4
    awsAccount=$5
    awsRegion=$6
    #terraform destroy -force
    #TODO: Do not destroy before creating. 0 down time is a requirement.
    #TODO: How do you roll back?
    #terraform destroy -target aws_ecs_service.sqspoller -target aws_ecs_task_definition.sqspoller-taskdef -force

    if [ "$deployEnv" = "QA" ]; then
    true
    #    terraform import aws_cognito_user_pool_client.user_job_scheduler_client "abcd" || true
    #   terraform state rm aws_launch_configuration.identitycs

    elif [ "$deployEnv" = "UAT" ]; then
        terraform import aws_cognito_user_pool.user_pool "abc" || true
        
    #   terraform state rm aws_launch_configuration.identitycs && \
    #   terraform import aws_launch_configuration.identitycs identity-20181029171432502300000001 || true
    elif [ "$deployEnv" = "Prod" ]; then
        terraform import aws_cognito_user_pool.user_pool "nnn" || true
    fi;
    
    terraform apply -var deploy_env=\"${deployEnv}\" -var newrelic_license=\"${newRelicLicenseKey}\" \
    -var docker_image_tag=\"${imageTag}\" -var ecr_aws_account=\"${awsAccount}\" -var ecr_aws_region=\"${awsRegion}\" \
    -auto-approve

    checkoutput

    echo "zip=\"${zip}\""

    touch terraform_state.yml
    cat >> terraform_state.yml << EOM
    LatestZip: "${zip}"
EOM

exit 0
}

