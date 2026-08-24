# EcoPack local dev — API + Web 동시 실행
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host 'Starting ecopack.Api on http://localhost:5260 ...' -ForegroundColor Green
Start-Process powershell -ArgumentList '-NoExit', '-Command', "cd '$root\ecopack.Api'; dotnet run --launch-profile ecopack.Api"

Start-Sleep -Seconds 2

Write-Host 'Starting ecopack.Web on http://localhost:5173 ...' -ForegroundColor Green
Start-Process powershell -ArgumentList '-NoExit', '-Command', "cd '$root\ecopack.Web'; npm run dev"

Write-Host ''
Write-Host 'Open http://localhost:5173 in your browser.' -ForegroundColor Cyan
Write-Host 'Close the two PowerShell windows to stop the servers.' -ForegroundColor Yellow
