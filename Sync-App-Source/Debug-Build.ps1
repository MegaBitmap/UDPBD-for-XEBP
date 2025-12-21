
$CLIDir = ".\UDPBD-for-XEB+-CLI\bin\Debug\net10.0"
$GUIDir = ".\UDPBD-for-XEB+-GUI\bin\Debug\net10.0-windows7.0"
$TrayDir = ".\UDPBDTray\bin\Debug\net10.0-windows7.0"

dotnet build ".\UDPBD-for-XEB+-CLI.sln"
dotnet build ".\UDPBD-for-XEB+-GUI.sln"
dotnet build ".\UDPBDTray.sln"

<#
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
#>

Copy-Item -Path "..\List Builder\vmc_groups.list" -Destination $CLIDir -Force

Copy-Item -Path ".\Needed-for-Release\*" -Exclude *.txt -Destination $CLIDir -Force
Copy-Item -Path "$CLIDir\*" -Destination $GUIDir -Force
Copy-Item -Path "$TrayDir\*" -Destination $GUIDir -Force
Copy-Item -Path "$GUIDir\*" -Destination $TrayDir -Force

