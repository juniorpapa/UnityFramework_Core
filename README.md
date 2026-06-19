# UnityFramework_Core
UnityFramework_Core 1.0
This is the core of a Unity game framework, written in Unity 6.4 LTS.
The project includes a built-in plugin manager, resource manager, input manager, and a simple debug script for demonstration purposes.
All test scripts are located in the 'Asset/Test' directory, while the core code is in 'Asset/Script/Core'.
>>The project code has not been optimized in any way and can only run normally; it is for learning and communication purposes only

Namespace 'UnityFramework_Core'
by Papa

Project Resources:
Name:					          Purpose:
Console Manager:√		    Provides a simple console for the game. (ConsoleManager)
Resource Manager:√		  Manages game resources. (ResourceManager)
Input Manager:√		      Handles player input. (InputManager)

Naming conventions in the project:
Type:		    Rule:			            Example:
Constants		ALL UPPERCASE		      GLOBAL
Static		  Prefix s_			        s_lilium
Method		  PascalCase		        AddNumber
Type		    PascalCase		        GameObject
Interface		Prefix I			        IGamePool
Member		  camelCase		          objectName
Private		  Prefix and suffix _		_gameTick_
Local		    Prefix _			        _name
