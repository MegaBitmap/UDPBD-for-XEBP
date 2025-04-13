#! /bin/bash

# This script is intended to run with MSYS2 UCRT64 on windows
pacman --noconfirm -Syuu
pacman --noconfirm --needed -S git mingw-w64-ucrt-x86_64-gcc

curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y --default-host x86_64-pc-windows-gnu --no-modify-path
export PATH="$USERPROFILE/.cargo/bin:$PATH"

git clone --recurse-submodules https://github.com/awaken1ng/udpbd-vexfat.git
cd ./udpbd-vexfat/
git reset --hard --recurse-submodules f20ec467e05328460d9493c0f252d87ca2a24407
cd ./vexfatbd/
cargo update
cd ..
cargo update

cargo build --release --target x86_64-pc-windows-gnu


