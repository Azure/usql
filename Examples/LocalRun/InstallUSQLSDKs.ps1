#Setup for U-SQL Local Run

Set-StrictMode -Version 2
$ErrorActionPreference = "Stop"

$pkg_names = @(
    "Microsoft.Azure.DataLake.USQL.SDK",
    "Microsoft.Azure.DataLake.USQL.Interfaces"
)

$pkg_source = "https://www.nuget.org/api/v2/"

$root_pkg_folder = join-path $env:ProgramFiles "PackageManagement\NuGet\Packages\"


function Install-UsqlSdks()
{
    if (!(Test-Path $root_pkg_folder))
    {
        New-Item $root_pkg_folder -type directory
    }

    foreach ($pkg_name in $pkg_names)
    {
        Write-Host "Looking for package on Nuget" $pkg_name

        $pkg = Find-Package -Name $pkg_name -Source $pkg_source 

        if ($pkg -eq $null)
        {
            Throw "DId not find package"
        }

        Write-Host "Package found"

        $pkg_folder = join-path $root_pkg_folder ($pkg.Name + "." + $pkg.Version)
        Write-Host "Package will be installed here: " $pkg_folder 


        $install = $false

        if (!(Test-Path $pkg_folder))
        {
            Write-Host "Package is not installed. Will download and install" 
            $install = $true
        }

        if ($install)
        {

            Write-Host "Installing" 
            $pkg | Install-Package -Verbose -Destination $root_pkg_folder
        }
    }
}


Install-UsqlSdks