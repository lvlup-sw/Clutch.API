locals {
  default_tags = {
    Application = var.application_name
    Environment = var.environment
  }
}