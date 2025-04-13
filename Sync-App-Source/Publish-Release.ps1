
$ReleaseVersion = (Get-Content -Path ".\UDPBD-for-XEB+-GUI\UDPBD-for-XEB+-GUI.csproj" | Select-String -Pattern AssemblyVersion).ToString().Trim() -replace "<[^>]+>"
$ReleaseFolder = ".\UDPBD-for-XEB+-GUI\bin\Release\net8.0-windows\publish\release-$ReleaseVersion"

dotnet publish ".\UDPBD-for-XEB+-CLI.sln"
dotnet publish ".\UDPBD-for-XEB+-GUI.sln"

New-Item -ItemType Directory -Path "$ReleaseFolder\UDPBD-for-XEB+ Sync App"

Get-ChildItem -File -Path ".\UDPBD-for-XEB+-CLI\bin\Release\net8.0\publish\*" | Move-Item -Destination "$ReleaseFolder\UDPBD-for-XEB+ Sync App"
Get-ChildItem -File -Path ".\UDPBD-for-XEB+-GUI\bin\Release\net8.0-windows\publish\*" | Move-Item -Destination "$ReleaseFolder\UDPBD-for-XEB+ Sync App"

if (Test-Path -Path "C:\msys64\usr\bin\bash.exe" -PathType Leaf)
{
    # Preserve the current working directory
    $env:CHERE_INVOKING = "yes"
    # Start a 64 bit Mingw environment
    $env:MSYSTEM = "UCRT64"
    # Run for the first time
    & "C:\msys64\usr\bin\bash" "-lc" " "
    # Update MSYS2 Core (in case any core packages are outdated)
    & "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm -Syuu"

    & "C:\msys64\usr\bin\bash.exe" "-lc" "bash ./Build-udpbd-vexfat-MSYS2.sh"
    & "C:\msys64\usr\bin\bash.exe" "-lc" "bash ./Build-udpbd-server-MSYS2.sh"
}

if (Test-Path -Path ".\udpbd-vexfat\target\x86_64-pc-windows-gnu\release\udpbd-vexfat.exe" -PathType Leaf)
{
    Copy-Item -Path ".\udpbd-vexfat\target\x86_64-pc-windows-gnu\release\udpbd-vexfat.exe" -Destination ".\Needed-for-Release\udpbd-vexfat.exe"
}
if (Test-Path -Path ".\udpbd-server\udpbd-server.exe" -PathType Leaf)
{
    Copy-Item -Path ".\udpbd-server\udpbd-server.exe" -Destination ".\Needed-for-Release\udpbd-server.exe"
}

Copy-Item -Path ".\Needed-for-Release\*" -Exclude *.txt -Destination "$ReleaseFolder\UDPBD-for-XEB+ Sync App"
Copy-Item -Path ".\Needed-for-Release\*" -Include *.txt -Destination $ReleaseFolder
Copy-Item -Path "..\List Builder" -Destination $ReleaseFolder -Recurse

New-Item -ItemType Directory -Path "$ReleaseFolder\XEBPLUS"

Copy-Item -Path "..\XEBPLUS\APPS" -Destination "$ReleaseFolder\XEBPLUS" -Recurse
Copy-Item -Path "..\XEBPLUS\PLG" -Destination "$ReleaseFolder\XEBPLUS" -Recurse

Compress-Archive -Path "$ReleaseFolder\*" -DestinationPath ".\UDPBD-for-XEBP-v$ReleaseVersion.zip" -Force

Remove-Item -Path ".\UDPBD-for-XEB+-CLI\bin\Release" -Recurse
Remove-Item -Path ".\UDPBD-for-XEB+-GUI\bin\Release" -Recurse


