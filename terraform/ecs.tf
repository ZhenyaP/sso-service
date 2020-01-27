data "template_file" "identity-containerdefs-tpl" {

  template = "${file("templates/containerDefs.json.tpl")}"

  vars {
    IDP_API_REPOSITORY_URL            = "${format(var.idp_api_repository_url, var.ecr_aws_account, var.ecr_aws_region, var.docker_image_tag)}"
    IDP_CERT_REPOSITORY_URL           = "${format(var.idp_cert_repository_url, var.ecr_aws_account, var.ecr_aws_region, var.docker_image_tag)}"
    IDP_NGINX_REPOSITORY_URL          = "${format(var.idp_nginx_repository_url, var.ecr_aws_account, var.ecr_aws_region, var.docker_image_tag)}"
    IDP_NGINX_NEWRELIC_REPOSITORY_URL = "${format(var.idp_nginx_newrelic_repository_url, var.ecr_aws_account, var.ecr_aws_region, var.docker_image_tag)}"
    IDP_DOCKER_REPOSITORY_URL         = "${format(var.idp_docker_repository_url, var.ecr_aws_account, var.ecr_aws_region, var.docker_image_tag)}"
    APP_VERSION                       = "${var.app_version}"
    IDP_API_NAME                      = "${var.idp_api_name}"
    IDP_API_NEWRELIC_APP_NAME         = "${var.idp_api_newrelic_app_name}"
    IDP_NGINX_NAME                    = "${var.idp_nginx_name}"
    IDP_CERT_NAME                     = "${var.idp_cert_name}"
    IDP_NGINX_NEWRELIC_NAME           = "${var.idp_nginx_newrelic_name}"
    IDP_NGINX_NEWRELIC_APP_NAME       = "${var.idp_nginx_newrelic_app_name}"
    IDP_DOCKER_NAME                   = "${var.idp_docker_name}"
    DEPLOY_ENV                        = "${var.deploy_env}"
    NEWRELIC_LICENSE                  = "${var.newrelic_license}"
    NGINX_SERVER_NAME                 = "${lookup(var.nginx_server_names, lower(var.deploy_env))}"
    NGINX_STATUS_ACCESS               = "${lookup(var.nginx_status_access, lower(var.deploy_env))}"
    NGINX_SET_REAL_IP_FROM            = "${lookup(var.nginx_set_real_ip_from, lower(var.deploy_env))}"
    CORECLR_PROFILER                  = "${var.coreclr_profiler}"
    CORECLR_NEWRELIC_HOME             = "${var.coreclr_newrelic_home}"
    CORECLR_PROFILER_PATH             = "${var.coreclr_profiler_path}"    
  }
}

resource "aws_ecs_task_definition" "identity-taskdef" {
  family                = "identity"
  container_definitions = "${data.template_file.identity-containerdefs-tpl.rendered}"
  network_mode = "bridge"

  volume {
    name = "secrets"
  }
}

resource "aws_ecs_service" "identity" {
  launch_type     = "EC2"
  name            = "${var.service_name}"
  cluster         = "${aws_ecs_cluster.identitycs.id}"
  task_definition = "${aws_ecs_task_definition.identity-taskdef.arn}"
  desired_count   = "${var.identity_task_count}"
  deployment_maximum_percent = 200
  deployment_minimum_healthy_percent = 0
  iam_role = "${var.iam_role}"

  load_balancer {
    elb_name       = "${aws_elb.elb4identity.name}"
    container_name = "${var.idp_nginx_name}"
    container_port = 443
  }
}

# load balancer
resource "aws_elb" "elb4identity" {
  name = "${var.elb_name}"
  internal           = true
  
  listener {
    instance_port     = 443
    instance_protocol = "tcp"
    lb_port           = 443
    lb_protocol       = "tcp"
  }

  health_check {
    healthy_threshold   = 3
    unhealthy_threshold = 3
    timeout             = 30
    target              = "TCP:443"
    interval            = 60
  }

  cross_zone_load_balancing   = true
  idle_timeout                = 400
  connection_draining         = true
  connection_draining_timeout = 400

  subnets         = ["${var.subnet_elb_1}", "${var.subnet_elb_2}"]
  security_groups = "${var.elb_security_group}"
  //security_groups = ["${aws_security_group.identitycs_elb_sg.id}", "${var.elb_security_group}"]

  tags {
    Name = "elb4identity",
    LOB = "${lookup(var.Tag_App, "LOB")}",
    AppOwner = "${lookup(var.Tag_App, "AppOwner")}",
    CreatedBy = "${lookup(var.Tag_App, "CreatedBy")}",
    CreationMethod = "${lookup(var.Tag_App, "CreationMethod")}"
  }
}

/*
resource "aws_security_group" "identitycs_elb_sg" {
  name        = "${var.idp_elb_security_group}"
  description = "Identity Provider - ELB Security Group"
  vpc_id      = "${var.vpc_id}"

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    security_groups  = ["${var.idp_elb_ingress_security_group}", "${var.security_group}"]
  }

  egress {
    from_port       = 0
    to_port         = 0
    protocol        = "-1"
    cidr_blocks     = ["0.0.0.0/0"]
    //prefix_list_ids = ["pl-12c4e678"]
  }
}
*/

resource "aws_route53_record" "identity_route53record" {
   zone_id = "${var.idp_route53_zone_id}"
   name    = "${var.domain_name}"
   type    = "A"

  alias {
    name                   = "${aws_elb.elb4identity.dns_name}"
    zone_id                = "${aws_elb.elb4identity.zone_id}"
    evaluate_target_health = false
  }
}

resource "aws_proxy_protocol_policy" "identity_proxy_policy" {
  load_balancer  = "${aws_elb.elb4identity.name}"
  instance_ports = ["443"]
}

/*
resource "aws_cognito_user_pool" "pool" {
  name = "us-east-1_B69xsDiqd"
  define_auth_challenge = "${aws_lambda_function.cognito_lambda.arn}"
}
*/