#! /bin/bash

# This script is intended to run with MSYS2 UCRT64 on windows
pacman --noconfirm -Syuu
pacman --noconfirm --needed -S git make mingw-w64-ucrt-x86_64-gcc

git clone -b windows https://github.com/MegaBitmap/udpbd-server.git
cd ./udpbd-server/
make


