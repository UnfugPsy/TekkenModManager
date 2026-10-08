<#
.SYNOPSIS
Builds the two release zips for the version in Directory.Build.props and writes the release notes.

.DESCRIPTION
Runs the tests, publishes a self-contained and a framework-dependent single-file build,
zips each with ModManager.dll.config, and writes SHA256SUMS.txt and notes.md (the CHANGELOG.md
section for this version) to artifacts/release.

.EXAMPLE
pwsh scripts/release.ps1
#>
[CmdletBinding()]
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

[xml]$props = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props')
$prefix = $props.SelectSingleNode('//VersionPrefix').InnerText
$suffixNode = $props.SelectSingleNode('//VersionSuffix')
$version = if ($suffixNode -and $suffixNode.InnerText) { "$prefix-$($suffixNode.InnerText)" } else { $prefix }
Write-Host "[INFO] version $version"

$changelog = Get-Content -LiteralPath (Join-Path $root 'CHANGELOG.md') -Raw
$pattern = '(?ms)^## \[' + [regex]::Escape($version) + '\][^\r\n]*\r?\n(?<body>.*?)(?=^## \[|\z)'
$match = [regex]::Match($changelog, $pattern)
if (-not $match.Success -or [string]::IsNullOrWhiteSpace($match.Groups['body'].Value)) {
    throw "CHANGELOG.md has no section for $version"
}
$notes = $match.Groups['body'].Value.Trim()

if (-not $SkipTests) {
    dotnet test ModManager.sln -c Release --nologo -v q -clp:ErrorsOnly
    if ($LASTEXITCODE -ne 0) {
        throw 'tests failed'
    }
}

$artifacts = Join-Path $root 'artifacts'
$publish = Join-Path $artifacts 'publish'
$release = Join-Path $artifacts 'release'
foreach ($dir in $publish, $release) {
    if (Test-Path -LiteralPath $dir) {
        Remove-Item -LiteralPath $dir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $dir | Out-Null
}

$project = 'src/ModManager/ModManager.csproj'
$builds = @(
    @{ Name = 'selfcontained'; Args = @('--self-contained', 'true', '-p:IncludeNativeLibrariesForSelfExtract=true') },
    @{ Name = 'framework-dependent'; Args = @('--self-contained', 'false') }
)

foreach ($build in $builds) {
    $out = Join-Path $publish $build.Name
    dotnet publish $project -c Release -r win-x64 -p:PublishSingleFile=true @($build.Args) -o $out --nologo -v q -clp:ErrorsOnly
    if ($LASTEXITCODE -ne 0) {
        throw "publish failed for $($build.Name)"
    }

    $zip = Join-Path $release "TekkenModManager-$version-win-x64-$($build.Name).zip"
    $files = 'ModManager.exe', 'ModManager.dll.config' | ForEach-Object { Join-Path $out $_ }
    foreach ($file in $files) {
        if (-not (Test-Path -LiteralPath $file)) {
            throw "missing $file in the $($build.Name) publish output"
        }
    }
    Compress-Archive -LiteralPath $files -DestinationPath $zip -CompressionLevel Optimal
}

$sums = Get-ChildItem -LiteralPath $release -Filter '*.zip' | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLower(), $_.Name
}
Set-Content -LiteralPath (Join-Path $release 'SHA256SUMS.txt') -Value $sums -Encoding ascii
Set-Content -LiteralPath (Join-Path $release 'notes.md') -Value $notes -Encoding utf8

Get-ChildItem -LiteralPath $release | ForEach-Object {
    Write-Host ('[OK] {0}  {1:N1} MB' -f $_.Name, ($_.Length / 1MB))
}
