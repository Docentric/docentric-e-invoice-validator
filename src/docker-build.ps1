#!/usr/bin/env pwsh

docker buildx bake e-invoice-validator

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Docker build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Docker image built successfully!"
