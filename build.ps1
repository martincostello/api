#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][switch] $SkipTests
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

$installDotNetSdk = $false

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Information "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Information "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {

    ${env:DOTNET_INSTALL_DIR} = Join-Path $PSScriptRoot ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (!(Test-Path $sdkPath)) {
        if (!(Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        if (($PSVersionTable.PSVersion.Major -ge 6) -And !$IsWindows) {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --jsonfile $sdkFile --install-dir ${env:DOTNET_INSTALL_DIR} --no-path
        }
        else {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath
        }
    }
}
else {
    ${env:DOTNET_INSTALL_DIR} = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path ${env:DOTNET_INSTALL_DIR} "dotnet"

if ($installDotNetSdk) {
    ${env:PATH} = "${env:DOTNET_INSTALL_DIR};${env:PATH}"
}

function DotNetTest {
    param([string]$Project)

    $additionalArgs = @()

    if (-Not [string]::IsNullOrEmpty(${env:GITHUB_SHA})) {
        $additionalArgs += "--logger"
        $additionalArgs += "GitHubActions;report-warnings=false"
    }

    & $dotnet test $Project --configuration "Release" $additionalArgs -- RunConfiguration.TestSessionTimeout=1200000

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function DotNetPublish {
    param([string]$Project)

    & $dotnet publish $Project

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

$testProjects = @(
    (Join-Path $solutionPath "tests" "API.Tests" "API.Tests.csproj")
)

$publishProjects = @(
    (Join-Path $solutionPath "src" "API" "API.csproj")
)

Write-Information "Publishing solution..."
ForEach ($project in $publishProjects) {
    DotNetPublish $project
}

if (-Not $SkipTests) {
    Write-Information "Testing $($testProjects.Count) project(s)..."
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}
