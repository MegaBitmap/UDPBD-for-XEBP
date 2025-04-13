#! /bin/bash

# This script is intended to run with MSYS2 UCRT64 on windows
pacman --noconfirm -Syuu
pacman --noconfirm --needed -S git make mingw-w64-ucrt-x86_64-gcc

git clone -b windows https://github.com/israpps/udpbd-server.git
cd ./udpbd-server/
git reset --hard 7b20316d861d9187747715b89aed3ac811984809
make


