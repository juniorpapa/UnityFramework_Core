using UnityEngine;

namespace UnityFramework_Core
{
    public class DebugConsole : MonoBehaviour
    {
        private void Start()
        {
            // InputManager
            IInputManager _im = GameObject.Find("InputManager").GetComponent<InputManager>();
            KeyEventBind _keyEventBind = new KeyEventBind()
            {
                name = "mouse0",
                invokeEvent = DEBUGLOD,
                triggerKeys = new Key[][] { new Key[] { new Key() { key = KeyCode.Mouse0, triggerType = 1 } } }
            };
            _im.AddKeyEvent("mouse0", _keyEventBind);
            _im.enableInput = true;

            // ConsoleManager
            IConsoleManager _consoleManager = GameObject.Find("ConsoleManager").GetComponent<ConsoleManager>();
            _consoleManager.consoleList.Add("console_Test", new Console());
            _consoleManager.consoleList["console_Test"].token["help"] = () => { return new Tree_Help(); };
            _consoleManager.consoleList["console_Test"].Execute("help");

            // ResourceManager
        }

        private void DEBUGLOD()
        {
            Debug.Log(">>mouse0<<");
        }
    }
}