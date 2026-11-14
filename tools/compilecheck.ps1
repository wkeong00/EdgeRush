# Offline managed-code compilation using the Unity version pinned by this project.
# This does not launch Unity, run tests, resolve packages, or build Android/real ad SDKs.
[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [string]$UnityDataPath,
    [switch]$Editor,
    [switch]$Development
)
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) { $ProjectRoot = Join-Path $PSScriptRoot '..' }

$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$versionFile = Join-Path $ProjectRoot 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $versionFile)) {
    throw "Not an EdgeRush Unity project: $ProjectRoot"
}
$versionLine = Get-Content -LiteralPath $versionFile | Where-Object { $_ -match '^m_EditorVersion:\s*(\S+)' } | Select-Object -First 1
if (-not $versionLine -or $versionLine -notmatch '^m_EditorVersion:\s*(\S+)') {
    throw "Cannot read the pinned Unity editor version from $versionFile"
}
$editorVersion = $Matches[1]
if ([string]::IsNullOrWhiteSpace($UnityDataPath)) {
    $UnityDataPath = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$editorVersion\Editor\Data"
}
if (-not (Test-Path -LiteralPath $UnityDataPath)) {
    throw "Unity $editorVersion data directory not found: $UnityDataPath. Supply -UnityDataPath for another installation location."
}
$UnityDataPath = (Resolve-Path -LiteralPath $UnityDataPath).Path
$dotnetPath = Join-Path $UnityDataPath 'NetCoreRuntime\dotnet.exe'
$sdkRoot = Join-Path $UnityDataPath 'DotNetSdk\sdk'
$sdk = Get-ChildItem -LiteralPath $sdkRoot -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+\.\d+$' } |
    Sort-Object { [version]$_.Name } -Descending |
    Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'Roslyn\bincore\csc.dll') } |
    Select-Object -First 1
if ($null -eq $sdk) { throw "Unity's bundled Roslyn compiler was not found under $sdkRoot" }
$compilerPath = Join-Path $sdk.FullName 'Roslyn\bincore\csc.dll'
$managedRoot = Join-Path $UnityDataPath 'Managed\UnityEngine'
$netstandardPath = Join-Path $UnityDataPath 'NetStandard\ref\2.1.0\netstandard.dll'
$uguiRoot = Join-Path $UnityDataPath 'Resources\PackageManager\BuiltInPackages\com.unity.ugui\Runtime'
foreach ($required in @($dotnetPath, $compilerPath, $managedRoot, $netstandardPath, $uguiRoot)) {
    if (-not (Test-Path -LiteralPath $required)) { throw "Required Unity dependency missing: $required" }
}

# Separate response files and assemblies keep this tool independent of ../_tools/.cc.
$outputRoot = Join-Path $ProjectRoot 'Temp\CompileCheck'
$mode = if ($Editor) { 'Editor' } elseif ($Development) { 'Development' } else { 'Runtime' }
$outputDirectory = Join-Path $outputRoot $mode
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$references = @($netstandardPath)
$references += Get-ChildItem -LiteralPath $managedRoot -Filter 'UnityEngine.*Module.dll' |
    Sort-Object Name | ForEach-Object { $_.FullName }
$sources = @()
if ($Editor) {
    # The editor verification helpers reference Development-only Mock types.
    $runtimeAssembly = Join-Path $outputRoot 'Development\UnityEngine.UI.dll'
    if (-not (Test-Path -LiteralPath $runtimeAssembly)) {
        throw 'Compile the runtime with -Development first, then rerun with -Editor.'
    }
    $references += $runtimeAssembly
    $references += Get-ChildItem -LiteralPath $managedRoot -Filter 'UnityEditor*.dll' |
        Sort-Object Name | ForEach-Object { $_.FullName }
    $sources += Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Assets\Editor') -Recurse -Filter '*.cs' |
        Sort-Object FullName | ForEach-Object { $_.FullName }
    $assemblyName = 'EdgeRush.Editor.dll'
} else {
    # As in the original check, these two unused uGUI files require extra package dependencies.
    $skip = @('InputField.cs', 'DefaultControls.cs')
    $sources += Get-ChildItem -LiteralPath (Join-Path $uguiRoot 'InternalBridge') -Recurse -Filter '*.cs' |
        Sort-Object FullName | ForEach-Object { $_.FullName }
    $sources += Get-ChildItem -LiteralPath (Join-Path $uguiRoot 'UGUI') -Recurse -Filter '*.cs' |
        Where-Object { $skip -notcontains $_.Name } | Sort-Object FullName | ForEach-Object { $_.FullName }
    $sources += Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Assets\Scripts') -Recurse -Filter '*.cs' |
        Sort-Object FullName | ForEach-Object { $_.FullName }
    # uGUI's InternalsVisibleTo grant requires this assembly name.
    $assemblyName = 'UnityEngine.UI.dll'
}

$defines = @(
    'UNITY_5_3_OR_NEWER','UNITY_2017_1_OR_NEWER','UNITY_2018_1_OR_NEWER','UNITY_2019_1_OR_NEWER',
    'UNITY_2020_1_OR_NEWER','UNITY_2021_1_OR_NEWER','UNITY_2022_1_OR_NEWER','UNITY_2023_1_OR_NEWER',
    'UNITY_6000_0_OR_NEWER','UNITY_6000_6_OR_NEWER',
    'UNITY_STANDALONE','UNITY_STANDALONE_WIN','PLATFORM_STANDALONE','PLATFORM_STANDALONE_WIN',
    'ENABLE_LEGACY_INPUT_MANAGER','ENABLE_MONO','ENABLE_UGUI','PACKAGE_PHYSICS2D','PACKAGE_PHYSICS',
    'UNITY_UGUI','UNITY_ASSERTIONS','CSHARP_7_3_OR_NEWER','NET_STANDARD_2_0','NETSTANDARD2_1','NET_4_6'
)
if ($Development -or $Editor) { $defines += 'DEVELOPMENT_BUILD' }
$assemblyPath = Join-Path $outputDirectory $assemblyName
$arguments = @('/nologo', '/target:library', "/out:`"$assemblyPath`"", '/nostdlib+', "/define:$($defines -join ';')")
foreach ($reference in $references) { $arguments += "/r:`"$reference`"" }
foreach ($source in $sources) { $arguments += "`"$($source -replace '\\','/')`"" }
$responsePath = Join-Path $outputDirectory 'sources.rsp'
Set-Content -LiteralPath $responsePath -Value $arguments -Encoding UTF8
Write-Host "Checking $mode against Unity $editorVersion ($($sources.Count) source files)."
& $dotnetPath exec $compilerPath "@$responsePath"
$compilerExitCode = $LASTEXITCODE
if ($compilerExitCode -eq 0) { Write-Host "PASS: $assemblyPath" }
exit $compilerExitCode
