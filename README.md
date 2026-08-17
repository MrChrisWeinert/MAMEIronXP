# MAMEIronXP
MAMEIronXP is a cross-platform MAME front-end built in C#/AvaloniaUI.
![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Animation.gif?raw=true)

# Getting Started
## Prerequisites
We assume you already have MAME downloaded and installed/compiled. If not, head to the [bottom of this README](#Prerequisites)  for more info.

## Windows
1) Download the latest win-x64 release asset from the Releases page:
https://github.com/MrChrisWeinert/MAMEIronXP/releases and unzip it into a directory (e.g. C:\MAME)
2) Edit `appsettings.json` to match your environment (set `MAME:Directory`, `MAME:Executable`, etc.).
3) Double-click on MAMEIronXP.exe. If Windows SmartScreen blocks the execution, you'll need to right-click on the executable and check the "Unblock" box.

![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/SmartScreen.png?raw=true)

## Ubuntu
1) Download the latest linux x64 release asset from the Releases page:
https://github.com/MrChrisWeinert/MAMEIronXP/releases and unzip it into a directory (e.g. ~/MAME)
2) Edit `appsettings.json` to match your environment (set `MAME:Directory`, `MAME:Executable`, etc.).
3) Add the execute permission to MAMEIronXP: ```chmod +x MAMEIronXP```
4) Run MAMEIronXP: ```./MAMEIronXP```

## macOS (Apple Silicon)
1) Download the latest macOS ARM release asset from the Releases page:
https://github.com/MrChrisWeinert/MAMEIronXP/releases
2) Unzip it into a directory (e.g. `~/MAME/MAMEIronXP`).
3) Add execute permissions: `chmod +x MAMEIronXP`
4) Remove the quarantine flag macOS adds to downloaded files, or Gatekeeper will refuse to run it:
`xattr -d com.apple.quarantine MAMEIronXP`
`xattr -d com.apple.quarantine libAvaloniaNative.dylib`
`xattr -d com.apple.quarantine libHarfBuzzSharp.dylib`
5) Run it from Terminal: `./MAMEIronXP`

# Pre-built Binaries via GitHub Actions
This repository includes an automated release workflow that builds these targets:
- `win-x64`
- `linux-x64`
- `linux-arm64` (Raspberry Pi 5 when using a 64-bit OS)
- `osx-arm64` (Apple Silicon)

The workflow lives at `.github/workflows/release-binaries.yml`.

# MAMEIronXP Controls
## Keyboard
"5" on the keyboard will mark a game as a Favorite and a little Pac-Man icon will show up to the left of a game. The game will show up at the top of the Games list so it's easily accessible. The game will still show up in the list in alphabetic order. Pressing 5 again will unfavorite a game.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/12): make this into a long-press to prevent accidental favorite/unfavorites.

"1" on the keyboard will make a selection (start a game, or make a selection on the Exit menu)

"ESC" or "V" on the keyboard will bring up the Exit menu. Pressing it again will exit out of the Exit menu.

"Up/Down" on the keyboard will scroll the games list.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/13): Implement "acceleration" so you can navigate the list VERY fast when holding down the Up/Down button.


## X-Arcade Tankstick
![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/X-Arcade-Tankstick.png?raw=true)

"5" on the Tankstick will mark a game as a Favorite and a little Pac-Man icon will show up to the left of a game. The game will show up at the top of the Games list so it's easily accessible. The game will still show up in the list in alphabetic order. Pressing 5 again will unfavorite a game.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/12): make this into a long-press to prevent accidental favorite/unfavorites.

"1" on the Tankstick will make a selection (start a game, or make a selection on the Exit menu)

"Red button" at the top right of the tankstick will bring up the Exit menu. Pressing it again will exit out of the Exit menu.

"Up/Down" on the left joystick will scroll the games list.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/13): Implement "acceleration" so you can navigate the list VERY fast when holding down the joystick.


# Known Issues
Wayland support is experimental as of Avalonia 12.1 (previously required disabling Wayland on the Pi). If you hit issues, disabling Wayland and falling back to X11 is still an option.

