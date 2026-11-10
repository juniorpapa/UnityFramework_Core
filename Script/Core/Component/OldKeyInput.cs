using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework_Core.Script
{
    public interface IKeyInput : IGameScript
    {
        public UnityFramework_Core.Environment environment { get; set; }
        public Dictionary<string, KeyEvent> keys { get; set; }
        public void Clear();
        public void Create();
    }
    public class OldKeyInput : MonoBehaviour, IKeyInput
    {
        public string scriptName { get; set; } = "OldKeyInput";
        public bool isMonoBehaviour { get; set; } = true;

        public UnityFramework_Core.Environment environment { get; set; }
        public Dictionary<string, KeyEvent> keys { get; set; }
        public void Clear()
        {
            if (environment == null || environment.eventManager == null) return;
            foreach (var _kvp in keys)
            {
                environment.eventManager.RemoveEvent(_kvp.Key);
            }
        }
        public void Create()
        {
            if (environment == null) return;
            foreach (var _kvp in keys) environment.eventManager.CreateEvent(_kvp.Key, out _);
        }

        private List<(string, object)> _cacheFixedUpdate_ = new List<(string, object)> ();
        private List<(string, object)> _cacheLateUpdate_ = new List<(string, object)> ();
        public readonly Dictionary<string, System.Func<object, object>> r_map = new Dictionary<string, System.Func<object, object>>() {
            ["mousePos"] = (_o) => { return Input.mousePosition; },
            ["mousePosDelta"] = (_o) => { return Input.mousePositionDelta; },
            ["mouseMove"] = (_o) => { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); },
            ["mouseMoveRaw"] = (_o) => { return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")); },
            ["mouseScrollDelta"] = (_o) => { return Input.mouseScrollDelta; },
            ["mouseButton0"] = (_o) => 
            {
                if (_o is KeyEvent.keyState) 
                switch ((KeyEvent.keyState)_o)
                {
                    case KeyEvent.keyState.Down:
                        if (Input.GetMouseButtonDown(0)) return null;
                        break;
                    case KeyEvent.keyState.Up:
                        if (Input.GetMouseButtonUp(0)) return null;
                            break;
                    case KeyEvent.keyState.Get:
                        if (Input.GetMouseButton(0)) return null;
                        break;
                    default: return false;
                }
                return false;
            },
            ["mouseButton1"] = (_o) =>
            {
                if (_o is KeyEvent.keyState)
                    switch ((KeyEvent.keyState)_o)
                    {
                        case KeyEvent.keyState.Down:
                            if (Input.GetMouseButtonDown(1)) return null;
                            break;
                        case KeyEvent.keyState.Up:
                            if (Input.GetMouseButtonUp(1)) return null;
                            break;
                        case KeyEvent.keyState.Get:
                            if (Input.GetMouseButton(1)) return null;
                            break;
                        default: return false;
                    }
                return false;
            },
            ["mouseButton2"] = (_o) =>
            {
                if (_o is KeyEvent.keyState)
                    switch ((KeyEvent.keyState)_o)
                    {
                        case KeyEvent.keyState.Down:
                            if (Input.GetMouseButtonDown(2)) return null;
                            break;
                        case KeyEvent.keyState.Up:
                            if (Input.GetMouseButtonUp(2)) return null;
                            break;
                        case KeyEvent.keyState.Get:
                            if (Input.GetMouseButton(2)) return null;
                            break;
                        default: return false;
                    }
                return false;
            },
            ["mouseButton3"] = (_o) =>
            {
                if (_o is KeyEvent.keyState)
                    switch ((KeyEvent.keyState)_o)
                    {
                        case KeyEvent.keyState.Down:
                            if (Input.GetMouseButtonDown(3)) return null;
                            break;
                        case KeyEvent.keyState.Up:
                            if (Input.GetMouseButtonUp(3)) return null;
                            break;
                        case KeyEvent.keyState.Get:
                            if (Input.GetMouseButton(3)) return null;
                            break;
                        default: return false;
                    }
                return false;
            },
            ["mouseButton4"] = (_o) =>
            {
                if (_o is KeyEvent.keyState)
                    switch ((KeyEvent.keyState)_o)
                    {
                        case KeyEvent.keyState.Down:
                            if (Input.GetMouseButtonDown(4)) return null;
                            break;
                        case KeyEvent.keyState.Up:
                            if (Input.GetMouseButtonUp(4)) return null;
                            break;
                        case KeyEvent.keyState.Get:
                            if (Input.GetMouseButton(4)) return null;
                            break;
                        default: return false;
                    }
                return false;
            }, };
        private object Check(object[][] _getKeys)
        {
            foreach (var _kvp in _getKeys)
            {
                if (_kvp.Length <= 0 || _kvp[0] == null) return false;
                if (_kvp[0] is string && r_map.TryGetValue(_kvp[0] as string, out var _v)) return _v?.Invoke(_kvp.Length <= 1 || _kvp[1] == null? null : _kvp[1]);
                else if (_kvp[0] is KeyCode)
                {
                    if (_kvp.Length <= 1 || _kvp[1] == null || !(_kvp[1] is KeyEvent.keyState)) return false;
                    switch (_kvp[1])
                    {
                        case KeyEvent.keyState.Down:
                            if (!Input.GetKeyDown((KeyCode)_kvp[0])) return false;
                            break;
                        case KeyEvent.keyState.Up:
                            if (!Input.GetKeyUp((KeyCode)_kvp[0])) return false;
                            break;
                        case KeyEvent.keyState.Get:
                            if (!Input.GetKey((KeyCode)_kvp[0])) return false;
                            break;
                        default: return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return null;
        }
        private void Execute(string _name, KeyEvent _keyEvent)
        {
            if (_keyEvent == null || _keyEvent.getKeys == null) return;
            var _r = Check(_keyEvent.getKeys);
            if (_r is bool && (bool)_r == false) return;
            if (_keyEvent.nonKeys != null)
                foreach (var _k in _keyEvent.nonKeys)
                {
                    if (_k != null && _k.getKeys != null && Check(_k.getKeys) is bool _ok && _ok != false)
                        return;
                }
            switch (_keyEvent.callTime)
            {
                case KeyEvent.Update.Update:
                    environment.eventManager.InvokeEvent(_name, _r);
                    break;
                case KeyEvent.Update.FixedUpdate:
                    _cacheFixedUpdate_.Add((_name, _r));
                    break;
                case KeyEvent.Update.LateUpdate:
                    _cacheLateUpdate_.Add((_name, _r));
                    break;
            }
        }

        private void Update()
        {
            if (environment == null) return;
            foreach (var _kvp in keys)
            {
                Execute(_kvp.Key, _kvp.Value);
            }
        }
        private void FixedUpdate()
        {
            if (environment == null) return;
            foreach (var _kvp in _cacheFixedUpdate_)
            {
                environment.eventManager.InvokeEvent(_kvp.Item1, _kvp.Item2);
            }
            _cacheFixedUpdate_.Clear();
        }
        private void LateUpdate()
        {
            if (environment == null) return;
            foreach (var _kvp in _cacheLateUpdate_)
            {
                environment.eventManager.InvokeEvent(_kvp.Item1, _kvp.Item2);
            }
            _cacheLateUpdate_.Clear();
        }
    }
    public class KeyEvent
    {
        public enum keyState
        {
            Down,
            Up,
            Get,
            Other
        }
        public enum Update
        {
            Update,
            FixedUpdate,
            LateUpdate
        }
        public Update callTime = Update.FixedUpdate; // 0 KeyID, 1 keyState, 2 data
        public object[][] getKeys = null;
        public KeyEvent[] nonKeys = null;
    }
}
