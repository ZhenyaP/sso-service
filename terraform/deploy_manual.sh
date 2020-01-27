#!/bin/bash

cp terraform_dev_config.txt terraform.tfvars
terraform init
terraform apply -var deploy_env=\"QA\" -var newrelic_license=\"070707e52b3d58152891dc566b22440fec325c2a\" -auto-approve
tail -f /dev/null