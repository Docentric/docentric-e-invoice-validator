#!/usr/bin/env pwsh

docker build -t docentric/e-invoice-validator:2025.1.0 -t docentric/e-invoice-validator:latest .

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Docker image built successfully!"
