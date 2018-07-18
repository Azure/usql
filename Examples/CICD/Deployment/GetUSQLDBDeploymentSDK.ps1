<#
    This script is used for getting dependencies and SDKs for U-SQL database deployment.
    PowerShell command line support for deploying U-SQL database package(.usqldbpack file) will come soon.
    
    Example :
        GetUSQLDBDeploymentSDK.ps1 -AzureSDK "AzureSDKFolderPath" -DBDeploymentTool "DBDeploymentToolFolderPath"
#>

param (
    [string]$AzureSDK = "AzureSDK", # Folder to cache Azure SDK dependencies
    [string]$DBDeploymentTool = "DBDeploymentTool", # Folder to cache U-SQL dabatase deployment tool
    [string]$workingfolder = "" # Folder to execute these command lines
)

if ([string]::IsNullOrEmpty($workingfolder))
{
    $scriptpath = $MyInvocation.MyCommand.Path
    $workingfolder = Split-Path $scriptpath
}
cd $workingfolder

echo "workingfolder=$workingfolder, outputfolder=$outputfolder"
echo "Downloading required packages..."

iwr https://www.nuget.org/api/v2/package/Microsoft.Azure.Management.DataLake.Analytics/3.2.3-preview -outf Microsoft.Azure.Management.DataLake.Analytics.3.2.3-preview.zip
iwr https://www.nuget.org/api/v2/package/Microsoft.Azure.Management.DataLake.Store/2.3.3-preview -outf Microsoft.Azure.Management.DataLake.Store.2.3.3-preview.zip
iwr https://www.nuget.org/api/v2/package/Microsoft.IdentityModel.Clients.ActiveDirectory/2.28.3 -outf Microsoft.IdentityModel.Clients.ActiveDirectory.2.28.3.zip
iwr https://www.nuget.org/api/v2/package/Microsoft.Rest.ClientRuntime/2.3.11 -outf Microsoft.Rest.ClientRuntime.2.3.11.zip
iwr https://www.nuget.org/api/v2/package/Microsoft.Rest.ClientRuntime.Azure/3.3.7 -outf Microsoft.Rest.ClientRuntime.Azure.3.3.7.zip
iwr https://www.nuget.org/api/v2/package/Microsoft.Rest.ClientRuntime.Azure.Authentication/2.3.3 -outf Microsoft.Rest.ClientRuntime.Azure.Authentication.2.3.3.zip
iwr https://www.nuget.org/api/v2/package/Newtonsoft.Json/6.0.8 -outf Newtonsoft.Json.6.0.8.zip
iwr https://www.nuget.org/api/v2/package/Microsoft.Azure.DataLake.USQL.SDK/ -outf USQLSDK.zip

echo "Extracting packages..."

Expand-Archive Microsoft.Azure.Management.DataLake.Analytics.3.2.3-preview.zip -DestinationPath Microsoft.Azure.Management.DataLake.Analytics.3.2.3-preview -Force
Expand-Archive Microsoft.Azure.Management.DataLake.Store.2.3.3-preview.zip -DestinationPath Microsoft.Azure.Management.DataLake.Store.2.3.3-preview -Force
Expand-Archive Microsoft.IdentityModel.Clients.ActiveDirectory.2.28.3.zip -DestinationPath Microsoft.IdentityModel.Clients.ActiveDirectory.2.28.3 -Force
Expand-Archive Microsoft.Rest.ClientRuntime.2.3.11.zip -DestinationPath Microsoft.Rest.ClientRuntime.2.3.11 -Force
Expand-Archive Microsoft.Rest.ClientRuntime.Azure.3.3.7.zip -DestinationPath Microsoft.Rest.ClientRuntime.Azure.3.3.7 -Force
Expand-Archive Microsoft.Rest.ClientRuntime.Azure.Authentication.2.3.3.zip -DestinationPath Microsoft.Rest.ClientRuntime.Azure.Authentication.2.3.3 -Force
Expand-Archive Newtonsoft.Json.6.0.8.zip -DestinationPath Newtonsoft.Json.6.0.8 -Force
Expand-Archive USQLSDK.zip -DestinationPath USQLSDK -Force


echo "Copy required DLLs to output folder..."

mkdir $AzureSDK -Force
mkdir $DBDeploymentTool -Force
copy Microsoft.Azure.Management.DataLake.Analytics.3.2.3-preview\lib\net452\*.dll $AzureSDK
copy Microsoft.Azure.Management.DataLake.Store.2.3.3-preview\lib\net452\*.dll $AzureSDK
copy Microsoft.IdentityModel.Clients.ActiveDirectory.2.28.3\lib\net45\*.dll $AzureSDK
copy Microsoft.Rest.ClientRuntime.2.3.11\lib\net452\*.dll $AzureSDK
copy Microsoft.Rest.ClientRuntime.Azure.3.3.7\lib\net452\*.dll $AzureSDK
copy Microsoft.Rest.ClientRuntime.Azure.Authentication.2.3.3\lib\net452\*.dll $AzureSDK
copy Newtonsoft.Json.6.0.8\lib\net45\*.dll $AzureSDK
copy USQLSDK\build\runtime\*.* $DBDeploymentTool
