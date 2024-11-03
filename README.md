# MAMEIronXP
MAMEIronXP is a cross-platform MAME front-end built in C#/AvaloniaUI.

This is a Work In Progress in the beginning stages of development.
Ideally it will scale across different sized-screens, and function the same across operating systems.

![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Animation.gif?raw=true)

# Getting Started
## Windows
We assume you already have MAME downloaded/compiled. If you don't, we'll include those steps down below.
1) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. C:\MAME\snap). https://www.progettosnaps.net/snapshots/
2) Download the latest MAMEIronXP release (https://github.com/MrChrisWeinert/MAMEIronXP/releases/download/1.1.0/MAMEIronXP-Win-x64-1.1.0.zip) and unzip it into a directory (e.g. C:\MAMEIronXP)
3) Edit the MAMEIronXP.dll.config configuration file to match your environment (set the location of your MAME folder, etc.)
4) Double-click on MAMEIronXP.exe. If Windows SmartScreen blocks the execution, you'll need to right-click on the executable and check the "Unblock" box.

![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/SmartScreen.png?raw=true)


## Windows - Download MAME
1) Download the MAME binary and extract it to a directory of your choice (e.g. C:\MAME). You'll want to download a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/releases/download/mame0258/mame0258b_64bit.exe

## Ubuntu
We assume you already have MAME downloaded/compiled. If you don't, we'll include those steps down below.
1) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. ~/MAME/snap). https://www.progettosnaps.net/snapshots/
2) Download the latest MAMEIronXP release (https://github.com/MrChrisWeinert/MAMEIronXP/releases/download/1.1.0/MAMEIronXP-Linux-x64-1.1.0.zip) and unzip it into a directory (e.g. ~/MAMEIronXP)
3) Edit the MAMEIronXP.dll.config configuration file to match your environment (set the location of your MAME folder, etc.)
4) Add the execute permission to MAMEIronXP: ```chmod +x MAMEIronXP```
5) Run MAMEIronXP: ```./MAMEIronXP```

## Ubuntu - Download/compile MAME
1) Download the MAME source code and extract it to a directory of your choice (e.g. ~/MAME). You'll want to use a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/archive/refs/tags/mame0258.zip
2) Compile MAME. Follow the directions here: https://docs.mamedev.org/initialsetup/compilingmame.html 

In short, change to your ~/MAME directory and run the following commands:
  -  ```sudo apt-get install git build-essential python3 libsdl2-dev libsdl2-ttf-dev libfontconfig-dev libpulse-dev qtbase5-dev qtbase5-dev-tools qtchooser qt5-qmake```
  -  ```make -j 5``` (you should set this value to the number of CPU cores you have + 1)
 

  If everything goes well, you'll have a working "mame" executable


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


## Known Issues
[Currently limited](https://github.com/MrChrisWeinert/MAMEIronXP/issues/7) to the following resolutions with more on the way:
- 2560x1440
- 1920x1080
- 1600x900


Games list items have a [fixed font size and Favorite icon size](https://github.com/MrChrisWeinert/MAMEIronXP/issues/7).

Does not currently support Wayland [this is an Avalonia limitation as of 11.2]. MAMEIronXP will run on the latest version of Raspbian (tested on Raspberry Pi 5), but you have to disable Wayland first.


# Where did this name come from?
MAME = **M**ultiple **A**rcade **M**achine **E**mulator

Iron = This is a MAME **F**ront-**E**nd ("**Fe**") and Fe is the chemical elemental symbol for Iron.

XP = **X**-**P**latform, or cross-platform