## Video renderer (`-video`)
The default `MAME:Args` in `appsettings.json` includes `-video bgfx`. BGFX is the one MAME renderer that works well across all three of our release targets (Windows, Raspberry Pi 5/Linux, macOS), which is why it's the default here. A few alternatives depending on your setup:
- **Windows**: `-video d3d` is MAME's native Windows default — slightly lower overhead than bgfx, but no shader-chain (CRT/scanline) support.
- **Raspberry Pi 5 / Wayland**: `-video opengl` is usually a safer, lighter choice than bgfx, since the Pi's GPU has less headroom for bgfx's shader-chain machinery.
- If `-video` is omitted entirely, Linux/macOS fall back to MAME's `soft` (software) renderer, which is noticeably slower.

# Prerequisites
## Download/install MAME

### Windows
1) Download the MAME binary and extract it to a directory of your choice (e.g. C:\MAME). You'll want to download a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/releases/download/mame0258/mame0258b_64bit.exe
2) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. C:\MAME\snap). https://www.progettosnaps.net/snapshots/


### Linux
1) Download the MAME source code and extract it to a directory of your choice (e.g. ~/MAME). You'll want to use a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/archive/refs/tags/mame0258.zip
2) Compile MAME. Follow the directions here: https://docs.mamedev.org/initialsetup/compilingmame.html 

In short, change to your ~/MAME directory and run the following commands:
  -  ```sudo apt-get install git build-essential python3 libsdl2-dev libsdl2-ttf-dev libfontconfig-dev libpulse-dev qtbase5-dev qtbase5-dev-tools qtchooser qt5-qmake```
  -  ```make -j 5``` (you should set this value to the number of CPU cores you have + 1)
 
  If everything goes well, you'll have a working "mame" executable

3) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. ~/MAME/snap). https://www.progettosnaps.net/snapshots/

# Additional tips...
MAMEIronXP was designed to run as a dedicated arcade machine in kiosk mode. The goal is to abstract the "computer" away from the end-user. Therefore, no keyboard/mouse should be required.
That introduces a few complexities, so this is what I do to work around them:
1) Auto-login
    - On Raspbian, this can be configured during installation.
    - On Ubuntu, open up your User settings and flip the slider to auto-login. (Note that changing these settings requires you to "Unlock" and that button will not work in an XRDP session)

    ![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Ubuntu_AutoLogin.png?raw=true)
    - On Windows, Download the [Microsoft Autologon tool](https://learn.microsoft.com/en-us/sysinternals/downloads/autologon) and enter your username/password

    ![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Windows_AutoLogin.png?raw=true)
    
2) Auto-start
    - On Raspbian
      - Edit ```/etc/xdg/lxsession/LXDE-pi/autostart``` and add the following line ```@lxterminal -e bash /home/me/startup.sh```
      - Then create a startup.sh bash script in your home directory with the following 
        ```bash
        #!/bin/bash
        /home/me/MAMEIronXP/MAMEIronXP
        ```

    - On Ubuntu, open up 'Startup Applcations' and add MAMEIronXP.
  
    ![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Ubuntu_AutoStart.png?raw=true)
    
    - On Windows, modify this registry key and point it at your MAMEIronXP executable ```HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Shell```

    ![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Windows_AutoStart.png?raw=true)
3) Shutdown
    - On Raspbian, set the s-bit to enable non-superusers to run shutdown as root: ```sudo chmod a+s /sbin/shutdown```
    - On Ubuntu, set the s-bit to enable non-superusers to run shutdown as root: ```sudo chmod a+s /sbin/shutdown```
    - On Windows nothing special is needed. For locked-down Windows machines (i.e. Server) you'll need to open gpedit.msc and grant access
    
    ![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Windows_Shutdown.png?raw=true)
  
# Where did this name come from?
MAME = **M**ultiple **A**rcade **M**achine **E**mulator

Iron = This is a MAME **F**ront-**E**nd ("**Fe**") and Fe is the chemical elemental symbol for Iron.

XP = **X**-**P**latform, or cross-platform
