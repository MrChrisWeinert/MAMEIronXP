# MAMEIronXP
MAMEIronXP is a cross-platform MAME front-end built in C#/AvaloniaUI.

This is a Work In Progress in the beginning stages of development.
Ideally it will scale across different sized-screens, and function the same across operating systems.



# Prerequisites
- MAME
  - On Windows, you'll have a directory structure similar to this: C:\MAME\mame.exe
  - On Linux, you'll have a directory structure similar to this: /home/username/MAME/mame.exe
  - Tested with version .244 from https://github.com/mamedev/mame/releases/tag/mame0244
- Snapshots
  - Snapshots are game images, usually of gameplay or a title screen
  - You should have a "snap" folder in your root MAME directory, with all snapshot images extracted there like this: C:\MAME\snap\1on1gov.png (Windows) or /home/username/MAME/snap/1on1gov.png (Linux)
  - Tested with full set .260 from https://www.progettosnaps.net/snapshots/
- Roms
  - You should have a "roms" folder in your root MAME directory, with all zip files inside like this: C:\MAME\roms\mspacman.zip (Windows) or /home/username/MAME/roms/mspacman.zip (Linux)
  - Tested with version .244 (merged)

## Where did this name come from?
MAME = **M**ultiple **A**rcade **M**achine **E**mulator

Iron = Iron's chemical elemental symbol "**Fe**" (**F**ront-**E**nd)

XP = **X**-**P**latform, or cross-platform


## Performance Note (Windows)
I like add an Anti-Virus exception so C:\MAME\roms and C:\MAME\snap directories do not get scanned.