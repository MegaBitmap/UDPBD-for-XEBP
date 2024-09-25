
# UDPBD plugin for XtremeEliteBoot+

This is a plugin for the Xtreme Elite Boot Plus dashboard for the PlayStation 2.  
It allows XEB+ to load PlayStation 2 games from a PC via [neutrino](https://github.com/rickgaiser/neutrino).  
It is bundled with a windows app `UDPBD-for-XEB+.exe` to automate the setup process.

## Features

- Load PlayStation 2 game backups straight from the Xtreme Elite Boot Plus dashboard.
- Two different editions:
	- Standard edition for quick and easy setup. (udpbd-vexfat)
	- VMC edition with support for virtual memory cards. (udpbd-server)
- Quick navigation functions for browsing large game lists.
- Remember last played game.
- Display game-specific artwork on the menu.
- Set neutrino compatibility flags and other options on a global or per-game basis.
- Favorites list.

## Requirements

- A PlayStation 2 console configured to run unsigned code via an exploit such as FreeMCBoot or PS2BBL.
- LaunchELF v4.43x_isr (bundled with PS2BBL) or LaunchELF v4.43a 41e4ebe (bundled with FMCB v1.966)
- A PlayStation 2 slim with built in Ethernet or a fat model with an OEM network adapter.
- A MBR FAT32 formatted USB drive that is ready to be erased.  
This is used for XEB+ and assorted files. Game ISOs are stored on the PC.
- A Windows PC with enough free space to store game ISOs.
- An ethernet cable to connect the PS2 to the PC.  
If your PC does not have an ethernet port available, a USB ethernet adapter will work.  

## Standard Edition or VMC Edition

In the [latest release](https://github.com/MegaBitmap/UDPBD-for-XEBP/releases) there are two different editions to choose from.  
The standard edition uses the udpbd-vexfat server which supports windows NTFS and exFAT partitions.
This means that game ISOs can be put almost anywhere on your main C: drive or any secondary drive.
The limitation of this edition is that virtual memory cards (VMCs) are not supported.  

The VMC edition uses the original udpbd-server built for windows and includes support for virtual memory cards.
This edition requires extra steps to setup as it only allows games to be stored on an exFAT partition specifically created with GParted/Linux.  

### The Following Steps are needed for the VMC Edition ONLY!

1. Open Disk Management and select a drive to store PS2 game ISOs.  

2. Shrink or delete a partition so that there is enough unallocated space to store games.  
WARNING Please back up any important files before deleting a partition.  

3. Use [rufus](https://rufus.ie/en/) to flash a [GParted live ISO](https://gparted.org/download.php) onto a flash drive.  

4. Restart the PC and enter the BIOS.  
In boot options, select the flash drive with GParted and boot from it.  

5. Select the default options with the enter key to start Gparted.  

6. Select the unallocated space in the drive of choice and create a new exFAT partition.  
It is recommended to label it as PS2.  

7. Apply the changes, then restart back into windows.  

8. Navigate to `This PC` and an empty drive labeled PS2 should show up.  
Please note the drive letter for step 19.  

9. In the drive create a folder named `DVD` and copy all game ISOs into it.  
For games in a CUE+BIN format, create a folder named `CD` and copy the files into it.

10. Now continue with the steps below.
The VMC edition of the sync app is slightly different but mostly the same.

## Setup

1. Use [rufus](https://rufus.ie/en/) to format the USB flash drive with settings as shown:  
![mbr-fat32-usb-drive](./images/mbr-fat32-usb-drive.jpg)
2. Download the Xtreme Elite Boot Plus Xmas 2021 showcase [here](https://web.archive.org/web/*/hwc.nat.cu/ps2-vault/hwc-projects/xebplus).  
3. Use [7-zip](https://7-zip.org/) to extract the `XEBPLUS` folder to the root of your USB drive. (Windows does not natively support `.rar` with passwords.)
The password is at the bottom of `xebplus_xmas_showcase.rar\Distibution License.txt`.
4. Download the [latest release on this page](https://github.com/MegaBitmap/UDPBD-for-XEBP/releases).
This will include the UDPBD plugin bundled with neutrino.
5. Extract the `XEBPLUS` folder and merge into the root of the USB drive.
6. Somewhere on your PC, example `Documents`, create a new folder `PS2`.
7. Inside the `PS2` folder, create `CD` and `DVD` folders.
8. Rip/copy any PlayStation 2 ISOs you wish to load into the folder that corresponds with their original source media.
9. All `.bin` + `.cue` games must be converted to `.iso` format.
[OPL manager](https://www.psx-place.com/resources/opl-manager-tool-to-manage-your-games.663/) will do this conversion.
10. Plug in the ethernet cable as shown:  
![ps2-slim-connected-to-laptop](images/ps2-slim-connected-to-laptop.jpg)
Please note that it is possible to use a router but that is not covered in this guide.
11. On the PC -> Settings -> Network -> Ethernet -> Use the following settings:  
![pc-ip-settings](images/pc-ip-settings.jpg)
12. Plug the USB flash drive from step 5 into the PS2.
13. Power on the PS2 while holding `R1` on the controller. In both freeMCboot and PS2BBL this opens launchELF.
14. In launchELF go to MISC:  
![launchelf-misc](images/launchelf-misc.jpg)
15. Run PS2Net.  
![launchelf-ps2net](images/launchelf-ps2net.jpg)
16. Once you reach this screen, you are ready for the next steps on the PC.  
![launchelf-ftp-enabled](images/launchelf-ftp-enabled.jpg)
17. From the [latest release on this page](https://github.com/MegaBitmap/UDPBD-for-XEBP/releases), extract the `UDPBD-for-XEB+` folder.
18. Run `UDPBD-for-XEB+.exe` and click connect.  
![udpbd-xeb-1](images/udpbd-xeb-1.jpg)
19. Click `Select Game Path` then choose an ISO from the `DVD` folder from step 8.
20. Click `Sync with PS2`, upon success this message will be displayed:  
![udpbd-xeb-synced](images/udpbd-xeb-synced.jpg)
21. Click `Start Server` and make sure to allow.  
![udpbd-vexfat-firewall](images/udpbd-vexfat-firewall.jpg)  
If you miss clicked, either move the `UDPBD-for-XEB+` folder inside a new folder or manually delete the inbound rules for `udpbd-vexfat` in `Windows Defender Firewall with Advanced Security`.  
udpbd-vexfat will open minimized.
The server needs to be open and running for the entire play session. (Disable sleep on the PC.)
22. The PC setup is now complete, back on the PS2 run XEB+.  
![launchelf-xeb](images/launchelf-xeb.jpg)
It's recommended to set freeMCboot or PS2BBL to autorun the `.ELF` file.
23. Play!  
![xeb-game-list](images/xeb-game-list.jpg)



## Add/Remove Games
Repeat steps 13-23 of the setup process each time you add or remove games.

## Adding Artwork
The Neutrino UDPBD Loader plugin can display game-specific artwork in the selection menu.  
This feature uses the same file type and naming conventions as Open PS2 Loader,  
which allows the plugin to take advantage of existing PS2 artwork libraries designed for OPL.  
Note that only background (*_BG*) and disc art (*_ICO*) files are used by this plugin.  

To make use of this feature, check the box `Download Artwork` before clicking sync.  
This will download artwork from the [OPLM art backup on archive.org](https://oplmanager.com/site/?backups).  

If the plugin detects files at `/XEBPLUS/GME/ART`, it will automatically create an artwork cache in the plugin folder.  
This is necessary to maintain a usable level of performance while displaying artwork in the menu.  
The caching process can take a long time to complete the first time it runs,  
potentially up to several hours if you have a very large game library.  
Reducing the number for files in the `ART` folder can speed up this process somewhat.  

Changing the contents of the `CD` or `DVD` folders on the USB drive,  
or modifying the cache folder, will trigger a refresh of the artwork cache the next time the plugin is launched.  
A refresh can also be triggered manually from the plugin settings.  
Cache refreshes take much less time than initial creation, so long as the cache folder has not been moved or deleted.

<h2>Usage</h2>
<h3>Controls</h3>
CROSS -         <b>confirm / launch game</b><br>
CIRCLE / LEFT - <b>cancel / close plugin</b><br>
SQUARE -        <b>open context menu</b><br>
TRIANGLE -      <b>show favorites / show all</b><br>
DOWN -          <b>scroll down</b><br>
UP -            <b>scroll up</b><br>
R1 -            <b>scroll down 10 items</b><br>
L1 -            <b>scroll up 10 items</b><br>
R2 -            <b>jump to next letter</b><br>
L2 -            <b>jump to previous letter</b><br>
R3 -            <b>jump to bottom of list</b><br>
L3 -            <b>jump to top of list</b><br>
SELECT -        <b>view control map</b>
<h3>Basic Usage</h3>

Launch the XEB+ dashboard on your PlayStation 2 console, and select *Neutrino Launcher (UDPBD)* from the menu.  
Wait a moment for the plugin to load.  
If there are artwork files present, you may need to wait for the artwork cache to build or refresh.  
Select a game from the list to launch it with Neutrino, or press back to close the plugin.    

## Game Options
Press SQUARE while in the game selection menu, and the context menu will open.  
Context menu options apply to the currently highlighted game by default, and are as follows:

* **Add To Favorites** - Adds the current game to the favorites list.
If the current game is already in the favorites list, this option will remove it.  

* **Global / Per-Game Settings** - This option toggles between Global and Per-Game settings modes.
When set to Global Settings, options shown below this one will apply to all games.
When set to Per-Game Settings, options shown below this one will apply only to the current game.

* **Enable Boot Logo** - When enabled, the PlayStation 2 logo will be shown on screen when starting a game.
This slightly increases the start time.

* **Enable Debug Colors** - When enabled, a series of colors will flash on screen when starting a game.
This can be used to help diagnose games that will not start.

* **Accurate Reads** - When enabled, the data transfer rate for games will be limited to that of the PlayStation 2 DVD drive.
This will increase load times, but can fix compatibility issues with some titles.

* **Synchronous Reads** - When enabled, asynchronous (background) loading will not be used.
This can affect load times, and fixes compatibility issues with some titles.

* **Unhook Syscalls** - When enabled, neutrino Syscalls will be removed from memory after starting a game.
This fixes compatibility issues with some titles.

* **Emulate DVD-DL**- When enabled, neutrino will emulate the data structure of a dual-layer DVD.
This option is required for DVD9 images that have been modified to fit a DVD5.

* **Refresh Artwork** - Immediately delete and re-copy cached artwork for the current game.
This option is only available for games with a valid title ID in their filename, and is not affected by the current settings mode.

Closing the context menu will automatically save the currently selected options. 

## Favorites
Games can be added to a favorites list for easy organization and quicker access.  
To add a game to the favorites list, open the context menu and select "Add To Favorites", as described in the previous section.  
Repeat this process to remove a game from the favorites list.  
When a game is in he favorites list, the word "Favorite" will be show below it's file name on the game selection menu.  
To access the favorites list, press TRIANGLE while on the game selection menu.  
Press TRIANGLE again to show all games.  
The plugin will always start up to the all games list, even if it was previously closed while viewing the favorites list.

## Neutrino UDPBD Loader Settings
This package includes a secondary plugin called <i>neutrino Launcher (UDPBD) Settings</i>,
which is accessible from the far right column of the XEB+ dashboard.  
This contains a settings menu whose options apply to the main neutrino Loader plugin.  
The available options are as follows:

  * **Disable Artwork** - When enabled, game specific artwork will not be shown on the menu. This can prevent the cache from auto-refreshing in some cases.
  * **Disable Status Messages** - When enabled, the text that appears at the bottom of the screen to indicate loading and other behavior will not be shown.
  * **Disable Icon Animation** - When enabled, the disc icon for the current game will not spin
  * **Refresh Artwork Cache** - Sets artwork cache to refresh next time the main plugin is loaded

## Known Issues and Limitations

  * Artwork cannot be refreshed from the context menu while in the favorites list.
  * If an artwork cache build / refresh is interrupted, the system may crash the next time the plugin is loaded.  
If this happens, the behavior will persist until the cache is refreshed manually.
  * The neutrino Loader plugin can take up to 30 seconds to load, and may appear to hang if there are a large number of games present.
  * If enabled, the PlayStation 2 boot logo will display incorrectly for games that do not match the console's region.
  * Repeatedly closing and re-opening the Neutrino Loader plugin can cause the system to crash.
  * Scrolling through the list quickly may cause corrupted graphics to be displayed briefly.

## Compiling the Sync App

If you wish to make any modifications to the windows presentation framework (wpf) application follow these steps:
1. Download and install Visual Studio Community with the `.NET desktop development` workload.
2. Clone this repository.
3. Open the `Sync-App-Source\UDPBD-for-XEB+.sln` file with Visual Studio.
4. Edit `MainWindow.xaml` to change the appearance, or edit `MainWindow.xaml.cs` to change the functionality.
5. Click Build -> Build Solution, then copy the files from `Sync-App-Source\needed-for-release` to `Sync-App-Source\UDPBD-for-XEB+\bin\Debug\net8.0-windows`  
This only needs to be done once.
6. Now you can test your changes by clicking the green arrow in VS or open `Sync-App-Source\UDPBD-for-XEB+\bin\Debug\net8.0-windows\UDPBD-for-XEB+.exe`

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

sync-on-luma - neutrinoHDD plugin for XEB+ - forked from v1.0.2  
https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin
