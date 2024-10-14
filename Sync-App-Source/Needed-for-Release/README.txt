# THIS BRANCH IS A WORK IN PROGRESS
# UNDER CONSTRUCTION
# DO NOT USE

# XEB+ neutrino Launcher Plugin

This is a plugin for the Xtreme Elite Boot Plus dashboard for the PlayStation 2.  
It allows XEB+ to load PlayStation 2 games from HDD, MX4SIO, UDPBD, and USB via [neutrino](https://github.com/rickgaiser/neutrino).  
It is bundled with a sync app `UDPBD-for-XEB+-GUI.exe` for windows or `UDPBD-for-XEB+-CLI.dll` for linux.  
The sync app automates the setup process.  

## Features

  * Load PlayStation 2 game backups straight from the Xtreme Elite Boot Plus dashboard.
  * Supports game loading from HDD, MX4SIO, UDPBD, and USB.
  * Supports high capacity exFAT drives (currently tested with drives up to 4TB).
  * Quick navigation functions for browsing large game lists.
  * Remember last played game.
  * Display game-specific artwork on the menu.
  * Display metadata for each game.
  * Set neutrino compatibility flags and other options on a global or per-game basis.
  * Favorites list.
  * Custom theme integration.

## Requirements

  * A PlayStation 2 console with a memory card that has either FreeMCBoot or PS2BBL installed.  
[PS2BBL](https://israpps.github.io/PlayStation2-Basic-BootLoader/) is the recommended option, and is required for USB loading.
  * All of the requirements specific to the device you want to load games from.
 
### HDD

  * A fat PlayStation 2 console.
  * A PlayStation 2 network adapter or hard drive add-on.
  * A hard drive or SSD that is compatible with your Playstaion 2 network or hard drive adapter.
  * A FAT32 or exFAT formatted USB drive 1GB or larger.
  * A PC hard drive dock or USB adapter (optional)
    
### MX4SIO

  * An MX4SIO adapter
  * An exFAT formatted SD card no less than 4GB in size.  
32GB or larger is recommended.
  * A FAT32 or exFAT formatted USB drive 1GB or larger.
  * A USB SD card reader (optional)
    
### UDPBD

  * An OEM network adapter if using a fat PS2.
  * A PC with enough free space to store game ISOs.
  * A MBR FAT32 or exFAT formatted USB drive 1GB or larger.  
  This is used for XEB+ and assorted files, game ISOs are stored on the PC.
  * An ethernet cable to connect the PS2 to the PC.  
If your PC does not have an ethernet port available, a USB ethernet adapter will work.  

### USB

  * An exFAT formatted USB drive no less than 4GB in size.  
32GB or larger is recommended.

## Setup

*Note: This setup process is for version 2 of this plugin.  
If you have previously installed version 1, you will need to remove the old version of neutrino Launcher from your XEB+ install before continuing.  
This includes all neutrino Launcher files in the `APPS`, `PLG`, and `CFG` folders, as well as the `CD` and `DVD` folders on the root of your USB drive.*

1. If you have not already done so, download XtremeElieteBoot+:  
http://www.hwc.nat.cu/ps2-vault/hwc-projects/xebplus/  
or  
https://web.archive.org/web/*/hwc.nat.cu/ps2-vault/hwc-projects/xebplus
2. Extract the `XEBPLUS` folder to the root of your USB drive, and ensure that you can load into the XEB+ dashboard on your PlayStation 2.  
Note that if you are using an exFAT formatted USB drive, you will need to use PS2BBL and [this version](https://github.com/israpps/wLaunchELF_ISR) of wLaunchELF. 
3. (optional) Configure your PS2 exploit of choice to autorun XEB+ on startup.
4. Download the latest version of this plugin from the [latest release on this page](https://github.com/MegaBitmap/UDPBD-for-XEBP/releases).  
Extract the`XEBPLUS`folder to the root of your USB drive, merging all folders if prompted.
5. Extract the UDPBD Sync app or List Builder application.
6. Complete setup by following the steps specific to the device you want to load games from.

### UDPBD on Windows

1. Somewhere on your PC, for example `Documents`, create a new folder `PS2`.

2. Inside the `PS2` folder, create `CD` and `DVD` folders.

3. Rip/copy any PlayStation 2 ISOs you wish to load into the folder that corresponds with their original source media.

4. All `.bin` + `.cue` CD games must be converted to `.iso` format.  
The sync app will convert them automatically if placed in the CD folder and check the `Convert CD Bin to ISO` box before synchronizing.

5. From the [latest release on this page](https://github.com/MegaBitmap/UDPBD-for-XEBP/releases), extract the `UDPBD-for-XEB+ Sync App` folder.

6. Plug in the ethernet cable as shown:  
![ps2-slim-connected-to-laptop](readme-images/ps2-slim-connected-to-laptop.jpg)

7. For a direct connection, as seen in the previous step, set a manual IPv4 address and subnet mask.  
![pc-ip-settings](readme-images/pc-ip-settings.jpg)  
For using a router instead of a direct connnection, set the PC's IP assignmnet to *Automatic (DHCP)*.  
Then [follow this guide](http://ps2ulaunchelf.pbworks.com/w/page/19520139/ps2ftp) for assigning a proper IP address on the PS2.  
LaunchELF defaults to *192.168.0.10* and needs to be changed manualy to work on your network.

8. Hold *R1* on the controller during FreeMCBoot/PS2BBL startup to open LaunchELF.  
Open MISC -> PS2Net  
![launchelf-ps2net](readme-images/launchelf-ps2net.jpg)  

9. Let the PS2 idle on this screen for the next steps on the PC.  
![launchelf-ftp-enabled](readme-images/launchelf-ftp-enabled.jpg)

10. Run `UDPBD-for-XEB+-GUI.exe` and click connect.  
![udpbd-xeb-1](readme-images/udpbd-xeb-sync-1.jpg)

11. Click `Select Game Path` then choose an ISO from the `DVD` folder from step 8.

12. Click `Sync with PS2`, upon success this message will be displayed:  
![udpbd-xeb-synced](readme-images/udpbd-xeb-sync-2.jpg)

13. Click `Start Server` and make sure to allow.  
![udpbd-vexfat-firewall](readme-images/udpbd-vexfat-firewall.jpg)  
If you miss clicked, either move the `UDPBD-for-XEB+` folder inside a new folder or manually delete the inbound rules for `udpbd-vexfat` in `Windows Defender Firewall with Advanced Security`.  
udpbd-vexfat will open minimized.  
The server needs to be open and running for the entire play session. (Disable sleep on the PC.)

14. The PC setup is now complete, back on the PS2 run XEB+.  
![launchelf-xeb](readme-images/launchelf-xeb.jpg)
It's recommended to set FreeMCBoot or PS2BBL to autorun the `.ELF` file.

15. Play!  
![xeb-game-list](readme-images/xeb-game-list.jpg)  

Repeat steps 8-15 of the setup process after adding or removeing games.

### UDPBD on Linux

I will be using Ubuntu, if you are using a different distribution these steps may vary.  

1. Compile the udpbd-server
```
sudo apt update
sudo apt upgrade
sudo apt install git build-essential
git clone https://gitlab.com/ps2max/udpbd-server.git
cd udpbd-server
make
```
2. Install .NET 8, GParted, and exfatprogs
```
sudo apt install dotnet-runtime-8.0 gparted exfatprogs
```
3. Create a new exFAT partition in GParted and note the partition number.  
For this guide the exFAT partition is `/dev/nvme0n1p6`, it will most likely be slightly different on your system.  
![gparted-exfat](readme-images/gparted-exfat.jpg)
4. Mount it to `/mnt/ps2`. Your storage device `/dev/nvme0n1p6` will probably be differnt.
```
sudo mkdir /mnt/ps2/
sudo mount /dev/nvme0n1p6 /mnt/ps2/ -o uid=$USER
```
5. Create folders named `CD` and `DVD` in the exFAT partition.  
Rip/copy any PlayStation 2 disc images you wish to load into the folder that corresponds with their original source media.  
Example:  `/mnt/ps2/DVD/Grand Theft Auto III.iso`

6. From the [latest release on this page](https://github.com/MegaBitmap/UDPBD-for-XEBP/releases), extract the `UDPBD-for-XEB+ Sync App` folder.

7. Plug in the ethernet cable as shown:  
![ps2-slim-connected-to-laptop](readme-images/ps2-slim-connected-to-laptop.jpg)

8. For a direct connection, as seen in the previous step, set a manual IPv4 address and subnet mask.  
![ubuntu-ip-settings](readme-images/ubuntu-ip-settings.jpg)  
For using a router instead of a direct connnection, set the PC's IP assignmnet to *Automatic (DHCP)*.  
Then [follow this guide](http://ps2ulaunchelf.pbworks.com/w/page/19520139/ps2ftp) for assigning a proper IP address on the PS2.  
LaunchELF defaults to *192.168.0.10* and needs to be changed manualy to work on your network.

9. Hold *R1* on the controller during FreeMCBoot/PS2BBL startup to open LaunchELF.  
Open MISC -> PS2Net  
![launchelf-ps2net](readme-images/launchelf-ps2net.jpg)  

10. Let the PS2 idle on this screen for the next steps on the PC.  
![launchelf-ftp-enabled](readme-images/launchelf-ftp-enabled.jpg)

11. Run the sync app, please note that these are example parameters.
```
dotnet UDPBD-for-XEB+-CLI.dll -path /mnt/ps2 -ps2ip 192.168.0.10 -bin2iso
```
12. Unmount the exFAT partition then Start the udpbd-server.
```
sudo umount /mnt/ps2
sudo ./udpbd-server /dev/nvme0n1p6
```
The server needs to be open and running for the entire play session.

13. Launch XEB+ then Play!

To add or remove games, stop the server then mount the exFAT storage device to `/mnt/ps2`  
```
sudo mount /dev/nvme0n1p6 /mnt/ps2/ -o uid=$USER
```
Then add your games and repeat steps 9-13 of the linux setup process.

### HDD

1. Connect your hard drive or SSD to a computer, and format it as an exFAT partition.  

2. Create folders named `CD` and `DVD` on the root of the hard drive.  

3. Rip/copy any PlayStation 2 disc images you wish to load into the folder that corresponds with their original source media.  
All disc images must be in .iso format.  

4. Run `GUI.py` from the included `List Builder` folder, and select PS2 HDD under Drive Type.  

5. Click the Browse button under XEBPLUS Location and navigate to the root of the USB drive containing your XEB+ install.  

6. Click the Browse button under Games Location and navigate to the root of your hard drive.  

7. Click Build List and wait for the process to complete.  

8. Eject both drives from the computer.  
Connect the hard drive / SSD to the PlayStation 2 via the network adapter, and plug the USB drive into either of the front USB ports.  

9. Launch XEB+ on the PS2, and use neutrino Launcher (HDD) to load games from the hard drive.  

Repeat steps 3-9 to add or remove games on the hard drive.

### MX4SIO

1. Connect your SD card to the computer, and format it as an exFAT partition.  

2. Create folders named `CD` and `DVD` on the root of the SD card.  

3. Rip/copy any PlayStation 2 disc images you wish to load into the folder that corresponds with their original source media.  
All disc images must be in .iso format.  

4. Run `GUI.py` from the included `List Builder` folder, and select MX4SIO under Drive Type.  

5. Click the Browse button under XEBPLUS Location and navigate to the root of the USB drive containing your XEB+ install.  

6. Click the Browse button under Games Location and navigate to the root of your SD card.  

7. Click Build List and wait for the process to complete.  

8. Eject both drives from the computer.  
Insert the SD card into an MX4SIO adapter and connect it to memory card slot 2 on the PlayStation 2.  
Plug the USB drive into either of the front USB ports.  

9. Launch XEB+ on the PS2, and use neutrino Launcher (MX4SIO) to load games from the SD card.  

Repeat steps 3-9 to add or remove games on the SD card.

### USB

1. Ensure your USB drive is formatted as an exFAT partition. You will need to reformat it and repeat steps 2-4 if it is not.  

2. Create folders named `CD` and `DVD` on the root of the USB drive.  

3. Rip/copy any PlayStation 2 disc images you wish to load into the folder that corresponds with their original source media.  
All disc images must be in .iso format.  

4. Run `GUI.py` from the included `List Builder` folder, and select USB under Drive Type.  

5. Click the Browse button under XEBPLUS Location and navigate to the root of your USB drive.  
The Games Location will automatically be set to the same folder.  

6. Click Build List and wait for the process to complete.  

7. Eject the USB drive and plug it into either of the front USB ports on the PlayStation 2.  

8. Launch XEB+ on the PS2, and use neutrino Launcher (USB) to load games from the USB drive.  

Repeat steps 3-8 to add or remove games on the USB drive.


### Adding Artwork

The neutrino Launcher plugin can display game-specific artwork in the selection menu.  
This feature uses the same file type and naming conventions as Open PS2 Loader, which allows the plugin to take advantage of existing PS2 artwork libraries designed for OPL.  
 To make use of this feature, the following additional steps are required:

1. Prepare or acquire artwork files with the same file format and naming conventions as those used by OPL.
    * Only background art (*_BG*) and disc icon (*_ICO*) files are used by this plugin.
    * Some recent versions of OPL have added support for 128x128 disc icons. It is strongly recommended to only use the older 64x64 icons.
2. Copy the artwork files you wish to use to `/XEBPLUS/GME/ART` on your USB drive.  
It is recommended that you copy as few files as possible to this directory.
3. Launch XEB+ and select and of the neutrino Launcher plugins.  
If the plugin detects files at `/XEBPLUS/GME/ART`, it will automatically create an artwork cache in the `CFG/neutrinoLauncher` folder.  
This is necessary to maintain a usable level of performance while displaying artwork in the menu.

The caching process can take a long time to complete the first time it runs, potentially up to several hours if you have a very large game library.  
Reducing the number for files in the `ART` folder can speed up this process somewhat.  

Making any changes to the *.list* files in the 'CFG/neutrinoLauncher`, or modifying the cache folder, will trigger a refresh of the artwork cache the next time the plugin is launched.  
A refresh can also be triggered manually from the plugin settings.  
Cache refreshes take much less time than initial creation, so long as the cache folder has not been moved or deleted.

### Usage

### Controls

CROSS -         **confirm / launch game**  
CIRCLE / LEFT - **cancel / close plugin**  
SQUARE -        **open context menu**  
TRIANGLE -      **show favorites / show all**  
DOWN -          **scroll down**  
UP -            **scroll up**  
R1 -            **scroll down 10 items**  
L1 -            **scroll up 10 items**  
R2 -            **jump to next letter**  
L2 -            **jump to previous letter**  
R3 -            **jump to bottom of list**  
L3 -            **jump to top of list**  
SELECT -        **view control map**

### Basic Usage

Launch the XEB+ dashboard on your PlayStation 2 console, and select the appropriate *neutrino Launcher* entry from the menu.  
Wait a moment for the plugin to load. If there are artwork files present, you may need to wait for the artwork cache to build or refresh.  
Select a game from the list to launch it with neutrino, or press back to close the plugin.  
Repeat steps 9-14 of the setup process for your drive type each time you want to add or remove games.

### Game Options

Press SQUARE while in the game selection menu, and the context menu will open.  
Context menu options apply to the currently highlighted game by default, and are as follows:

  * **Add To Favorites** - Adds the current game to the favorites list. If the current game is already in the favorites list, this option will remove it.
  * **Global / Per-Game Settings** - This option toggles between Global and Per-Game settings modes.  
When set to Global Settings, options shown below this one will apply to all games.  
When set to Per-Game Settings, options shown below this one will apply only to the current game.
  * **Enable Boot Logo** - When enabled, the PlayStation 2 logo will be shown on screen when starting a game.  
  This slightly increases the start time.
  * **Enable Debug Colors** - When enabled, a series of colors will flash on screen when starting a game.  
  This can be used to help diagnose games that will not start.
  * **Accurate Reads** - When enabled, the data transfer rate for games will be limited to that of the PlayStation 2 DVD drive.  
  This will increase load times if using a hard drive, but can fix compatibility issues with some titles.
  * **Synchronous Reads** - When enabled, asynchronous (background) loading will not be used.  
  This can affect load times, and fixes compatibility issues with some titles.
  * **Unhook Syscalls** - When enabled, neutrino Syscalls will be removed from memory after starting a game.  
  This fixes compatibility issues with some titles.
  * **Emulate DVD-DL**- When enabled, neutrino will emulate the data structure of a dual-layer DVD.  
  This option is required for DVD9 images that have been modified to fit a DVD5.
  * **Refresh Artwork** - Immediately delete and re-copy cached artwork for the current game.  
  This option is not affected by the current settings mode.

Closing the context menu will automatically save the currently selected options. 

### Favorites

Games can be added to a favorites list for easy organization and quicker access.  

The favorites list is shared between all three drive types, however only titles present on the currently selected drive will be displayed.  

To add a game to the favorites list, open the context menu and select "Add To Favorites", as described in the previous section.  
Repeat this process to remove a game from the favorites list.  
When a game is in the favorites list, it's title will be flanked by "●" on either side.  

To access the favorites list, press TRIANGLE while on the game selection menu.  
Press TRIANGLE again to show all games.  
The plugin will always start up to the all games list, even if it was previously closed while viewing the favorites list.

### Neutrino Launcher settings

This package includes a secondary plugin called *neutrino Launcher Settings*, which is accessible from the far right column of the XEB+ dashboard.  
This contains a settings menu whose options apply to the main neutrino Launcher plugins.  
The available options are as follows:
   
   * **Enable HDD** - When enabled, *neutrino Launcher (HDD)* will be available on the XEB+ dashboard.*
   * **Enable MX4SIO** - When enabled, *neutrino Launcher (MX4SIO)* will be available on the XEB+ dashboard.*
   * **Enable USB** - When enabled, *neutrino Launcher (USB)* will be available on the XEB+ dashboard.*
   * **Disable Artwork** - When enabled, game specific artwork will not be shown on the menu.  
   This can prevent the cache from auto-refreshing in some cases.
  * **Disable Status Messages** - When enabled, the text that appears at the bottom of the screen to indicate loading and other behavior will not be shown.
  * **Disable Fade Effect** - When enabled, background artwork will not fade in/out when the image changes.
  * **Disable Icon Animation** - When enabled, the disc icon for the currently highlighted game will not spin.
  *  **Show Title ID** - When enabled, each game's PlayStation 2 title ID will be shown under it's title.
  *  **Show Media** - When enabled, each game's media type will be shown under it's title.
  * **Refresh Artwork Cache** - Sets artwork cache to refresh next time one of the neutrino Launcher plugins is loaded. 

    *An XEB+ reboot/refresh is required for changes made to this setting to take effect.

Returning to the dashboard will automatically save the currently selected options.

## Integrating With Custom Themes

The three neutrino Launcher plugins and the settings plugin each use their own custom dashboard icon that is not normally present in XEB+.  
Versions of these icons that are visually consistent with the default XEB+ theme are included and will be loaded with each plugin by default.  
Alternate icons will be used instead, if icon files with the correct names are present in the current XEB+ theme folder.  

The filenames each plugin looks for are as follows:

  * *ic_tool_neutrino_hdl.png* - neutrino Luancher (HDD)
  * *ic_tool_neutrino_mx4.png* - neutrino Luancher (MX4SIO)
  * *ic_tool_neutrino_udpbd.png* - neutrino Luancher (UDPBD)
  * *ic_tool_neutrino_usb.png* - neutrino Luancher (USB)
  * *ic_set_neutrino_cfg.png* - neutrino Luancher Settings

If you wish to reference the included versions of these icons, they are located at `XEBPLUS/APPS/neutrinoLauncher/image/`.

## Known Issues and Limitations

  * Artwork cannot be refreshed from the context menu while in the favorites list.
  * The plugin may need to be closed and re-opened for refreshed artwork to be shown.
  * If an artwork cache build / refresh is interrupted, the system may crash the next time the plugin is loaded.  
If this happens, the behavior will persist until the cache is refreshed manually.
  * The *neutrino Launcher* plugins can take up to 30 seconds to load, and may appear to hang if there are a large number of games present.
  * If enabled, the PlayStation 2 boot logo will display incorrectly for games that do not match the console's region.
  * If installed, the PlayStation 2 hard drive will spin up when loading games from MX4SIO.
  * Repeatedly closing and re-opening the *neutrino Launcher* plugin can cause the system to crash.
  * Scrolling through the list quickly may cause corrupted graphics to be displayed briefly.
  * This plugin does not currently support virtual memory cards. Further development / testing is required to implement this feature.

## Credits

Big Thanks to these Devs!  

Alex Parrado & El_isra & Rick Gaiser - udpbd-server - 2023-3-8  
https://github.com/israpps/udpbd-server

awaken1ng - udpbd-vexfat - v0.2.0  
https://github.com/awaken1ng/udpbd-vexfat

Howling Wolf & Chelsea - XtremeEliteBoot+  
https://www.psx-place.com/threads/xtremeeliteboot-s-dashboard-special-xmas-showcase.38959/

Rick Gaiser - neutrino - v1.3.1  
https://github.com/rickgaiser/neutrino

sync-on-luma - neutrinoHDD plugin for XEB+ - forked from v2.1  
https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin
