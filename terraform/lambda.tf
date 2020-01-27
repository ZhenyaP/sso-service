resource "aws_lambda_function" "cognito_defAuthChallenge_lambda" {
  filename         = "cognito_lambda.zip"
  function_name    = "CognitoDefineAuthChallenge"
  role             = "${var.cognito_lambda_role}"
  handler          = "index.handler"
  source_code_hash = "${base64sha256(file("cognito_lambda.zip"))}"
  runtime          = "nodejs8.10"
}

resource "aws_lambda_function" "cognito_preTokenGen_lambda" {
  filename         = "cognito_preTokenGen_lambda.zip"
  function_name    = "CognitoPreTokenGeneration"
  role             = "${var.cognito_lambda_role}"
  handler          = "index.handler"
  source_code_hash = "${base64sha256(file("cognito_preTokenGen_lambda.zip"))}"
  runtime          = "nodejs8.10"
}

resource "aws_lambda_permission" "cognito_defAuthChallenge_Invoke" {
  statement_id  = "IdP_CognitoDefineAuthChallenge_new"
  action        = "lambda:InvokeFunction"
  function_name = "${aws_lambda_function.cognito_defAuthChallenge_lambda.function_name}"
  principal     = "cognito-idp.amazonaws.com"
  source_arn    = "${aws_cognito_user_pool.user_pool.arn}"
}

resource "aws_lambda_permission" "cognito_preTokenGen_Invoke" {
  statement_id  = "IdP_CognitoPreTokenGeneration_new"
  action        = "lambda:InvokeFunction"
  function_name = "${aws_lambda_function.cognito_preTokenGen_lambda.function_name}"
  principal     = "cognito-idp.amazonaws.com"
  source_arn    = "${aws_cognito_user_pool.user_pool.arn}"
}