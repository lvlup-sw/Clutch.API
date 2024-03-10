terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.8.0"
    }
  }
  required_version = ">= 1.0"
}

provider "aws" {
  region = "us-east-1"
  default_tags {
    tags = local.default_tags
  }
}

provider "aws" {
  alias  = "primary"
  region = var.aws_regions[0]
  default_tags {
    tags = local.default_tags
  }
}

provider "aws" {
  alias  = "secondary"
  region = var.aws_regions[1]
  default_tags {
    tags = local.default_tags
  }
}
