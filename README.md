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

Here's the procedure to compile it youself :<br/>
<br/>
1- Go to the github page on the branch "MainMod"<br/>
2- Download the source code as ZIP and extract it<br/>
3- Download a code editor : VS Community or Jetbrain Rider<br/>
4- Once installed open the project with one of these editor <br/>
<br/>
From there it change depending the version of the mod you've downloaded.<br/>
CASE 1 - 0.2.8 and Later<br/>
5- Download and Install MelonLoader on version 0.6.1<br/>
6- Open the game 1 time until main Menu and close it<br/>
7- Go back to code editor, press Ctrl + B to compile (maybe not working on VS Community)<br/>
8- Go Copy the "CMS21-Together.dll" located inside bin/debug folder from project directory<br/>
9- Paste the mod dll into mod folder in game directory<br/>
<br/>
CASE 2 - Older than 0.2.8<br/>
5- You need to download and Install MelonLoader on version 0.6.1<br/>
6- Open the game 1 time until main Menu and close it<br/>
7- Once you done that , go back to you code editor , it'll have a lot of error because of the dependencies missing<br/>
8- Delete All existing dependencies except System and System.xxxxx<br/>
9- Add all required dependencies listed below: (you can find them either in net6 or IL2CppAssemblies folders)<br/>
10- Once done , there should no longer have any error , press Ctrl + B to compile<br/>
11- Go Copy the "CMS21-Together.dll" located inside bin/debug folder from project directory<br/>
12- You'll also need to past 2 additional dll on userLibs, you can find them on github page<br/>
13- Paste the mod dll into mod folder in game directory<br/>
<br/>
Then your all set :) just launch the game and everything should work .<br/>
NOTE: for those using mod before 0.3.0 to open the mod menu it's "Right Shift"<br/>

From this point on, you should be able to launch the game with the mod, once launched you should be able to open the mod's menu with the Right Shift key, <br/>
<br/>
from there you'll be able to launch a game and join one (you have to launch/join from the main menu, not in-game). <br/>
if there's no issue with the save system it should create a new save and create a lobby that 4 player max can join,<br/>
if they are all ready you can start the game and start playing (in the case where everything work)<br/>

## Authors

* **Fozkais** - *Main dev* - [Fozkais](https://github.com/Fozkais)
*  **Meitzi** - *Huge Contributor* - [MetziQ](https://www.nexusmods.com/carmechanicsimulator2021/users/151281813)

## License

Distributed under the MIT License. See [LICENSE](https://github.com/Fozkais/CMS21-Together/blob/MainMod/LICENSE) for more information.
