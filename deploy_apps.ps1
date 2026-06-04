# Stardust App Deployment Script
# This script builds all projects in the Apps/ folder and copies their DLLs to the RootFS/bin directory.

$rootFsBin = Join-Path $PSScriptRoot "RootFS/bin"
$appsDir = Join-Path $PSScriptRoot "Apps"

if (-not (Test-Path $rootFsBin)) {
    New-Item -ItemType Directory -Path $rootFsBin -Force
}

Write-Host "Starting Stardust App Deployment..." -ForegroundColor Cyan

# Find all .csproj files in the Apps folder
$projects = Get-ChildItem -Path $appsDir -Filter "*.csproj" -Recurse

foreach ($project in $projects) {
    Write-Host "Building project: $($project.Name)..." -ForegroundColor Yellow
    
    # Build the project
    dotnet build $project.FullName --configuration Debug --output (Join-Path $project.DirectoryName "bin/deploy") --nologo
    
    if ($LASTEXITCODE -eq 0) {
        $outputDir = Join-Path $project.DirectoryName "bin/deploy"
        $dlls = Get-ChildItem -Path $outputDir -Filter "*.dll"
        
        foreach ($dll in $dlls) {
            Write-Host "  Copying $($dll.Name) to RootFS/bin..." -ForegroundColor Green
            Copy-Item -Path $dll.FullName -Destination $rootFsBin -Force
        }
    } else {
        Write-Error "Failed to build $($project.Name)"
    }
}

Write-Host "Deployment complete!" -ForegroundColor Cyan
