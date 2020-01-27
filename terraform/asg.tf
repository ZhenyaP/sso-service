#needs launch template and also autoscaling group

resource "aws_launch_template" "identitycs" {
  name_prefix = "identity-lt-new-"
  image_id = "${lookup(var.amis, lower(var.deploy_env))}"
  instance_type = "${var.instance_type}"
  key_name = "${var.key_name}"
  vpc_security_group_ids = "${var.ec2_security_group}"
  disable_api_termination = true
  user_data   = "${base64encode("#!/bin/bash\necho 'ECS_CLUSTER=${aws_ecs_cluster.identitycs.id}' > /etc/ecs/ecs.config\nstart ecs")}"
  iam_instance_profile {
    name = "${var.iam_instance_profile}"
  }

  monitoring {
    enabled = true
  }

  lifecycle {
    create_before_destroy = true
  }
}

/*
resource "aws_security_group" "identitycs_ec2_sg" {
  name        = "${var.idp_ec2_security_group}"
  description = "Identity Provider - EC2 Security Group"
  vpc_id      = "${var.vpc_id}"

  ingress {
    # TLS (change to whatever ports you need)
    from_port   = 443
    to_port     = 443
    protocol    = "-1"
    # Please restrict your ingress to only necessary IPs and ports.
    # Opening to 0.0.0.0/0 can lead to security vulnerabilities.
    security_groups  = ["${aws_security_group.identitycs_elb_sg.id}"]
  }

  egress {
    from_port       = 0
    to_port         = 0
    protocol        = "-1"
    cidr_blocks     = ["0.0.0.0/0"]
    #prefix_list_ids = ["pl-12c4e678"]
  }
}
*/

resource "aws_launch_configuration" "identitycs" {

  name_prefix = "identity-"
  image_id = "${lookup(var.amis, lower(var.deploy_env))}"
  instance_type = "${var.instance_type}"
  key_name = "${var.key_name}"
  security_groups = "${var.ec2_security_group}"
  user_data   = "#!/bin/bash\necho 'ECS_CLUSTER=${aws_ecs_cluster.identitycs.id}' > /etc/ecs/ecs.config\nstart ecs"
  iam_instance_profile = "${var.iam_instance_profile}"

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_autoscaling_group" "identitygroup" {
  name = "${var.autoscaling_group_name}"
  vpc_zone_identifier = ["${var.subnet_asg_1}", "${var.subnet_asg_2}"]
  launch_configuration = "${aws_launch_configuration.identitycs.name}"
/*
  launch_template = {
    id      = "${join("", aws_launch_template.identitycs.*.id)}"
    version = "${aws_launch_template.identitycs.latest_version}"
  }
*/
  health_check_type    = "EC2"
  min_size = "${var.idp_asg_min_size}"
  max_size = "${var.idp_asg_max_size}"
  load_balancers = ["${aws_elb.elb4identity.name}"]

  lifecycle {
    create_before_destroy = true
  }

  tags = [
    {
      key = "Name"
      value = "ECS Autoscaling for identity!"
      propagate_at_launch = true
    },
    {
      key                 = "MainApplication"
      value               = "${lookup(var.Tag_App, "MainApplication")}"
      propagate_at_launch = true
    },
    {
      key                 = "Application"
      value               = "${lookup(var.Tag_App, "Application")}"
      propagate_at_launch = true
    },
    {
      key                 = "AppOwner"
      value               = "${lookup(var.Tag_App, "AppOwner")}"
      propagate_at_launch = true
    },
    {
      key                 = "RequestedBy"
      value               = "${lookup(var.Tag_App, "RequestedBy")}"
      propagate_at_launch = true
    },
    {
      key                 = "CreatedBy"
      value               = "${lookup(var.Tag_App, "CreatedBy")}"
      propagate_at_launch = true
    },
    {
      key                 = "CreationMethod"
      value               = "${lookup(var.Tag_App, "CreationMethod")}"
      propagate_at_launch = true
    },
    {
      key                 = "LOB"
      value               = "${lookup(var.Tag_App, "LOB")}"
      propagate_at_launch = true
    }
  ]
}

resource "aws_launch_configuration" "idptest" {
  name_prefix = "idptest-"
  image_id = "${lookup(var.amis, lower(var.deploy_env))}"
  instance_type = "${var.instance_type}"
  key_name = "${var.key_name}"
  security_groups = "${var.ec2_security_group}"
  iam_instance_profile = "${var.iam_test_instance_profile}"

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_autoscaling_group" "idptestgroup" {
  name = "${var.test_autoscaling_group_name}"
  vpc_zone_identifier = ["${var.subnet_asg_1}", "${var.subnet_asg_2}"]
  launch_configuration = "${aws_launch_configuration.idptest.name}"
  health_check_type    = "EC2"
  min_size = "${var.idp_test_asg_min_size}"
  max_size = "${var.idp_test_asg_max_size}"

  lifecycle {
    create_before_destroy = true
  }
}

#cluster used for container
resource "aws_ecs_cluster" "identitycs" {
  name = "${var.cluster_name}"
}
