#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $true)][string] $PullRequestId,
    [Parameter(Mandatory = $false)][string] $AccessToken,
    [Parameter(Mandatory = $false)][string] $Benchmark = "root",
    [Parameter(Mandatory = $false)][string] $Repository = "https://github.com/martincostello/api",
    [Parameter(Mandatory = $false)][switch] $PublishResults
)

$additionalArgs = @()

if (![string]::IsNullOrEmpty($AccessToken)) {
    $additionalArgs += "--access-token"
    $additionalArgs += $AccessToken

    if ($PublishResults) {
        $additionalArgs += "--publish-results"
        $additionalArgs += "true"
    }
}

if ($IsWindows) {
    Start-Process -FilePath "crank-agent" -WindowStyle Hidden | Out-Null
} else {
    Start-Process -FilePath "crank-agent" | Out-Null
}

Start-Sleep -Seconds 2

$repoPath = Split-Path $MyInvocation.MyCommand.Definition
$components = "api"
$config = Join-Path $repoPath "benchmark.yml"
$profiles = "local"

crank-pr `
    --benchmarks $Benchmark `
    --components $components `
    --config $config `
    --profiles $profiles `
    --pull-request $PullRequestId `
    --repository $Repository `
    $additionalArgs
