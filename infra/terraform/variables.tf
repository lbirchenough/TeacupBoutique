variable "subscription_id" {
  type        = string
  description = "Azure subscription ID to deploy into."
}

variable "location" {
  type        = string
  description = "Azure region for all resources."
  default     = "australiaeast"
}

variable "project" {
  type        = string
  description = "Short project prefix used in resource names."
  default     = "tcb"
}
