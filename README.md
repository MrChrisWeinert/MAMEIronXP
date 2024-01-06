# MAMEIronXP
MAMEIronXP is a cross-platform MAME front-end built in C#/AvaloniaUI.

This is a Work In Progress in the beginning stages of development.
Ideally it will scale across different sized-screens, and function the same across operating systems.

![screenshot](https://github.com/MrChrisWeinert/MAMEIronXP/blob/main/Assets/Animation.gif?raw=true)

# Getting Started
## Windows
1) Download the MAME binary and extract it to a directory of your choice (e.g. C:\MAME). You'll want to download a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/releases/download/mame0258/mame0258b_64bit.exe
2) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. C:\MAME\snap). https://www.progettosnaps.net/snapshots/
3) Download & install the .NET 8 SDK (not runtime): https://dotnet.microsoft.com/en-us/download/dotnet/8.0
4) Download MAMEIronXP and extract it to a directory of your choice (e.g. C:\MAMEIronXP).
5) Edit the App.config file and tell it where you extracted MAME.
6) To build MAMEIronXP, open a command prompt/terminal and navigate to your MAMEIronXP directory (C:\MAMEIronXP). Type: ```dotnet build```
7) To run MAMEIronXP, navigate to the output directory (C:\MAMEIronXP\bin\Debug\net8.0) and double-click one MAMEIronXP executable, or at a command prompt/terminal type: ```dotnet run```
## Ubuntu**
**.NET 8 is a pain to install on older Ubuntu distributions because .NET 8 isn't in Ubuntu's package management system (apt). The "snap" install is quirky and doesn't install all the necessary dependencies. Therefore I'd recommend using Ubuntu 23.10 that version *does* include .NET 8 in the apt repositories.
1) Download the MAME source code and extract it to a directory of your choice (e.g. ~/MAME). You'll want to use a version of MAME that matches the version of your roms. I'm using version .258
https://github.com/mamedev/mame/archive/refs/tags/mame0258.zip
2) Compile MAME. Follow the directions here: https://docs.mamedev.org/initialsetup/compilingmame.html 

In short, change to your ~/MAME directory and run the following commands:
  -  ```sudo apt-get install git build-essential python3 libsdl2-dev libsdl2-ttf-dev libfontconfig-dev libpulse-dev qtbase5-dev qtbase5-dev-tools qtchooser qt5-qmake```
  -  ```make -j 5``` (you should set this value to the number of CPU cores you have + 1)
  

  If everything goes well, you'll have a working "mame" executable

3) Download a full set of Snapshots and extract them (just the .png files) to your MAME "snap" directory (e.g. C:\MAME\snap). https://www.progettosnaps.net/snapshots/
4) Install .NET 8

```sudo apt update && sudo apt install dotnet-sdk-8.0```

5) Download MAMEIronXP and extract it to a directory of your choice (e.g. ~/MAMEIronXP)

```git clone https://github.com/MrChrisWeinert/MAMEIronXP.git```

6) Overwrite the default "App.config" file with a version suitable for Linux: ```cp App.config.Linux App.config```
7) Edit the new App.config file and tell it where you extracted MAME.
8) To build MAMEIronXP, open a terminal and navigate to your MAMEIronXP directory (~/MAMEIronXP). Type: ```dotnet build```
9) To run MAMEIronXP after building, you run it like any other executable ```~/MAMEIronXP/bin/Debut/net8.0/MAMEIronXP```

# Where did this name come from?
MAME = **M**ultiple **A**rcade **M**achine **E**mulator

Iron = This is a MAME **F**ront-**E**nd ("**Fe**") and Fe is the chemical elemental symbol for Iron.

XP = **X**-**P**latform, or cross-platform
