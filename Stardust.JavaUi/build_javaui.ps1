# Builds the Java UI binding:
#   1. builds Stardust.JavaApi.dll (the .NET adapters)
#   2. compiles the Java facade (impl + cli stubs) -> Stardust.JavaUi\build\Stardust.UiJava.jar
#   3. dotnet build Stardust.JavaUi.vbproj compiles that jar with ikvmc via the
#      IkvmReference build item (References metadata provides Stardust.JavaApi +
#      Stardust.Display). At compile time the facade references cli.* stub types (ikvmc's
#      name for .NET types); these are rebound to the real managed types and do not ship.
#   4. compiles the api jar (throwing stubs) for app developers -> libs/Stardust.JavaUi/stardust-ui-api.jar
#   5. builds the sample demo app from the api jar -> RootFS/bin/StardustDemo.jar
#
# Requires: dotnet, javac/jar (JDK).

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $root

$apiProj = Join-Path $repoRoot 'Stardust.JavaApi\Stardust.JavaApi.vbproj'
$uiProj = Join-Path $repoRoot 'Stardust.JavaUi\Stardust.JavaUi.vbproj'
$outDir = Join-Path $repoRoot 'libs\Stardust.JavaUi'
$buildDir = Join-Path $env:TEMP 'stardust-javaui-build'

# --- 1. build the .NET adapter assembly ---
Write-Host '[build_javaui] building Stardust.JavaApi' -ForegroundColor Cyan
dotnet build $apiProj -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'Stardust.JavaApi build failed' }

# --- 2. javac facade impl against the clr stubs (classpath only), then jar ONLY the ovh facade ---
# The cli.* stubs exist solely for javac; they must NOT be in the jar, otherwise ikvmc
# compiles them as ordinary classes instead of remapping references to the real .NET types.
$implSrc = Join-Path $root 'src\impl'
$stubSrc = Join-Path $root 'src\stubs'
$classesStubs = Join-Path $buildDir 'stub-classes'
$classesImpl = Join-Path $buildDir 'impl-classes'
$classesApi = Join-Path $buildDir 'api-classes'
$classesSample = Join-Path $buildDir 'sample-classes'
$facadeJar = Join-Path $root 'build\Stardust.UiJava.jar'
$tmpApiJar = Join-Path $buildDir 'stardust-ui-api.jar'
$sampleJar = Join-Path $buildDir 'StardustDemo.jar'

Remove-Item -Recurse -Force $buildDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $classesImpl | Out-Null
New-Item -ItemType Directory -Force -Path $classesApi | Out-Null
New-Item -ItemType Directory -Force -Path $classesStubs | Out-Null
New-Item -ItemType Directory -Force -Path $classesSample | Out-Null
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $root 'build') | Out-Null

Write-Host '[build_javaui] javac: clr stubs (classpath only)' -ForegroundColor Cyan
javac --release 8 -d $classesStubs `
    (Get-ChildItem -Recurse $stubSrc -Filter *.java | ForEach-Object { $_.FullName })
if ($LASTEXITCODE -ne 0) { throw 'javac (clr stubs) failed' }

Write-Host '[build_javaui] javac: facade impl (ovh only) -> Stardust.UiJava.jar' -ForegroundColor Cyan
javac --release 8 -cp $classesStubs -d $classesImpl `
    (Get-ChildItem -Recurse $implSrc -Filter *.java | ForEach-Object { $_.FullName })
if ($LASTEXITCODE -ne 0) { throw 'javac (impl) failed' }

Push-Location $classesImpl
jar cf $facadeJar ovh
Pop-Location

# --- 3. ikvmc via MSBuild (IkvmReference) ---
Write-Host '[build_javaui] dotnet build Stardust.JavaUi (ikvmc) -> Stardust.UiJava.dll' -ForegroundColor Cyan
dotnet build $uiProj -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'Stardust.JavaUi (ikvmc) build failed' }

# ikvmc output lives in the cache, not in bin/ (the project assembly is a marker).
$cacheDir = Join-Path $env:TEMP 'ikvm\cache\1'
$uiDll = Get-ChildItem $cacheDir -Recurse -Filter 'Stardust.UiJava.dll' |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $uiDll) { throw "missing Stardust.UiJava.dll in ikvm cache" }
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
Copy-Item $uiDll.FullName (Join-Path $outDir 'Stardust.UiJava.dll') -Force

# Deploy the real UiJava + JavaApi assemblies next to the host so the kernel can
# Assembly.Load them at runtime (the JavaUi project's own bin only holds the marker).
$hostOut = Join-Path $repoRoot 'Stardust\bin\Debug\net10.0'
$javaApiDll = Join-Path $repoRoot 'Stardust.JavaApi\bin\Debug\net10.0\Stardust.JavaApi.dll'
if (Test-Path $hostOut) {
    Copy-Item $uiDll.FullName (Join-Path $hostOut 'Stardust.UiJava.dll') -Force
    if (Test-Path $javaApiDll) { Copy-Item $javaApiDll (Join-Path $hostOut 'Stardust.JavaApi.dll') -Force }
}

# --- 4. api jar (throwing stubs + cli stubs so app devs can compile standalone) ---
Write-Host '[build_javaui] javac: api stubs -> stardust-ui-api.jar' -ForegroundColor Cyan
$apiSrc = Join-Path $root 'src\api'
javac --release 8 -d $classesApi -sourcepath "$apiSrc;$stubSrc" `
    (Get-ChildItem -Recurse $apiSrc -Filter *.java | ForEach-Object { $_.FullName }) `
    (Get-ChildItem -Recurse $stubSrc -Filter *.java | ForEach-Object { $_.FullName })
if ($LASTEXITCODE -ne 0) { throw 'javac (api) failed' }

Push-Location $classesApi
jar cf $tmpApiJar ovh cli
Pop-Location
Copy-Item $tmpApiJar (Join-Path $outDir 'stardust-ui-api.jar') -Force

# --- 5. sample demo app ---
$sampleSrc = Join-Path $root 'sample'
Write-Host '[build_javaui] javac: sample app' -ForegroundColor Cyan
javac --release 8 -cp $tmpApiJar -d $classesSample `
    (Get-ChildItem -Recurse $sampleSrc -Filter *.java | ForEach-Object { $_.FullName })
if ($LASTEXITCODE -ne 0) { throw 'javac (sample) failed' }

Push-Location $classesSample
jar cfe $sampleJar ovh.finite.stardust.demo.UiDemo ovh
Pop-Location

# deploy sample + bundle manifest
$rootFSEntry = Join-Path $repoRoot 'RootFS\bin\StardustDemo.jar'
Copy-Item $sampleJar $rootFSEntry -Force
New-Item -ItemType Directory -Force -Path (Join-Path $repoRoot 'RootFS\bin\StardustDemo.app') | Out-Null
@'
{
  "name": "StardustDemo",
  "displayName": "Demo",
  "launcher": "/bin/StardustDemo.jar"
}
'@ | Set-Content -Path (Join-Path $repoRoot 'RootFS\bin\StardustDemo.app\manifest.json') -Encoding utf8

Write-Host ''
Write-Host 'Done.' -ForegroundColor Green
Write-Host "  Stardust.UiJava.dll    -> $(Join-Path $outDir 'Stardust.UiJava.dll')"
Write-Host "  stardust-ui-api.jar    -> $(Join-Path $outDir 'stardust-ui-api.jar')"
Write-Host "  StardustDemo.jar        -> $rootFSEntry"