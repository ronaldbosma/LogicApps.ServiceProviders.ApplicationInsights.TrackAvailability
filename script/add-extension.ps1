function add-extension
{
   param(
       [ValidateScript({
          if( -Not ($_ | Test-Path) ){
             throw "Extension folder does not exist.. check Path parameter."
          }
          return $true
       })]
       [System.IO.FileInfo]$Path,

       [String]
       $Name,

       [String]
       $Version,

       [ValidateScript({
          if( -Not ($_ | Test-Path) ){
             throw "Target project directory does not exist.. check TargetProjectDirectory parameter."
          }
          return $true
       })]
       [System.IO.FileInfo]$TargetProjectDirectory
   )
   process
   {
      $extensionPath = $Path
      $extensionName = $Name
      $extensionNameServiceProvider = "TrackAvailabilityExtensionConfigProvider"

      write-host "Nuget extension path is $extensionPath"

      $userprofile = [environment]::GetFolderPath('USERPROFILE')
      $dll =  Join-Path -Path $extensionPath -ChildPath "net6.0"
      $childPath = $extensionName + ".dll"
      $dll =  Join-Path -Path $dll -ChildPath $childPath

      if ( Test-Path -Path $dll )
      {
            write-host "Extension dll path is $dll"
      }
      else
      {
            throw "Extension dll path does not exist $dll"
      }

      $extensionModulePath = Join-Path -Path $userprofile -ChildPath ".azure-functions-core-tools"
      $extensionModulePath = Join-Path -Path $extensionModulePath -ChildPath "Functions"
      $extensionModulePath = Join-Path -Path $extensionModulePath -ChildPath "ExtensionBundles"
      $extensionModulePath = Join-Path -Path $extensionModulePath -ChildPath "Microsoft.Azure.Functions.ExtensionBundle.Workflows"

      if ( Test-Path -Path $extensionModulePath )
      {
            write-host "Extension bundle module path is $extensionModulePath"
      }
      else
      {
            throw "Extension bundle module path does not exist $extensionModulePath"
      }

      $latest = Get-ChildItem -Path $extensionModulePath | Sort-Object name -Descending | Select-Object -First 1
      $latest.name

      $extensionModulePath = Join-Path -Path $extensionModulePath -ChildPath $latest.name 
      $extensionModulePath = Join-Path -Path $extensionModulePath -ChildPath "bin"
      $extensionModulePath = Join-Path -Path $extensionModulePath -ChildPath "extensions.json"

      if ( Test-Path -Path $extensionModulePath )
      {
            write-host "EXTENSION PATH is $extensionModulePath and dll Path is $dll"
      }
      else
      {
            throw "The extensions.json in extension bundle does not exist $extensionModulePath"
      }

      $fullAssemlyName = [System.Reflection.AssemblyName]::GetAssemblyName($dll).FullName

      try
      {
            # Kill all the existing func.exe else modeule extension cannot be modified. 
            taskkill /IM "func.exe" /F
      }
      catch
      {
         write-host "func.exe not found"
      }
      
      $extensionFullName = $extensionName
      $startupClass = "$Name.TrackAvailabilityServiceProviderStartup"

      try {
         Push-Location $TargetProjectDirectory
         
            write-host "Add package to project in $TargetProjectDirectory"

            # 1. Add Nuget package to existing project 
         dotnet add package $extensionFullName --version $Version  --source $extensionPath
      }
      finally {
         Pop-Location
      }

      #  2. Update extensions.json under extension module
      $typeFullName =  $startupClass + ", " + $fullAssemlyName
      $newNode =  [pscustomobject] @{ 
         name = $extensionNameServiceProvider
         typeName = $typeFullName}

      $jsonContent = Get-Content $extensionModulePath -raw | ConvertFrom-Json
      if ( ![bool]($jsonContent.extensions.name -match $extensionNameServiceProvider))
      {
            $jsonContent.extensions += $newNode
      }
      else
      {
            $jsonContent.extensions | % {if($_.name -eq $extensionNameServiceProvider){$_.typeName=$typeFullName}}
      }
      $jsonContent | ConvertTo-Json -depth 32| set-content $extensionModulePath

      # 3. update dll in extension module.
      $spl = Split-Path $extensionModulePath

      Copy-Item $dll  -Destination $spl

      Write-host "File  $dll is successfully Copied to folder $spl  "
      Write-host "Extension $extensionName is successfully added.  "
   }
}

# execute the above function here.

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$rootPath = Join-Path -Path $scriptDir -ChildPath ".."

$nuGetPackagePath = Join-Path -Path $rootPath -ChildPath "src"
$nuGetPackagePath = Join-Path -Path $nuGetPackagePath -ChildPath "bin"
$nuGetPackagePath = Join-Path -Path $nuGetPackagePath -ChildPath "Debug"

$targetProjectDirectory = Join-Path -Path $rootPath -ChildPath "samples"
$targetProjectDirectory = Join-Path -Path $targetProjectDirectory -ChildPath "nuget-package-based"

add-extension -Path $nuGetPackagePath -Name "LogicApps.ServiceProviders.ApplicationInsights.TrackAvailability" -Version "0.1.0" -TargetProjectDirectory $targetProjectDirectory