target "e-invoice-validator" {
  context    = "."
  dockerfile = "Dockerfile"
  tags = [
    "docentric/e-invoice-validator:2026.4.0-dev",
    "docentric/e-invoice-validator:latest-dev"
  ]
}
