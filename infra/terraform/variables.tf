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

variable "container_image_tag" {
  type        = string
  description = "Container image tag deployed to Azure Container Apps."
  default     = "latest"
}

variable "static_web_app_location" {
  type        = string
  description = "Azure region for Static Web Apps. This can differ from the main resource region because Static Web Apps are only available in selected regions."
  default     = "eastasia"
}

variable "frontend_url_override" {
  type        = string
  description = "Optional custom frontend URL used for CORS and email links. Leave null to use the Azure Static Web Apps generated hostname."
  default     = null
  nullable    = true
}

variable "admin_seed_email" {
  type        = string
  description = "Initial admin account email for the auth service."
  default     = "admin@teacupboutique.com.au"
}

variable "mailgun_domain" {
  type        = string
  description = "Mailgun domain used by the notifications service."
  default     = "mg.teacupboutique.lbirchen.com"
}

variable "mailgun_from_address" {
  type        = string
  description = "From address used by notification emails."
  default     = "noreply@mg.teacupboutique.lbirchen.com"
}

variable "mailgun_from_name" {
  type        = string
  description = "From display name used by notification emails."
  default     = "Teacup Boutique"
}

variable "github_owner" {
  type        = string
  description = "GitHub repository owner allowed to authenticate to Azure using OIDC."
  default     = "lbirchenough"
}

variable "github_repo" {
  type        = string
  description = "GitHub repository name allowed to authenticate to Azure using OIDC."
  default     = "TeacupBoutique"
}

variable "github_branch" {
  type        = string
  description = "GitHub branch allowed to authenticate to Azure using OIDC."
  default     = "main"
}
