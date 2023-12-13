This is a CS4096 project.
There are two stages to the AI. The first stage is the roaming stage. During which the ai will either stand guard or follow a guard path.
If it sees the playing, it will chase. It can also transition to a scouting stage. This was implemented using a state machine.
The second stage is the battle stage. It is triggered when an AI can see you for a certain period of time. For testing purposes, the 
user will always have the first turn.
This ai is made using GOAP.

---------------Important Keybinds---------------
WASD - Move the camera
QE - Rotate Camera
Left Click - Select Player
Left Click - Move player
Right Player - View Test path
P - Initiate Fight
TAB - End turn

---------------Important Information---------------
The console is very important. To save time on animation, the console displays information like a DM reading aloud a battle sequence.
Use it as it is your friend for understanding what has happened in a battle. 

The way to initate a battle if you click P is by attacking a goblin. If you initate a battle and they haven't been harmed, they will just
stand guard waiting for the player to walk in front of them.

Uses Unity Version 2022.3.9f1