#!/usr/bin/env pwsh

docker build -t docentric/e-invoice-validator:2026.1.0-dev -t docentric/e-invoice-validator:latest-dev .

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Docker build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Docker image built successfully!"
