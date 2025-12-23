
$CLIDir = ".\UDPBD-for-XEB+-CLI\bin\Debug\net10.0"
$GUIDir = ".\UDPBD-for-XEB+-GUI\bin\Debug\net10.0-windows7.0"
$TrayDir = ".\UDPBDTray\bin\Debug\net10.0-windows7.0"

dotnet build ".\UDPBD-for-XEB+-CLI.sln"
dotnet build ".\UDPBD-for-XEB+-GUI.sln"
dotnet build ".\UDPBDTray.sln"

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

Copy-Item -Path .\udpbd-vexfat\target\x86_64-pc-windows-gnu\release\udpbd_vexfat.dll -Destination $GUIDir -Force
Copy-Item -Path .\udpbd-server\udpbd_server.dll -Destination $GUIDir -Force
Copy-Item -Path "..\List Builder\vmc_groups.list" -Destination $CLIDir -Force

Copy-Item -Path ".\Needed-for-Release\*" -Exclude *.txt -Destination $CLIDir -Force
Copy-Item -Path "$CLIDir\*" -Destination $GUIDir -Force
Copy-Item -Path "$TrayDir\*" -Destination $GUIDir -Force
Copy-Item -Path "$GUIDir\*" -Destination $TrayDir -Force

