using UnityEngine;
using UnityFramework_Core.Script;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var _g = GameObject.Find("Main Camera");
        var _s = _g.GetComponent<Initialize>();
        _s.AddScripts();
        _s.InitializeGame();

        var _ok = GameObject.Find("GAMEENV").GetComponent<OldKeyInput>();
        _ok.environment = GameObject.Find("GAMEENV").GetComponent<UnityFramework_Core.Script.Environment>().environment;
        _ok.keys = new System.Collections.Generic.Dictionary<string, KeyEvent>();
        _ok.keys.Add("downA", new KeyEvent() { getKeys = new object[][] { new object[] { KeyCode.A, KeyEvent.keyState.Down } } });
        _ok.Create();
        var _e = _ok.GetComponent<UnityFramework_Core.Script.Environment>();
        _e.environment.eventManager.AddMonitor("downA", (_o) => Debug.Log("AAA"));
    }
}
