using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework_Core
{
    public interface IEventManager
    {
        List<GameLog> gameLogs { get; set; }
        ConcurrentDictionary<string, Action<object>> gameEvents { get; set; }
        void AddError(GameLog _gameLog);
        bool AddEvent(string _name, Action<object> _action = null);
        bool RemoveEvent(string _name);
        bool Invoke(string _name, object _obj = null);
        bool Monitor(string _name, Action<object> _action);
        bool UnMonitor(string _name, Action<object> _action);
        bool ContainsKey(string _name);
    }
    public class EventManager : MonoBehaviour, IEventManager
    {
        public List<GameLog> gameLogs { get; set; } = new List<GameLog>();
        public List<GameLog> gameErrors { get; set; } = new List<GameLog>();
        public ConcurrentDictionary<string, Action<object>> gameEvents { get; set; } = new ConcurrentDictionary<string, Action<object>>();
        public virtual void AddError(GameLog _gameLog)
        {
            gameLogs.Add(_gameLog);
            gameErrors.Add(_gameLog);
        }
        public virtual bool AddEvent(string _name, Action<object> _action = null)
        {
            if (gameEvents.TryAdd(_name, _action)) return true;
            return false;
        }
        public virtual bool RemoveEvent(string _name)
        {
            if (gameEvents.TryRemove(_name, out _)) return true;
            return false;
        }
        public virtual bool Invoke(string _name, object _obj = null)
        {
            if (gameEvents.TryGetValue(_name, out var _val))
            {
                _val?.Invoke(_obj);
                return true;
            }
            return false;
        }
        public virtual bool Monitor(string _name, Action<object> _action)
        {
            if (gameEvents.TryGetValue(_name, out var _val))
            {
                gameEvents.AddOrUpdate(_name, _action, (_key, _old) => _old + _action);
                return true;
            }
            return false;
        }
        public virtual bool UnMonitor(string _name, Action<object> _action)
        {
            if (gameEvents.TryGetValue(_name, out var _val))
            {
                gameEvents.AddOrUpdate(_name, _action, (_key, _old) => _old - _action);
                return true;
            }
            return false;
        }
        public virtual bool ContainsKey(string _name)
        {
            return gameEvents.TryGetValue(_name, out _);
        }
    }
    public class GameLog
    {
        public GameLog(string _m, DateTime _t)
        {
            message = _m;
            time = _t;
        }
        public string message;
        public DateTime time;
    }
}