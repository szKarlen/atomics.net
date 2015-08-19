$root = resolve-path .

$dllPath = "$root\src\System.Threading.Atomics\bin\Release\System.Threading.Atomics.dll"

Write-Host "Full path $dllPath"

$version = [System.Reflection.Assembly]::LoadFile($dllPath).GetName().Version
$versionStr = "{0}.{1}.{2}.{3}" -f ($version.Major, $version.Minor, $version.Build, $version.Revision)

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\NuGet\System.Threading.Atomics.nuspec) 
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\System.Threading.Atomics.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\nuget\System.Threading.Atomics.compiled.nuspec
