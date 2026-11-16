#requires -Version 5.1
<#
Creates a new, password-protected release keystore only after local interactive input.
Never use this for a game whose existing release signing key must be retained.
-ValidateOnly performs read-only preflight checks; it does not create a key or directory.
#>
[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]*\.(keystore|jks|p12)$')]
    [string]$FileName = 'edgerush-release.keystore',
    [ValidatePattern('^[a-z][a-z0-9_-]{0,39}$')]
    [string]$Alias = 'edgerush',
    [ValidateSet(2048, 4096)]
    [int]$KeySize = 4096,
    [string]$UnityDataPath,
    [switch]$ValidateOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-NoReparseAncestor([string]$Path) {
    $cursor = [System.IO.Path]::GetFullPath($Path)
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Refusing a symbolic link or junction in the signing path: $cursor"
            }
            if (-not $item.PSIsContainer) {
                throw "Expected a directory in the signing path: $cursor"
            }
        }
        $parent = [System.IO.Directory]::GetParent($cursor)
        if ($null -eq $parent) { break }
        $cursor = $parent.FullName
    }
}

function Invoke-Keytool([string[]]$Arguments, [string]$PlainPassword) {
    # These arguments contain only public configuration and the environment variable name.
    # All accepted paths end in a filename; reject quotes/newlines/trailing slashes rather
    # than interpolating them into a shell. No shell is started.
    $quoted = foreach ($argument in $Arguments) {
        if ($argument.Contains('"') -or $argument.Contains("`n") -or
            $argument.Contains("`r") -or $argument.EndsWith('\')) {
            throw 'Unsupported character in a keytool argument.'
        }
        '"' + $argument + '"'
    }
    $start = New-Object System.Diagnostics.ProcessStartInfo
    $start.FileName = $script:keytoolPath
    $start.Arguments = $quoted -join ' '
    $start.WorkingDirectory = $script:signingRoot
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.RedirectStandardInput = $true
    # This changes only the child process environment, never the user's or machine's.
    $start.EnvironmentVariables[$script:passwordVariable] = $PlainPassword
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $start
    try {
        if (-not $process.Start()) { throw 'Could not start Unity OpenJDK keytool.' }
        $process.StandardInput.Close()
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(60000)) {
            $process.Kill()
            $process.WaitForExit()
            throw 'Keytool timed out. No final keystore was installed.'
        }
        $output = $stdout.GetAwaiter().GetResult()
        $null = $stderr.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) {
            # Do not print captured child output or environment values on failure.
            throw "Keytool failed with exit code $($process.ExitCode). No final keystore was installed."
        }
        return $output
    }
    finally {
        $start.EnvironmentVariables.Remove($script:passwordVariable)
        $process.Dispose()
    }
}

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($FileName -match '^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)') {
    throw 'A Windows reserved device name cannot be used as the keystore filename.'
}
$versionFile = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $versionFile -PathType Leaf)) {
    throw 'Run this repository copy of the script from its tools directory.'
}
$script:signingRoot = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'UserSettings\Signing'))
$targetPath = [System.IO.Path]::GetFullPath((Join-Path $script:signingRoot $FileName))
$allowedPrefix = $script:signingRoot.TrimEnd('\') + '\'
if (-not $targetPath.StartsWith($allowedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'The keystore must stay inside this project UserSettings\Signing directory.'
}
Assert-NoReparseAncestor $script:signingRoot
if (Test-Path -LiteralPath $targetPath) {
    throw "Refusing to overwrite an existing file: $targetPath"
}

$git = Get-Command git -ErrorAction Stop
& $git.Source -C $projectRoot check-ignore --quiet -- $targetPath
if ($LASTEXITCODE -ne 0) {
    throw 'The output is not ignored by Git. Fix the ignore rules before creating a key.'
}
if ([string]::IsNullOrWhiteSpace($UnityDataPath)) {
    $versionLine = Get-Content -LiteralPath $versionFile | Where-Object { $_ -match '^m_EditorVersion:\s*(\S+)' } | Select-Object -First 1
    if (-not $versionLine -or $versionLine -notmatch '^m_EditorVersion:\s*(\S+)') {
        throw 'Cannot read the pinned Unity Editor version.'
    }
    $UnityDataPath = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$($Matches[1])\Editor\Data"
}
$script:keytoolPath = [System.IO.Path]::GetFullPath((Join-Path $UnityDataPath 'PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe'))
if (-not (Test-Path -LiteralPath $script:keytoolPath -PathType Leaf)) {
    throw 'Unity Android OpenJDK keytool.exe was not found. Supply -UnityDataPath for the pinned Editor.'
}
if ($ValidateOnly) {
    [pscustomobject]@{
        Status = 'Preflight passed; no files or directories created'
        KeystorePath = $targetPath
        Alias = $Alias
        Format = 'PKCS12'
        KeyAlgorithm = "RSA $KeySize"
        ValidityDays = 10000
        GitIgnored = $true
        KeytoolPath = $script:keytoolPath
    }
    return
}

Write-Host 'First confirm this game has no previously used release signing key to retain.'
Write-Host 'The earlier limited directory search did not establish that no old key exists.'
Write-Host "New key location: $targetPath"
$confirmation = Read-Host 'Type CREATE NEW KEY to proceed, or press Enter to stop'
if ($confirmation -cne 'CREATE NEW KEY') { throw 'No key created.' }

$password = $null
$repeat = $null
$passwordPointer = [IntPtr]::Zero
$repeatPointer = [IntPtr]::Zero
$plainPassword = $null
$plainRepeat = $null
$temporaryPath = $null
$script:passwordVariable = 'EDGERUSH_SIGNING_PASSWORD_' + [Guid]::NewGuid().ToString('N')
try {
    $password = Read-Host 'Enter a new keystore password (at least 12 characters)' -AsSecureString
    $repeat = Read-Host 'Enter the same password again' -AsSecureString
    if ($password.Length -lt 12) { throw 'Use at least 12 characters. No key created.' }
    $passwordPointer = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    $repeatPointer = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($repeat)
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    $plainRepeat = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($repeatPointer)
    if (-not [string]::Equals($plainPassword, $plainRepeat, [System.StringComparison]::Ordinal)) {
        throw 'Passwords did not match. No key created.'
    }
    Assert-NoReparseAncestor $script:signingRoot
    if (Test-Path -LiteralPath $targetPath) { throw 'The output now exists; refusing to overwrite it.' }
    $null = [System.IO.Directory]::CreateDirectory($script:signingRoot)
    Assert-NoReparseAncestor $script:signingRoot
    $temporaryPath = Join-Path $script:signingRoot ('new-' + [Guid]::NewGuid().ToString('N') + '.keystore')
    $common = @('-J-Duser.language=en', '-J-Duser.country=US', '-keystore', $temporaryPath,
        '-storetype', 'PKCS12', '-storepass:env', $script:passwordVariable)
    $null = Invoke-Keytool -Arguments (@('-genkeypair', '-alias', $Alias, '-keyalg', 'RSA',
        '-keysize', [string]$KeySize, '-sigalg', 'SHA256withRSA', '-validity', '10000',
        '-dname', 'CN=Gizmobyte, O=Gizmobyte, C=MY', '-keypass:env', $script:passwordVariable) + $common) -PlainPassword $plainPassword
    $certificate = Invoke-Keytool -Arguments (@('-list', '-v', '-alias', $Alias) + $common) -PlainPassword $plainPassword
    $fingerprint = [regex]::Match($certificate, 'SHA256:\s*([0-9A-Fa-f:]{95})')
    if (-not $fingerprint.Success) { throw 'Could not verify the public certificate SHA-256 fingerprint.' }
    Assert-NoReparseAncestor $script:signingRoot
    # The two-argument Move refuses an existing destination, including a concurrent creator.
    [System.IO.File]::Move($temporaryPath, $targetPath)
    $temporaryPath = $null
    [pscustomobject]@{
        KeystorePath = $targetPath
        Alias = $Alias
        CertificateSHA256 = $fingerprint.Groups[1].Value.ToUpperInvariant()
    }
    Write-Host 'Back up the encrypted keystore and password in separate secure locations.'
    Write-Host 'Do not send either in chat or commit them. This did not configure Unity or verify a backup restore.'
}
finally {
    if ($passwordPointer -ne [IntPtr]::Zero) { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer) }
    if ($repeatPointer -ne [IntPtr]::Zero) { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($repeatPointer) }
    if ($null -ne $password) { $password.Dispose() }
    if ($null -ne $repeat) { $repeat.Dispose() }
    $plainPassword = $null
    $plainRepeat = $null
    # A failed generation/verification retains its uniquely named encrypted temporary file.
    # Do not delete a potentially valid new signing key automatically.
    if ($temporaryPath -and (Test-Path -LiteralPath $temporaryPath)) {
        Write-Warning "An unconfigured temporary keystore remains at $temporaryPath. Review it locally before any retry."
    }
}
