resource "aws_cognito_user_pool" "user_pool" {
  name = "${var.cognito_user_pool_name}"
  admin_create_user_config {
    unused_account_validity_days= 7
    allow_admin_create_user_only= false
  }
  password_policy {
    require_lowercase = true
    require_symbols = true
    require_numbers = true
    minimum_length = 8
    require_uppercase = true
  }
  lambda_config {
    define_auth_challenge = "${aws_lambda_function.cognito_defAuthChallenge_lambda.arn}"
    pre_token_generation = "${aws_lambda_function.cognito_preTokenGen_lambda.arn}"
  }
  tags {
    LOB = "Tech Dev-BR General"
  }
  schema = [{
    attribute_data_type = "String"
    developer_only_attribute = false
    mutable = false
    name = "email"
    required = true
    string_attribute_constraints {
      min_length = 0
      max_length = 2048
    }
  },
    {
      attribute_data_type = "String"
      developer_only_attribute = false
      mutable = true
      name = "custom:SourceIP"
      required = false
    }]
  lifecycle {
    prevent_destroy = true
    #  create_before_destroy = true # it is dangerous to uncomment it, as AWS Cognito is a stateful resource !!!
    ignore_changes = [ "schema" ]
  }
  # device_configuration =
  # email_configuration =
}

resource "aws_cognito_resource_server" "user_resource_server" {
  name = "POS pool resource server"
  identifier = "${var.cognito_user_resource_server_identifier}"
  user_pool_id = "${aws_cognito_user_pool.user_pool.id}"
  # user_pool_id = "${var.cognito_user_pool_id}"
  scope = [{
    scope_name = "${var.cognito_user_resource_server_sso_signin_scope}"
    scope_description="SSO Sign In"
  }]
  #lifecycle {
  #  create_before_destroy = true
  #}
}

resource "aws_cognito_user_pool_domain" "user_domain" {
  domain = "${var.cognito_user_user_pool_domain}"
  user_pool_id = "${aws_cognito_user_pool.user_pool.id}"
  # user_pool_id = "${var.cognito_user_pool_id}"
  #lifecycle {
  #  create_before_destroy = true
  #}
}

resource "aws_cognito_user_pool_client" "user_pos_client" {
  name = "POS"
  user_pool_id = "${aws_cognito_user_pool.user_pool.id}"
  # user_pool_id = "${var.cognito_user_pool_id}"
  allowed_oauth_flows = [ "client_credentials" ]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes = [ "${var.cognito_user_resource_server_identifier}/${var.cognito_user_resource_server_sso_signin_scope}" ]
  generate_secret = true
  read_attributes = ["address", "birthdate", "email", "email_verified", "family_name", "gender", "given_name", "locale", "middle_name", "name", "nickname", "phone_number", "phone_number_verified", "picture", "preferred_username", "profile", "updated_at", "website", "zoneinfo" ]
  write_attributes = [ "address", "birthdate", "email", "family_name", "gender", "given_name", "locale", "middle_name", "name", "nickname", "phone_number", "picture", "preferred_username", "profile", "updated_at", "website", "zoneinfo" ]
  lifecycle {
    ignore_changes = [ "generate_secret"]
    #create_before_destroy = true
  }
  depends_on = ["aws_cognito_resource_server.user_resource_server"]
  supported_identity_providers = [ "COGNITO"]
}

resource "aws_cognito_user_pool_client" "user_job_scheduler_client" {
  name = "user-job-scheduler"
  user_pool_id = "${aws_cognito_user_pool.user_pool.id}"
  # user_pool_id = "${var.cognito_user_pool_id}"
  allowed_oauth_flows = [ "client_credentials" ]
  allowed_oauth_flows_user_pool_client = true
  allowed_oauth_scopes = [ "${var.cognito_user_resource_server_identifier}/${var.cognito_user_resource_server_sso_signin_scope}" ]
  generate_secret = true
  lifecycle {
    ignore_changes = [ "generate_secret"]
    #create_before_destroy = true
  }
  depends_on = ["aws_cognito_resource_server.user_resource_server"]
  supported_identity_providers = [ "COGNITO"]
}