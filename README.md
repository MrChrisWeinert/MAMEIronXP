# MAMEIronXP
MAMEIronXP is a cross-platform MAME front-end built in C#/AvaloniaUI.

This is a Work In Progress in the beginning stages of development.
Ideally it will scale across different sized-screens, and function the same across operating systems.

![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Screenshot.jpg?raw=true)

# Getting Started
## Windows
1) Download the MAME binary and extract it to a directory of your choice (e.g. C:\MAME). You'll want to download a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/releases/download/mame0258/mame0258b_64bit.exe
2) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. C:\MAME\snap). https://www.progettosnaps.net/snapshots/
3) Download & install .net 8: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
4) Download MAMEIronXP and extract it to a directory of your choice (e.g. C:\MAMEIronXP).
5) Edit the App.config file and tell it where you extracted MAME.
6) To run MAMEIronXP, open a command prompt/terminal and navigate to your MAMEIronXP directory (C:\MAMEIronXP). Type: ```dotnet run```
## Ubuntu
1) Download the MAME source code and extract it to a directory of your choice (e.g. ~/MAME). You'll want to use a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/archive/refs/tags/mame0258.zip
2) Compile MAME. Follow the directions here: https://docs.mamedev.org/initialsetup/compilingmame.html 

In short, change to your ~/MAME directory and run the following commands:
  -  ```sudo apt-get install git build-essential python3 libsdl2-dev libsdl2-ttf-dev libfontconfig-dev libpulse-dev qtbase5-dev qtbase5-dev-tools qtchooser qt5-qmake```
  -  ```make -j 5``` (you should set this value to the number of CPU cores you have + 1)
  

  If everything goes well, you'll have a working "mame" executable

3) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. C:\MAME\snap). https://www.progettosnaps.net/snapshots/
4) Install .net 8

```sudo apt update && sudo snap install dotnet-sdk --classic```

5) Download MAMEIronXP and extract it to a directory of your choice (e.g. ~/MAMEIronXP)

```git clone https://github.com/MrChrisWeinert/MAMEIronXP.git```

6) Overwrite the default "App.config" file with a version suitable for Linux: ```cp App.config.Linux App.config```
7) Edit the new App.config file and tell it where you extracted MAME.
8) To run MAMEIronXP, open a terminal and navigate to your MAMEIronXP directory (~/MAMEIronXP). Type: ```dotnet run```

# Where did this name come from?
MAME = **M**ultiple **A**rcade **M**achine **E**mulator

Iron = This is a MAME **F**ront-**E**nd ("**Fe**") and Fe is the checmical elemental symbol for Iron.

XP = **X**-**P**latform, or cross-platform