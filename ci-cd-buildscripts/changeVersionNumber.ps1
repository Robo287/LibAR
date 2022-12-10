# If found use it to version the assemblies.
#
# For example, if the 'Build number format' build pipeline parameter 
# $(BuildDefinitionName)_$(Year:yyyy).$(Month).$(DayOfMonth)$(Rev:.r)
# then your build numbers come out like this:
# "Build HelloWorld_2013.07.19.1"
# This script would then apply version 2013.07.19.1 to your assemblies.
	
# Enable -Verbose option
[CmdletBinding()]
	
# Regular expression pattern to find the version in the build number 
# and then apply it to the assemblies
$VersionRegex = "\d+\.\d+\.\d+\.\d+"
	
# If this script is not running on a build server, remind user to 
# set environment variables so that this script can be debugged
if(-not ($Env:BUILD_SOURCESDIRECTORY -and $Env:BUILD_BUILDNUMBER))
{
	Write-Error "You must set the following environment variables"
	Write-Error "to test this script interactively."
	Write-Host '$Env:BUILD_SOURCESDIRECTORY - For example, enter something like:'
	Write-Host '$Env:BUILD_SOURCESDIRECTORY = "C:\code\FabrikamTFVC\HelloWorld"'
	Write-Host '$Env:BUILD_BUILDNUMBER - For example, enter something like:'
	Write-Host '$Env:BUILD_BUILDNUMBER = "Build HelloWorld_0000.00.00.0"'
	exit 1
}
	
# Make sure path to source code directory is available
if (-not $Env:BUILD_SOURCESDIRECTORY)
{
	Write-Error ("BUILD_SOURCESDIRECTORY environment variable is missing.")
	exit 1
}
elseif (-not (Test-Path $Env:BUILD_SOURCESDIRECTORY))
{
	Write-Error "BUILD_SOURCESDIRECTORY does not exist: $Env:BUILD_SOURCESDIRECTORY"
	exit 1
}
Write-Verbose "BUILD_SOURCESDIRECTORY: $Env:BUILD_SOURCESDIRECTORY"
	
# Make sure there is a build number
if (-not $Env:BUILD_BUILDNUMBER)
{
	Write-Error ("BUILD_BUILDNUMBER environment variable is missing.")
	exit 1
}
Write-Verbose "BUILD_BUILDNUMBER: $Env:BUILD_BUILDNUMBER"
	
# Get and validate the version data
$VersionData = [regex]::matches($Env:BUILD_BUILDNUMBER,$VersionRegex)
switch($VersionData.Count)
{
   0		
      { 
         Write-Error "Could not find version number data in BUILD_BUILDNUMBER."
         exit 1
      }
   1 {}
   default 
      { 
         Write-Warning "Found more than instance of version data in BUILD_BUILDNUMBER." 
         Write-Warning "Will assume first instance is version."
      }
}
$NewVersion = $VersionData[0]
Write-Verbose "Version: $NewVersion"
	
# Apply the version to the assembly property files
$files = gci $Env:BUILD_SOURCESDIRECTORY -recurse -include "*Properties*","My Project" | 
	?{ $_.PSIsContainer } | 
	foreach { gci -Path $_.FullName -Recurse -include AssemblyInfo.* }
if($files)
{
	Write-Verbose "Will apply $NewVersion to $($files.count) files."
	
	foreach ($file in $files) {
		$filecontent = Get-Content($file)
		attrib $file -r
		$filecontent -replace $VersionRegex, $NewVersion | Out-File $file
		Write-Verbose "$file.FullName - version applied"
	}
}
else
{
	Write-Warning "Found no files."
}

#apply the version number to the Project settings file
#first create a backup
$overwrite = $true

# ProjectSettins.asset has version hardcoded to 1.0.0.2 so we need to update it
if([System.IO.File]::Exists(".\ProjectSettings\ProjectSettings.asset"))
{
    [System.IO.File]::Copy(".\ProjectSettings\ProjectSettings.asset", ".\ProjectSettings\ProjectSettings.backup", $overwrite)
    $settingsTextOld = [System.IO.File]::ReadAllText(".\ProjectSettings\ProjectSettings.asset")
    $settingsTextNew = $settingsTextOld.replace("bundleVersion: 1.0.0.2", "bundleVersion: " + $NewVersion)
    $settingsText = $settingsTextNew.replace("metroPackageVersion: 1.0.0.2", "metroPackageVersion: " + $NewVersion)
	Write-verbose "Found ProjectSettings.asset file"
	Write-Verbose $settingsText
    [System.IO.File]::WriteAllText(".\ProjectSettings\ProjectSettings.asset", $settingsText)
}

# Package.appxmanifest has version hardcoded to 1.0.0.0 so we need to update it
if([System.IO.File]::Exists(".\LibAR\Package.appxmanifest"))
{
    [System.IO.File]::Copy(".\LibAR\Package.appxmanifest", ".\LibAR\Package.backup", $overwrite)
    $settingsTextOld = [System.IO.File]::ReadAllText(".\LibAR\Package.appxmanifest")
    $settingsText = $settingsTextOld.replace("1.0.0.0", $NewVersion)
	Write-verbose "Found Package.appxmanifext file"
	Write-Verbose $settingsText
    [System.IO.File]::WriteAllText(".\LibAR\Package.appxmanifest", $settingsText)
}
