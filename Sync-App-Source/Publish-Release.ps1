
$CLIDir = ".\UDPBD-for-XEB+-CLI\bin\Release\net10.0\publish"
$GUIDir = ".\UDPBD-for-XEB+-GUI\bin\Release\net10.0-windows7.0\publish"
$TrayDir = ".\UDPBDTray\bin\Release\net10.0-windows7.0\publish"

$ReleaseVersion = (Get-Content -Path ".\UDPBD-for-XEB+-GUI\UDPBD-for-XEB+-GUI.csproj" | Select-String -Pattern AssemblyVersion).ToString().Trim() -replace "<[^>]+>"
$ReleaseFolder = "$GUIDir\release-$ReleaseVersion"
$GUIDestinationFolder = "$ReleaseFolder\UDPBD-for-XEB+ Sync App"

dotnet publish ".\UDPBD-for-XEB+-CLI.sln"
dotnet publish ".\UDPBD-for-XEB+-GUI.sln"
dotnet publish ".\UDPBDTray.sln"

# Preserve the current working directory
$env:CHERE_INVOKING = "yes"
# Start a 64 bit Mingw environment
$env:MSYSTEM = "UCRT64"
# Run for the first time
& "C:\msys64\usr\bin\bash" "-lc" " "
# Update MSYS2 Core (in case any core packages are outdated)
& "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm -Syuu"
& "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm -Syuu"
& "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm --needed -S git make mingw-w64-ucrt-x86_64-gcc"
& "C:\msys64\usr\bin\bash" "-lc" "curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y --default-host x86_64-pc-windows-gnu --no-modify-path"
# Build udpbd_vexfat.dll
& "C:\msys64\usr\bin\bash" "-lc" "git clone --recurse-submodules -b windows_dll https://github.com/MegaBitmap/udpbd-vexfat.git"
& "C:\msys64\usr\bin\bash" "-lc" "export PATH=`"/c/Users/`$USER/.cargo/bin:`$PATH`"
cd udpbd-vexfat/vexfatbd/
cargo update
cd ..
cargo update
cargo build --release --target x86_64-pc-windows-gnu"
# Build udpbd_server.dll
& "C:\msys64\usr\bin\bash" "-lc" "git clone -b windows_dll https://github.com/MegaBitmap/udpbd-server.git"
& "C:\msys64\usr\bin\bash" "-lc" "cd udpbd-server/
make"

New-Item -ItemType Directory -Path $GUIDestinationFolder

Copy-Item -Path .\udpbd-vexfat\target\x86_64-pc-windows-gnu\release\udpbd_vexfat.dll -Destination $GUIDestinationFolder -Force
Copy-Item -Path .\udpbd-server\udpbd_server.dll -Destination $GUIDestinationFolder -Force

Get-ChildItem -File -Path "$CLIDir\*" | Move-Item -Destination $GUIDestinationFolder -Force
Get-ChildItem -File -Path "$GUIDir\*" | Move-Item -Destination $GUIDestinationFolder -Force
Get-ChildItem -File -Path "$TrayDir\*" | Move-Item -Destination $GUIDestinationFolder -Force

Copy-Item -Path ".\Needed-for-Release\*" -Exclude *.txt -Destination $GUIDestinationFolder
Copy-Item -Path ".\Needed-for-Release\*" -Include *.txt -Destination $ReleaseFolder
Copy-Item -Path "..\List Builder\vmc_groups.list" -Destination $GUIDestinationFolder
Get-Content "..\README.md" -Encoding utf8 | Out-File "$ReleaseFolder\README.txt" -Encoding utf8
Copy-Item -Path "..\List Builder" -Destination $ReleaseFolder -Recurse
Copy-Item -Path "..\PS2BBL-AutoStart" -Destination $ReleaseFolder -Recurse

New-Item -ItemType Directory -Path "$ReleaseFolder\XEBPLUS"
Copy-Item -Path "..\XEBPLUS\APPS" -Destination "$ReleaseFolder\XEBPLUS" -Recurse
Copy-Item -Path "..\XEBPLUS\PLG" -Destination "$ReleaseFolder\XEBPLUS" -Recurse

Add-Content -Path "$ReleaseFolder\XEBPLUS\APPS\neutrinoLauncher\version.txt" -Value "neutrino`n`nv$ReleaseVersion`nUDPBD-for-XEB+"

Compress-Archive -Path "$ReleaseFolder\*" -DestinationPath ".\UDPBD-for-XEBP-v$ReleaseVersion.zip" -Force

