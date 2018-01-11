Set-StrictMode -Version 2
$ErrorActionPreference = "Stop"

#Setup for U-SQL Local Run

$usqlsdk_pkg_name = "Microsoft.Azure.DataLake.USQL.Interfaces"
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




$code =’using System;  
using System.Collections.Generic;  
using System.Text; 
 
namespace PSDLL  
{  
    class Program  
    {  
        static void Main(string[] args)  
        {  
        } 
 
        public int sum(int x, int y)  
        {  
            return x + y;  
        } 
 
    }  
}’;  

$libfolder = join-path $pkg_folder "lib/net45"

$interfaces_dll = join-path $libfolder "Microsoft.Analytics.Interfaces.dll"
$types_dll = join-path $libfolder "Microsoft.Analytics.Types.dll"


$src_cs = "d:\sourcecode.cs"
$code | Out-File $src_cs
$compiler = "$env:windir/Microsoft.NET/Framework/v4.0.30319/csc"  

$dest_dll = "d:\sourcecode.dll"

&$compiler /target:library /out:`"$dest_dll`" d:\`"$src_cs`" /reference:`"$interfaces_dll`"  /reference:`"$types_dll`" 
