using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework_Core
{
    public interface IConsoleManager
    {
        Action<Exception> errorCallBack { get; set; }
        Dictionary<string, Console> consoleList {  get; set; }
    }
    public interface IConsoleWindow
    {
        int consoleID { get; set; }
        string consoleName { get; set; }
        string sendButton { get; set; }
        string closeButton { get; set; }
        Func<string, object, bool> execute { get; set; }
        Action<Exception> errorCallBack { get; set; }
        bool isWindowVisible { get; set; }
        Vector2 initialSize { get; set; }
        Vector2 initialPos { get; set; }
        Vector2 scrollPos { get; set; }
        Vector2 widthSize { get; set; }
        Vector2 heightSize { get; set; }
        Rect windowRect { get; set; }
        List<string> logs { get; set; }
        int buttonWidth { get; set; }
        ushort logsSize { get; set; }
        string inputText { get; set; }

        void DrawConsoleWindow();
        void DrawWindow(int windowID);
        void Debug_Log(string text);
    }
    public class ConsoleManager : MonoBehaviour, IConsoleManager
    {
        public Action<Exception> errorCallBack { get; set; }
        public Dictionary<string, Console> consoleList { get; set; } = new Dictionary<string, Console>();
    }
    public class Console
    {
        public IConsoleWindow consoleWindow = new ConsoleWindow();
        public Dictionary<string, Func<Tree_Base>> token = new Dictionary<string, Func<Tree_Base>>();
        public Action<Exception> errorCallBack { get; set; }
        public byte mixExecute = 20;
        private byte _execute_ = 0;
        private int _pos_ = -1;
        private string[] _list_ = null;
        public Console()
        {
            consoleWindow.execute = Execute;
            consoleWindow.errorCallBack = errorCallBack;
        }

        private void End()
        {
            _execute_ = 0;
            _pos_ = -1;
            _list_ = null;
        }
        public bool Execute(string _code, object _obj = null)
        {
            _execute_++;
            if (_execute_ > mixExecute) return false;
            if (_list_ == null)
            {
                int _pos = _code.Length - 1;
                while (_pos > 0)
                {
                    if (char.IsWhiteSpace(_code[_pos]))
                    {
                        _pos--;
                        continue;
                    }
                    else if (_code[_pos] == '\"')
                    {
                        int _lastQuote = _pos;
                        int _firstQuote = _code.LastIndexOf('\"', _lastQuote - 1);
                        if (_firstQuote == -1) return false;
                        string _sub = _code.Substring(_firstQuote + 1, _lastQuote - _firstQuote - 1);
                        _code = _code.Substring(0, _firstQuote).TrimEnd();
                        _list_ = string.IsNullOrEmpty(_code) ? new string[0] : _code.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        _pos_ = _list_.Length - 1;
                        return Execute(_code, _sub);
                    }
                    break;
                }
                _list_ = _code.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _pos_ = _list_.Length - 1;
                return Execute(_code);
            }
            if (_pos_ >= 0 && _list_[_pos_] != null)
            {
                try
                {
                    if (token.TryGetValue(_list_[_pos_], out var _val))
                    {
                        _pos_--;
                        return Execute(_code, _val?.Invoke().func?.Invoke(consoleWindow, _obj));
                    }
                    _pos_--;
                    return Execute(_code, _list_[_pos_]);
                }
                catch (Exception _ex)
                {
                    consoleWindow.errorCallBack?.Invoke(_ex);
                }
            }
            else End();
            return true;
        }
        public bool ExecuteLines(string _code)
        {
            string[] _lines = _code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string _line in _lines)
            {
                if (!Execute(_line)) return false;
            }
            return true;
        }
    }
    public class ConsoleWindow : IConsoleWindow
    {
        // data
        public int consoleID { get; set; } = 0408;
        public string consoleName { get; set; } = "Cosole";
        public string sendButton { get; set; } = "send";
        public string closeButton { get; set; } = "close";
        public Func<string, object, bool> execute { get; set; } = null;
        public Action<Exception> errorCallBack { get; set; } = null;
        public bool isWindowVisible { get; set; } = false;
        public Vector2 initialSize { get; set; } = new Vector2(600f, 500f);
        public Vector2 initialPos { get; set; } = new Vector2(20f, 20f);
        public Vector2 scrollPos { get; set; } = Vector2.zero;
        public Vector2 widthSize { get; set; } = new Vector2(200f, 600f);
        public Vector2 heightSize { get; set; } = new Vector2(150f, 500f);
        public Rect windowRect { get; set; }
        public List<string> logs { get; set; } = new List<string>();
        public int buttonWidth { get; set; } = 30;
        public ushort logsSize { get; set; } = 1000;
        public string inputText { get; set; } = "";
        public ConsoleWindow(int? _id = null, string[] _text = null, Vector2[] _vector2s = null, Rect? _rect = null, List<string> _logs = null, int? buttonWidth = null, ushort? _logsSize = null)
        {
            if (_id != null) consoleID = _id.Value;
            if (_text != null)
            {
                if (_text.Length > 0 && _text[0] != null) consoleName = _text[0];
                if (_text.Length > 1 && _text[1] != null) sendButton = _text[1];
                if (_text.Length > 2 && _text[2] != null) closeButton = _text[2];
            }
            if (_vector2s != null)
            {
                if (_vector2s.Length > 0 && _vector2s[0] != null) initialSize = _vector2s[0];
                if (_vector2s.Length > 1 && _vector2s[1] != null) initialPos = _vector2s[1];
                if (_vector2s.Length > 2 && _vector2s[2] != null) scrollPos = _vector2s[2];
                windowRect = new Rect(initialPos.x, initialPos.y, initialSize.x, initialSize.y);
                if (_vector2s.Length > 3 && _vector2s[3] != null) widthSize = _vector2s[3];
                if (_vector2s.Length > 4 && _vector2s[4] != null) heightSize = _vector2s[4];
            }
            if (_rect != null) windowRect = _rect.Value;
            if (_logs != null) logs = _logs;
            if (buttonWidth != null) buttonWidth = buttonWidth.Value;
            if (_logsSize != null) logsSize = _logsSize.Value;
        }
        public virtual void DrawConsoleWindow()
        {
            if (!isWindowVisible) return;
            windowRect = GUILayout.Window(
                consoleID,
                windowRect,
                DrawWindow,
                consoleName,
                GUILayout.MinWidth(widthSize.x),
                GUILayout.MaxWidth(widthSize.y),
                GUILayout.MinHeight(heightSize.x),
                GUILayout.MaxHeight(heightSize.y)
            );
        }
        public virtual void DrawWindow(int _windowID)
        {
            // text
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(
                scrollPos,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );
            if (logs.Count > logsSize)
            {
                logs.RemoveRange(0, logs.Count - logsSize);
            }
            GUILayout.TextArea(string.Join("\n", logs), GUILayout.ExpandWidth(true));
            GUILayout.EndScrollView();
            // button
            GUILayout.BeginHorizontal();
            inputText = GUILayout.TextField(inputText, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("send", GUILayout.Width(buttonWidth)))
            {
                if (execute != null && !execute.Invoke(inputText, null))
                {
                    Debug_Log("error");
                }
                inputText = "";
            }
            if (GUILayout.Button("close", GUILayout.Width(buttonWidth)))
            {
                isWindowVisible = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        // API
        public virtual void Debug_Log(string _text)
        {
            Debug.Log(_text);
            logs.Add(_text);
            scrollPos = new Vector2(scrollPos.x, Mathf.Infinity);
        }
    }
    public abstract class Tree_Base
    {
        public abstract string introduction { get; set; }
        public abstract Func<IConsoleWindow, object, object> func { get; set; }
        public abstract Tree_Base[] expectation { get; set; }
    }
    public class Tree_Text : Tree_Base
    {
        public override string introduction { get; set; } = "";
        public override Func<IConsoleWindow, object, object> func { get; set; }
        public override Tree_Base[] expectation { get; set; }
    }
    public class Tree_Help : Tree_Base
    {
        public override string introduction { get; set; } = "";
        public override Func<IConsoleWindow, object, object> func { get; set; } = (_console, _obj) =>
        {
            _console.Debug_Log("hello world!");
            return null;
        };
        public override Tree_Base[] expectation { get; set; }
    }
}