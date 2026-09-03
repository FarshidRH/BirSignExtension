<#
.SYNOPSIS
    Verifies that every packages.config project lists its full NuGet dependency closure.

.DESCRIPTION
    Projects that use packages.config must list EVERY dependency explicitly, including
    transitive ones. NuGet does not resolve those for them the way it does for
    PackageReference projects.

    That makes version bumps dangerous: if a newer package version introduces a new
    dependency, nothing installs it, nothing warns you, and the build still succeeds.
    The failure only appears at run time as a FileNotFoundException -- and it can
    surface as a completely unrelated-looking error.

    This happened with Microsoft.IdentityModel.Tokens 8.22.0, which added a dependency
    on Microsoft.Bcl.Cryptography. The missing assembly made JWT signature validation
    fail with "IDX10511: Signature validation failed", pointing at the wrong problem
    entirely.

    This script reads the .nuspec of every installed package, picks the dependency
    group matching the project's target framework, and reports anything that is not
    in packages.config.

.PARAMETER Path
    Repository root to scan. Defaults to the parent of the folder holding this script.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\Test-PackageDependencies.ps1

.OUTPUTS
    Exit code 0 when every closure is complete, 1 when something is missing.
#>
[CmdletBinding()]
param(
    [string] $Path
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if (-not $Path) { $Path = Split-Path -Parent $PSScriptRoot }
$repoRoot = (Resolve-Path $Path).Path
$packagesRoot = Join-Path $repoRoot 'packages'

Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue

# Frameworks a net4x project can consume, best match first. Both the long form used
# in .nuspec dependency groups and the short form are accepted.
$compatibleFrameworks = @(
    '.NETFramework4.8', 'net48'
    '.NETFramework4.7.2', 'net472'
    '.NETFramework4.7.1', 'net471'
    '.NETFramework4.7', 'net47'
    '.NETFramework4.6.2', 'net462'
    '.NETFramework4.6.1', 'net461'
    '.NETFramework4.6', 'net46'
    '.NETFramework4.5.2', 'net452'
    '.NETFramework4.5', 'net45'
    '.NETFramework4.0', 'net40'
    '.NETStandard2.0', 'netstandard2.0'
    '.NETStandard1.6', 'netstandard1.6'
    '.NETStandard1.3', 'netstandard1.3'
    '.NETStandard1.1', 'netstandard1.1'
    '.NETStandard1.0', 'netstandard1.0'
    ''
)

function Get-NuspecXml {
    param([string] $PackageId, [string] $Version)

    $folder = Join-Path $packagesRoot ("{0}.{1}" -f $PackageId, $Version)
    if (-not (Test-Path $folder)) { return $null }

    # Prefer a loose .nuspec, otherwise read it out of the .nupkg.
    $loose = Get-ChildItem -Path $folder -Filter '*.nuspec' -File -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($loose) { return (Get-Content $loose.FullName -Raw) }

    $nupkg = Get-ChildItem -Path $folder -Filter '*.nupkg' -File -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if (-not $nupkg) { return $null }

    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' -and $_.FullName -notmatch '/' } |
            Select-Object -First 1
        if (-not $entry) { return $null }
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
    }
    finally { $zip.Dispose() }
}

function Get-DependenciesForFramework {
    param([string] $NuspecXml)

    # Drop the default namespace so plain XPath works across nuspec schema versions.
    $clean = [regex]::Replace($NuspecXml, '\sxmlns="[^"]+"', '', 1)
    try { $doc = [xml] $clean } catch { return @() }

    $depsNode = $doc.SelectSingleNode('/package/metadata/dependencies')
    if (-not $depsNode) { return @() }

    $groups = $depsNode.SelectNodes('group')
    if (-not $groups -or $groups.Count -eq 0) {
        return @($depsNode.SelectNodes('dependency') | ForEach-Object { $_.id })
    }

    foreach ($wanted in $compatibleFrameworks) {
        foreach ($group in $groups) {
            $tfm = ''
            if ($group.HasAttribute('targetFramework')) { $tfm = $group.GetAttribute('targetFramework') }
            if ($tfm -eq $wanted) {
                return @($group.SelectNodes('dependency') | ForEach-Object { $_.id })
            }
        }
    }
    return @()
}

$configs = Get-ChildItem -Path $repoRoot -Filter 'packages.config' -Recurse -File |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|packages)\\' } |
    Sort-Object FullName

if (-not $configs) {
    Write-Host 'No packages.config projects found - nothing to check.'
    exit 0
}

$failed = $false

foreach ($config in $configs) {
    $relative = $config.FullName.Substring($repoRoot.Length).TrimStart('\')
    $installed = @{}
    ([xml](Get-Content $config.FullName -Raw)).packages.package | ForEach-Object {
        $installed[$_.id] = $_.version
    }

    Write-Host ''
    Write-Host ("{0}  ({1} packages)" -f $relative, $installed.Count)

    $missing = @{}
    $unreadable = @()

    foreach ($id in ($installed.Keys | Sort-Object)) {
        $nuspec = Get-NuspecXml -PackageId $id -Version $installed[$id]
        if (-not $nuspec) {
            $unreadable += ("{0} {1}" -f $id, $installed[$id])
            continue
        }
        foreach ($dep in (Get-DependenciesForFramework -NuspecXml $nuspec)) {
            if ($dep -and -not $installed.ContainsKey($dep)) {
                if (-not $missing.ContainsKey($dep)) { $missing[$dep] = @() }
                $missing[$dep] += ("{0} {1}" -f $id, $installed[$id])
            }
        }
    }

    if ($unreadable.Count -gt 0) {
        Write-Warning ("  Could not read the .nuspec for: {0}" -f ($unreadable -join ', '))
        Write-Warning '  Run a NuGet restore first, otherwise this check is incomplete.'
    }

    if ($missing.Count -eq 0) {
        Write-Host '  OK - dependency closure is complete' -ForegroundColor Green
    }
    else {
        $failed = $true
        Write-Host '  MISSING from packages.config:' -ForegroundColor Red
        foreach ($dep in ($missing.Keys | Sort-Object)) {
            Write-Host ("    {0}" -f $dep) -ForegroundColor Red
            foreach ($req in ($missing[$dep] | Sort-Object -Unique)) {
                Write-Host ("        required by {0}" -f $req)
            }
        }
    }
}

Write-Host ''
if ($failed) {
    Write-Host 'FAILED: add the packages listed above to packages.config, then add a' -ForegroundColor Red
    Write-Host '        matching <Reference> with HintPath in the .csproj and a binding' -ForegroundColor Red
    Write-Host '        redirect in app.config / Web.config, and restore again.' -ForegroundColor Red
    exit 1
}

Write-Host 'All packages.config projects list their full dependency closure.' -ForegroundColor Green
exit 0
