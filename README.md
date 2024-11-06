# MAMEIronXP
MAMEIronXP is a cross-platform MAME front-end built in C#/AvaloniaUI.
![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Animation.gif?raw=true)

# Getting Started
## Prerequisites
We assume you already have MAME downloaded and installed/compiled. If not, head to the [bottom of this README](#Prerequisites)  for more info.

## Windows
1) Download the latest MAMEIronXP release (https://github.com/MrChrisWeinert/MAMEIronXP/releases/download/1.1.0/MAMEIronXP-Win-x64-1.1.0.zip) and unzip it into a directory (e.g. C:\MAMEIronXP)
2) Edit the MAMEIronXP.dll.config configuration file to match your environment (set the location of your MAME folder, etc.)
3) Double-click on MAMEIronXP.exe. If Windows SmartScreen blocks the execution, you'll need to right-click on the executable and check the "Unblock" box.

![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/SmartScreen.png?raw=true)

## Ubuntu
1) Download the latest MAMEIronXP release (https://github.com/MrChrisWeinert/MAMEIronXP/releases/download/1.1.0/MAMEIronXP-Linux-x64-1.1.0.zip) and unzip it into a directory (e.g. ~/MAMEIronXP)
2) Edit the MAMEIronXP.dll.config configuration file to match your environment (set the location of your MAME folder, etc.)
3) Add the execute permission to MAMEIronXP: ```chmod +x MAMEIronXP```
4) Run MAMEIronXP: ```./MAMEIronXP```

# MAMEIronXP Controls
## Keyboard
"C" on the keyboard will mark a game as a Favorite and a little Pac-Man icon will show up to the left of a game. The game will show up at the top of the Games list so it's easily accessible. The game will still show up in the list in alphabetic order. Pressing C again will unfavorite a game.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/12): make this into a long-press to prevent accidental favorite/unfavorites.

"1" on the keyboard will make a selection (start a game, or make a selection on the Exit menu)

"ESC" on the keyboard will bring up the Exit menu. Pressing it again will exit out of the Exit menu.

"Up/Down" on the keyboard will scroll the games list.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/13): Implement "acceleration" so you can navigate the list VERY fast when holding down the Up/Down button.


## X-Arcade Tankstick
![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/X-Arcade-Tankstick.png?raw=true)

"C" on the Tankstick will mark a game as a Favorite and a little Pac-Man icon will show up to the left of a game. The game will show up at the top of the Games list so it's easily accessible. The game will still show up in the list in alphabetic order. Pressing C again will unfavorite a game.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/12): make this into a long-press to prevent accidental favorite/unfavorites.

"1" on the Tankstick will make a selection (start a game, or make a selection on the Exit menu)

"Red button" at the top right of the tankstick will bring up the Exit menu. Pressing it again will exit out of the Exit menu.

"Up/Down" on the left joystick will scroll the games list.
[_TODO_](https://github.com/MrChrisWeinert/MAMEIronXP/issues/13): Implement "acceleration" so you can navigate the list VERY fast when holding down the joystick.


# Known Issues
[Currently limited](https://github.com/MrChrisWeinert/MAMEIronXP/issues/7) to the following resolutions with more on the way:
- 2560x1440
- 1920x1080
- 1600x900


Games list items have a [fixed font size and Favorite icon size](https://github.com/MrChrisWeinert/MAMEIronXP/issues/7).

Does not currently support Wayland [this is an Avalonia limitation as of 11.2]. MAMEIronXP will run on the latest version of Raspbian (tested on Raspberry Pi 5), but you have to disable Wayland first.

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
