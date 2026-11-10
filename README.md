# UnityFramework_Core v1.5

v1.5 merges the original `juniorpapa/UnityFramework_Core2.0` into this project by overwriting it with the latest 2.0 files and deleting the old `UnityFramework_Core2.0` project.

`OldKeyInput` has been modified to make the code easier to extend and decouple. `NewKeyInput` has been temporarily removed. Some issues in other files have also been fixed.

The code is located under `Core/Script`. The folder name is misspelled as `Scrpit` (missing an “s”); I only noticed it recently and have not renamed it.

> This project is intended for learning and exchange only. It is not recommended for production use.

Core             Classes
Class	           Purpose
EventManager	   Stores and manages in-game events.
ResourceManager	 Stores and manages in-game resources.
ScriptManager	   Manages C#/sandbox scripts.
Environment	     Stores scripts in the game that do not inherit from Mono.
GameLog	         Game log.
ConsoleWindow	   Console window.
Initialize	     nitializes the game.
OldKeyInput	     Keybinding system based on the old input system.
Storage	         Stores data locally.
GameComponent	   All in-game object properties and related data are composed of Unity-like components; all components are under GameComponent.
HotFixActuator	 Performs game hot updates.
NewKeyInput	     Keybinding system based on the new input system. Temporarily removed.
