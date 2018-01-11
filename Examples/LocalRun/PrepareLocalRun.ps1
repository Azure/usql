#Setup for U-SQL Local Run

Set-StrictMode -Version 2
$ErrorActionPreference = "Stop"


$usqlsdk_pkg_name = "Microsoft.Azure.DataLake.USQL.SDK"
$pkg_source = "https://www.nuget.org/api/v2/"

$usqlsdk_pkg = find-package -Name $usqlsdk_pkg_name -Source $pkg_source



$pkg_folder = join-path $env:ProgramFiles ("PackageManagement\NuGet\Packages\" + $usqlsdk_pkg.Name + "." + $usqlsdk_pkg.Version)


# ----------------------------------------
# Package Installation
# ----------------------------------------

$install = $false
if (!(Test-Path $pkg_folder))
{
    $install = $true
}


if ($install)
{
    $usqlsdk_pkg | Install-Package -Verbose
}


Write-Host "USQL SDL Package Installed at" $pkg_folder


# ----------------------------------------
# Data Root Installation
# ----------------------------------------

$dataroot = join-path $env:LOCALAPPDATA "USQLDataRoot"

if (!(Test-Path $dataroot))
{
    Write-Host DataRoot does not exist. Creating    
    New-Item $dataroot -type directory
}

$env:LOCALRUN_DATAROOT = $dataroot

Write-Host "LOCALRUN_DATAROOT:" $dataroot

# ----------------------------------------
# Verify LocalRunHelper is available
# ----------------------------------------


$localrunhelperexe = join-path $pkg_folder "build\runtime\LocalRunHelper.exe"

Write-Host "LocalRunHelper.exe:" $localrunhelperexe

if (!(Test-Path $localrunhelperexe ))
{
    Write-Host LocalRunHelper does not exist path exists
    exit
}



# ----------------------------------------
# Configure Alises
# ----------------------------------------

Set-Alias "LocalRunHelper" $localrunhelperexe 

