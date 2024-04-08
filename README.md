<br/>
<p align="center">
  <a href="https://github.com/Fozkais/CMS21-Together">
    <img src="https://cdn.discordapp.com/icons/1076513862897119232/1150ebb5d3f306736e1a1cd080465b82.webp?size=96" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">CMS21 Together</h3>

  <p align="center">
    A work in progress multiplayer mod for Car Mechanic Simulator 2021
    <br/>
    <br/>
    <a href="https://discord.gg/rxnXWGCES9">Report Bug</a>
  </p>
</p>

![Forks](https://img.shields.io/github/forks/Fozkais/CMS21-Together?style=social) ![Stargazers](https://img.shields.io/github/stars/Fozkais/CMS21-Together?style=social) ![License](https://img.shields.io/github/license/Fozkais/CMS21-Together) 

## Table Of Contents

* [About the Project](#about-the-project)
* [How it Works](#how-it-work)
*  [License](#license)
* [Authors](#authors)

## About The Project

Who never dreamed playing CMS with some friends?  Since it's not in the base game I'll add it myself ! :)

**In its current state, the mod is EXPERIMENTAL, many features are missing, others are buggy and there are certainly many bugs and features that could break the game, use at your own risk.**

*Here's every planned feature state :*

* Implement Networking System (TCP/UDP) : ✅
* Implement Steam API Networking : ❌
* Create a Lobby system : ✅
* Add a Custom Save System : ✅
* Sync players Position and Rotation : ✅
* Sync Inventory : ✅ 
* Sync Cars Spawning and position : ✅
* Sync Garage Interaction : ✅ (Lifter, Wheel Assembler and Wheel Balancer)
* Sync OutDoor Interaction : 🚧 (Car Painting, Car Wash,Power bench and wheel alignment)
* Sync Cars "mechanic" : ✅ 
* Sync Stats (Money,exp,scrap) : ✅
* Sync Quest : ❌
* Sync garage upgrade : ❌
* Sync garage Customization : ❌
* Add Mod Support : ❌
* Add Animation to players : ✅

**Here the description of every emote :**
<br/>
✅: Done / Implemented 
<br/>
🚧: W.I.P / Work In Progress
<br/>
❌: To Do / Missing
<br/>
⏸️: "Paused" / Some new can appear but it's not the main focus.
<br/>

You can join the project Discord if you want to talk or ask for help : 
https://discord.gg/rMz4tGbrc6
## How it work

I'll try to be brief,
<br/>
First of all i dont know if it work on Linux or MacOs i'm on windows and do not plan to port it to other so maybe it work , dont know. 
<br/>
Once you've compiled the mod and obtained its DLL (CMS21MP.dll) you'll need to install MelonLoader version 6.1 on your CMS then add it in the mod folder which should be present in the game directory, 
<br/>
with that you need 2 other file you can find on the mod Discord, those are the player Model and texture wich you'll need to put inside the folder Mods in the subfolder "togetherMod" (create it if he wont exist)
<br/>

From this point on, you should be able to launch the game with the mod, once launched you should be able to open the mod's menu with the Right Shift key, 
<br/>
from there you'll be able to launch a game and join one (you have to launch/join from the main menu, not in-game). 
if there's no issue with the save system it should create a new save and create a lobby that 4 player max can join,
if they are all ready you can start the game and start playing (in the case where everything work)

## Authors

* **Fozkais** - *Main dev* - [Fozkais](https://github.com/Fozkais)
*  **Meitzi** - *Huge Contributor* - [MetziQ](https://www.nexusmods.com/carmechanicsimulator2021/users/151281813)

## License

Distributed under the MIT License. See [LICENSE](https://github.com/Fozkais/CMS21-Together/blob/MainMod/LICENSE) for more information.
