variable "region" {
  default = "us-east-1"
}

variable "profile" {
  description = "Please specify which profile you wan to use?"
  default = "dummy"
}

variable "amis" {
  type = "map"
  default = {
    qa = "ami-12345"
    uat = "ami-12345"
    prod = "ami-12345"
  }
}

variable "cognito_lambda_role" {
   default = "cognito_lambda_role"
   default = "dummy"
}

variable "nginx_server_names" {
  type = "map"
  default = {
    qa = "identity-provider.awsqa.net"
    uat = "identity-provider.awsuat.net"
    prod = "identity-provider.net"
  }
}

variable "nginx_status_access" {
  type = "map"
  default = {
    qa = "deny <CIDR_1>;deny <CIDR_2>"
    uat = "deny <CIDR_1>;deny <CIDR_2>"
    prod = "deny <CIDR_1>;deny <CIDR_2>"
  }
}

variable "nginx_set_real_ip_from" {
  type = "map"
  default = {
    qa = "set_real_ip_from <CIDR_1>;set_real_ip_from <CIDR_2>;"
    uat = "set_real_ip_from <CIDR_1>;set_real_ip_from <CIDR_2>;"
    prod = "set_real_ip_from <CIDR_1>;set_real_ip_from <CIDR_2>;"
  }
}

variable "coreclr_profiler" {
   description = "coreclr_profiler"
   default = "dummy"
}

variable "coreclr_newrelic_home" {
   description = "coreclr_newrelic_home"
   default = "dummy"
}

variable "coreclr_profiler_path" {
   description = "coreclr_profiler_path"
   default = "dummy"
}

variable "idp_api_newrelic_app_name" {
   description = "idp_api_newrelic_app_name"
   default = "dummy"
}

variable "instance_type" {
  default = "t3.large"
  description = "Specify instance type you want to use? e.g. t2.micro"
}

variable "key_name" {
  description = "Key name that can be used later to SSH"
  default = "restApi"
}

variable "idp_nginx_name" {
  description = "application name you want to deploy?"
  default = "dummy"
}

variable "idp_api_name" {
  description = "application name you want to deploy?"
  default = "dummy"
}

variable "idp_cert_name" {
  description = "application name you want to deploy?"
  default = "dummy"
}

variable "app_version" {
  description = "application version you want to deploy? e.g. 1.1"
  default = "dummy"
}

variable "subnet_elb_1" {
  description = "subnet_elb_1"
  default = "dummy"
}

variable "subnet_elb_2" {
  description = "subnet_elb_2"
  default = "dummy"
}

variable "subnet_asg_1" {
  description = "subnet_asg_1"
  default = "dummy"
}

variable "subnet_asg_2" {
  description = "subnet_asg_2"
  default = "dummy"
}

variable "idp_nginx_repository_url" {
  description = "URL of the repository for nginx--idp Docker image"
  default = "dummy"
}

variable "idp_api_repository_url" {
  description = "URL of the repository for -idp-api Docker image"
  default = "dummy"
}

variable "idp_cert_repository_url" {
  description = "URL of the repository for idp-certmanager Docker image"
  default = "dummy"
}

variable "idp_docker_repository_url" {
  description = "URL of the repository for -idp-docker Docker image"
  default = "dummy"
}

variable "idp_nginx_newrelic_repository_url" {
  description =  "URL of the repository for nginx-newrelic Docker image"
  default = "dummy"
}

variable "ecr_aws_account" {
  description = "AWS Account for ECR repos"
  default = "dummy"
}

variable "ecr_aws_region" {
  description = "AWS Region for ECR repos"
  default = "dummy"
}

variable "idp_docker_name" {
  description = "idp_docker_name"
  default = "dummy"
}

variable "idp_nginx_newrelic_name" {
  description = "idp_nginx_newrelic_name"
  default = "dummy"
}

variable "iam_role" {
  description = "IAM Role arn:aws:iam::aws:policy/service-role/AmazonEC2ContainerServiceRole"
  default = "dummy"
}

variable "elb_name" {
  description = "ELB name for SQSPoller"
  default = "dummy"
}

variable "lb_target_group_name" {
  description = "lb_target_group_name"
  default = "dummy"
}

variable "vpc_id" {
  description = "vpc_id"
  default = "dummy"
}

variable "autoscaling_group_name" {
  description = "AutoScaling Group name"
  default = "dummy"
}

variable "test_autoscaling_group_name" {
  description = "Test AutoScaling Group name"
  default = "dummy"
}

variable "cluster_name" {
  description = "Cluster Name"
  default = "dummy"
}

variable "service_name" {
  description = "Service Name"
  default = "dummy"
}

variable "nginx_service_name" {
  description = "Service Name"
  default = "dummy"
}

variable "deploy_env" {
  description = "Deploy environment Development, Sandbox, QA or Production"
  default = "dummy"
}

variable "docker_image_tag" {
  description = "Docker image tag in format <deploy_environment-jenkins_build_id>"
  default = "dummy"
}

variable "iam_instance_profile" {
  description = "IAM instance profile"
  default = "dummy"
}

variable "iam_test_instance_profile" {
  description = "IAM Test instance profile"
  default = "dummy"
}

variable "ec2_security_group" {
  description = "EC2 Security Group"
  type        = "list"
  default = ["sg-00a1b08018291a26f"]
}

variable "elb_security_group" {
  description = "ELB Security Group"
  type        = "list"
  default = ["sg-00a1b08018291a26f"]
}

/*
variable "idp_elb_ingress_security_group" {
  description = "Security Group"
  type        = "list"
  default = ["sg-00a1b08018291a26f"]
}

variable "idp_elb_security_group" {
  description = "ELB Security Group"
  default = "dummy"
}

variable "idp_ec2_security_group" {
  description = "EC2 Security Group"
  default = "dummy"
}
*/

variable "Tag_App" {
  type        = "map"
  description = "tag mapping"
  default = {}
}

variable "identity_task_count" {
  description = "identity_task_count"
  default = "dummy"
}

variable "newrelic_license" {
  description = "newrelic_license"
  default = "dummy"
}

variable "domain_name" {
  description = "domain_name"
  default = "dummy"
}

variable "idp_asg_min_size" {
  description = "idp_asg_min_size"
  default = "dummy"
}

variable "idp_asg_max_size" {
  description = "idp_asg_max_size"
  default = "dummy"
}

variable "idp_test_asg_min_size" {
  description = "idp_test_asg_min_size"
  default = "dummy"
}

variable "idp_test_asg_max_size" {
  description = "idp_test_asg_max_size"
  default = "dummy"
}

variable "idp_route53_zone_id" {
  description = "idp_route53_zone_id"
  default = "dummy"
}

variable "idp_nginx_newrelic_app_name" {
  description = "idp_nginx_newrelic_app_name"
  default = "dummy"
}

variable "cognito_user_pool_id" {
  description = "cognito_user_pool_id"
}

variable "cognito_user_pool_arn" {
  description = "cognito_user_pool_id"
}
