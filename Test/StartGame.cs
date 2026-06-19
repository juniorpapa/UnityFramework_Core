using UnityEngine;

namespace UnityFramework_Core
{
    public class StartGame : MonoBehaviour
    {
        private void Start()
        {
            CreateCoreObject();
        }

        private void CreateCoreObject()
        {
            GameObject _core = new GameObject();
            GameObject _debugConsole = new GameObject();
            GameObject _resourceManager = new GameObject();
            GameObject _inputManager = new GameObject();
            GameObject _consoleManager = new GameObject();

            _core.name = "Core";
            _debugConsole.name = "DebugConsole";
            _resourceManager.name = "ResourceManager";
            _inputManager.name = "InputManager";
            _consoleManager.name = "ConsoleManager";

            _resourceManager.AddComponent<ResourceManager>();
            _inputManager.AddComponent<InputManager>();
            _debugConsole.AddComponent<DebugConsole>();
            _consoleManager.AddComponent<ConsoleManager>();

            _resourceManager.transform.SetParent(_core.transform);
            _debugConsole.transform.SetParent(_core.transform);
            _inputManager.transform.SetParent(_core.transform);
            _consoleManager.transform.SetParent(_core.transform);

            DontDestroyOnLoad(_core);
        }
    }
}