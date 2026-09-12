# Stardust Java Build & Deployment Script
# Builds the Java sample app (Stardust.Java) into a runnable JAR and copies it
# to RootFS/bin so the Stardust shell can launch it through IKVM.
#
# Launches like any other app:
#   run StardustJava          (or: run StardustJava.jar  /  run /bin/StardustJava.jar)

$rootFsBin = Join-Path $PSScriptRoot "RootFS/bin"
$javaDir   = Join-Path $PSScriptRoot "Stardust.Java"
$mainClass = "ovh.finite.stardust.java.StardustJava"
$jarName   = "StardustJava.jar"

if (-not (Test-Path $rootFsBin)) {
    New-Item -ItemType Directory -Path $rootFsBin -Force
}

Write-Host "Building Java app into ${jarName}..." -ForegroundColor Cyan

$buildDir = Join-Path $env:TEMP "stardust-java-build"
if (Test-Path $buildDir) { Remove-Item -Recurse -Force $buildDir }
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null

$jarPath = Join-Path $buildDir $jarName

$mvn = Get-Command mvn -ErrorAction SilentlyContinue
if ($mvn) {
    Write-Host "Using Maven..." -ForegroundColor Yellow
    Push-Location $javaDir
    try {
        mvn -q package
        if ($LASTEXITCODE -ne 0) { throw "Maven build failed." }
        Copy-Item -Path (Join-Path $javaDir "target/$jarName") -Destination $jarPath -Force
    } finally {
        Pop-Location
    }
} else {
    $javac = Get-Command javac -ErrorAction SilentlyContinue
    $jar   = Get-Command jar   -ErrorAction SilentlyContinue
    if (-not ($javac -and $jar)) {
        Write-Error "Neither Maven (mvn) nor a JDK (javac/jar) is available. Install one to build the Java sample."
        exit 1
    }
    Write-Host "Using javac/jar directly..." -ForegroundColor Yellow
    $classesDir = Join-Path $buildDir "classes"
    New-Item -ItemType Directory -Path $classesDir -Force | Out-Null
    $sources = Get-ChildItem -Path (Join-Path $javaDir "src/main/java") -Filter "*.java" -Recurse
    & javac --release 8 -d $classesDir $sources.FullName
    if ($LASTEXITCODE -ne 0) { throw "javac failed." }
    & jar cfe $jarPath $mainClass -C $classesDir .
    if ($LASTEXITCODE -ne 0) { throw "jar failed." }
}

Copy-Item -Path $jarPath -Destination (Join-Path $rootFsBin $jarName) -Force
Write-Host "Deployed $jarName to RootFS/bin." -ForegroundColor Green

Remove-Item -Recurse -Force $buildDir