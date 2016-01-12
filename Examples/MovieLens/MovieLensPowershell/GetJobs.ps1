#
# GetJobs.ps1
#
# https://azure.microsoft.com/en-gb/documentation/articles/data-lake-analytics-get-started-powershell/
Import-Module AzureRM.Profile
login-AzureRmAccount

Get-AzureRmDataLakeAnalyticsJob -AccountName MovieLens | sort SubmitTime -Descending 