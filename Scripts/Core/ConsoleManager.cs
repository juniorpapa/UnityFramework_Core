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
    public class ConsoleManager : MonoBehaviour, IConsoleManager
    {
        public Action<Exception> errorCallBack { get; set; }
        public Dictionary<string, Console> consoleList { get; set; } = new Dictionary<string, Console>();
    }
    public class Console
    {
        public Console_Api _console_Api = new Console_Api();
        public Dictionary<string, Func<Tree_Base>> token = new Dictionary<string, Func<Tree_Base>>();
        private int _pos_ = -1;
        private string[] _list_ = null;

        private void End()
        {
            _pos_ = -1;
            _list_ = null;
        }
        public bool Execute(string _code, object _obj = null)
        {
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
                if (token.TryGetValue(_list_[_pos_], out var _val))
                {
                    _pos_--;
                    return Execute(_code, _val?.Invoke().func?.Invoke(_console_Api, _obj));
                }
                _pos_--;
                return Execute(_code, _list_[_pos_]);
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

    public class Console_Api
    {
        public virtual void Print(string _text)
        {
            Debug.Log(_text);
        }
    }
    public abstract class Tree_Base
    {
        public abstract string introduction { get; set; }
        public abstract Func<Console_Api, object, object> func { get; set; }
        public abstract Tree_Base[] expectation { get; set; }
    }
    public class Tree_Text : Tree_Base
    {
        public override string introduction { get; set; } = "";
        public override Func<Console_Api, object, object> func { get; set; }
        public override Tree_Base[] expectation { get; set; }
    }
    public class Tree_Help : Tree_Base
    {
        public override string introduction { get; set; } = "";
        public override Func<Console_Api, object, object> func { get; set; } = (_console, _obj) =>
        {
            _console.Print("Error");
            return null;
        };
        public override Tree_Base[] expectation { get; set; }
    }
}