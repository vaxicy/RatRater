$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$csc = 'D:\CodeBuddyData\roslyn\tasks\netcore\bincore\csc.dll'
$game = 'D:\Probably Stolen\Probably Stolen_Data\Managed'
$out = Join-Path $root 'build\RatRater.dll'

if (-not (Test-Path -LiteralPath (Join-Path $root 'build'))) {
    New-Item -ItemType Directory -Path (Join-Path $root 'build') | Out-Null
}

$refs = @(
    'mscorlib.dll', 'System.dll', 'System.Core.dll', 'netstandard.dll',
    'UnityEngine.dll', 'UnityEngine.CoreModule.dll', 'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll', 'UnityEngine.InputLegacyModule.dll', '0Harmony.dll', 'Assembly-CSharp.dll'
) | ForEach-Object { Join-Path $game $_ }

$args = @('dotnet', $csc, '-nologo', '-target:library', "-out:$out", '-langversion:latest')
foreach ($r in $refs) { $args += "-r:$r" }
$args += (Join-Path $root 'src\RatRaterMod.cs')

$sources = @(
    'GameBridge.cs', 'Scorer.cs', 'HighlightRenderer.cs', 'SortPatch.cs', 'ButcherService.cs', 'BladeBatchPatch.cs', 'RatRaterMod.cs'
) | ForEach-Object { Join-Path $root "src\$_" }

& dotnet $csc -nologo -target:library "-out:$out" '-langversion:latest' ($refs | ForEach-Object { "-r:$_" }) $sources
Write-Output ("exit=" + $LASTEXITCODE)
