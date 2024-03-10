########################################################################
#       Remote State
########################################################################

terraform {
  backend "s3" {
    key     = "StepNet/terraform.tfstate"
    encrypt = "true"
  }
}

########################################################################
#       Common Data
########################################################################

data "aws_caller_identity" "current" {}

data "aws_iam_account_alias" "current" {}
