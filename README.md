# StarMap

A POC/Prototype arbitrary code modloader for Kitten Space Agency.  
The idea is to get to a Factorio like experience where mods can be managed in game and mods can be synced by restarting the game.  
Currently this loader can be ran with this functionality in the background, or as a dumb loader just loading mods.  
It makes use of Assembly Load Contexts to ensure mod dependencies are managed seperatly, reducing conflicts

## Installation

-   Download and unzip release from [Releases](https://github.com/StarMapLoader/StarMap/releases/latest).
-   Run StarMapLoader.exe, this will fail and create a StarMapLoader.json.
-   Open StarMapLoader.json and set the location of your KSA installation.
-   Run StarMapLoader.exe again, this should launch KSA and load your mods.

## Running as dumb loader

If you do not want the seperate process and mod manager functionality, StarMap.exe can also be ran seperatly, this will just load the installed mods

## Mod location

Mods should be installed in the mods folder in the KSA installation, any KSA mod that has a dll with the same name as the mod that impelements the IStarMapMod interface will be loaded.

## Mod creation

For more information on mod creation, check out the example mods: [StarMap-ExampleMods](https://github.com/StarMapLoader/StarMap-ExampleMods).

## Credits

-   Lexi - [KSALoader](https://github.com/cheese3660/KsaLoader)
