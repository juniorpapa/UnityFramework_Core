using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework_Core
{
    public interface IInputManager
    {
        bool enableInput { get; set; }
        Func<KeyEventBind, bool> isTrigger { get; set; }
        Action<Exception> errorCallBack { get; set; }

        Dictionary<string, KeyEventBind> ListAllKeyEvent();
        KeyEventBind GetKeyEvent(string _name);
        public bool AddKeyEvent(string _name, Action _invokeEvent = null, Key[][] _triggerKeys = null, KeyEventBind[] _cantPressed = null);
        bool AddKeyEvent(string _name, KeyEventBind _keyEventBind = null);
        bool RemoveKeyEvent(string _name);
        bool AddActionInvoke(string _name, Action _action);
        bool RemoveActionInvoke(string _name, Action _action);
    }
    public class InputManager : MonoBehaviour, IInputManager
    {
        public bool enableInput { get; set; } = false;
        public KeyEventBind[] keyArray = new KeyEventBind[0];
        public Action<Exception> errorCallBack { get; set; }

        public static Func<KeyCode, bool>[] s_judgment = new Func<KeyCode, bool>[4]
        {
            (KeyCode _keyCode) =>
                {
                    return false;
                },
            (KeyCode _keyCode) =>
                {
                    if (Input.GetKeyDown(_keyCode)) return true;
                    return false;
                },
            (KeyCode _keyCode) =>
                {
                    if (Input.GetKeyUp(_keyCode)) return true;
                    return false;
                },
            (KeyCode _keyCode) =>
                {
                    if (Input.GetKey(_keyCode)) return true;
                    return false;
                }
        };
        public static Func<Key[][], bool> s_checkKeys = (Key[][] _keyArrayArray) =>
        {
            if (_keyArrayArray == null) return false;
            foreach (var _keyArray in _keyArrayArray)
            {
                if (_keyArray == null || _keyArray.Length == 0) continue;
                bool _is = true;
                foreach (var _key in _keyArray)
                {
                    int _index = Math.Clamp(_key.triggerType, 0, s_judgment.Length);
                    if (_index == 0) continue;
                    if (!s_judgment[_index](_key.key))
                    {
                        _is = false;
                        break;
                    }
                }
                if (_is) return true;
            }
            return false;
        };

        public Func<KeyEventBind, bool> isTrigger { get; set; } = (KeyEventBind _keyEventBind) =>
        {
            if (_keyEventBind.triggerKeys == null) return false;
            if (_keyEventBind.triggerKeys.Length == 0) return true;
            if (!s_checkKeys(_keyEventBind.triggerKeys)) return false;
            if (_keyEventBind.cantPressed != null)
                foreach (var _cantPressed in _keyEventBind.cantPressed)
                    if (s_checkKeys(_cantPressed.triggerKeys)) return false;
            return true;
        };

        private void Update()
        {
            try
            {
                if (enableInput)
                    foreach (var _keyEventBind in keyArray)
                    {
                        if (_keyEventBind == null) continue;
                        if (isTrigger(_keyEventBind))
                        {
                            _keyEventBind.invokeEvent?.Invoke();
                        }
                    }
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex);
            }
        }

        public Dictionary<string, KeyEventBind> ListAllKeyEvent()
        {
            Dictionary<string, KeyEventBind> _kvp = new Dictionary<string, KeyEventBind>();
            foreach (var _keyEventBind in keyArray) 
                if (_keyEventBind != null) _kvp[_keyEventBind.name] = _keyEventBind;
            return _kvp;
        }
        public KeyEventBind GetKeyEvent(string _name)
        {
            foreach (var _keyEventBind in keyArray) 
                if (_keyEventBind != null && _keyEventBind.name == _name) return _keyEventBind;
            return null;
        }
        public bool AddKeyEvent(string _name, Action _invokeEvent = null, Key[][] _triggerKeys = null, KeyEventBind[] _cantPressed = null)
        {
            int _nullPos = -1;
            for (int i = keyArray.Length - 1; i >= 0; i++)
            {
                if (keyArray[i] != null && keyArray[i].name == _name) return false;
                if (keyArray[i] == null && _nullPos == -1) _nullPos = i;
            }
            if (_nullPos != -1)
            {
                keyArray[_nullPos] = new KeyEventBind() { name = _name, invokeEvent = _invokeEvent, triggerKeys = _triggerKeys, cantPressed = _cantPressed };
                return true;
            }
            Array.Resize(ref keyArray, keyArray.Length + 1);
            keyArray[keyArray.Length - 1] = new KeyEventBind() { name = _name, invokeEvent = _invokeEvent, triggerKeys = _triggerKeys, cantPressed = _cantPressed };
            return true;
        }
        public bool AddKeyEvent(string _name, KeyEventBind _keyEventBind = null)
        {
            int _nullPos = -1;
            for (int i = keyArray.Length - 1; i >= 0; i++)
            {
                if (keyArray[i] != null && keyArray[i].name == _name) return false;
                if (keyArray[i] == null && _nullPos == -1) _nullPos = i;
            }
            if (_nullPos != -1)
            {
                if (_keyEventBind == null)
                    keyArray[_nullPos] = new KeyEventBind() { name = _name };
                else
                    keyArray[_nullPos] = _keyEventBind;
                return true;
            }
            Array.Resize(ref keyArray, keyArray.Length + 1);
            if (_keyEventBind == null)
                keyArray[keyArray.Length - 1] = new KeyEventBind() { name = _name };
            else
                keyArray[keyArray.Length - 1] = _keyEventBind;
            return true;
        }
        public bool RemoveKeyEvent(string _name)
        {
            for (int i = keyArray.Length - 1; i >= 0; i++)
                if (keyArray[i] != null && keyArray[i].name == _name)
                {
                    keyArray[i] = null;
                    return true;
                }
            return false;
        }

        public bool AddActionInvoke(string _name, Action _action)
        {
            for (int i = keyArray.Length - 1; i >= 0; i++)
                if (keyArray[i] != null && keyArray[i].name == _name)
                {
                    keyArray[i].invokeEvent += _action;
                    return true;
                }
            return false;
        }
        public bool RemoveActionInvoke(string _name, Action _action)
        {
            for (int i = keyArray.Length - 1; i >= 0; i++)
                if (keyArray[i] != null && keyArray[i].name == _name)
                {
                    keyArray[i].invokeEvent -= _action;
                    return true;
                }
            return false;
        }
    }
    public class Key
    {
        public KeyCode key;
        // 0 = Disabled,1 = down, 2 = up,3 = get
        public byte triggerType = 1;
    }
    public class KeyEventBind
    {
        public string name;
        public Action invokeEvent = null;
        public Key[][] triggerKeys = null;
        public KeyEventBind[] cantPressed = null;
    }
}