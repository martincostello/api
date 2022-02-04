#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $Scenario = "root",
    [Parameter(Mandatory = $false)][string] $Profile = "local",
    [Parameter(Mandatory = $false)][string] $BranchOrCommitOrTag = ""
)

$additionalArgs = @()

if (![string]::IsNullOrEmpty($BranchOrCommitOrTag)) {
    $additionalArgs += "--application.source.branchOrCommit"
    $additionalArgs += $BranchOrCommitOrTag
}

$repoPath = Split-Path $MyInvocation.MyCommand.Definition
$config = Join-Path $repoPath "crank.yml"

$agent = Start-Process -FilePath "crank-agent" -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 2

try {
    crank --config $config --scenario $Scenario --profile $Profile $additionalArgs
}
finally {
    Stop-Process -InputObject $agent -Force | Out-Null
}
