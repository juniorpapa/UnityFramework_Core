using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityFramework_Core
{
    public interface IResourceManager
    {
        ConcurrentDictionary<string, Resource.ResourceBase> gameResource { get; set; }
        Action<Exception> errorCallBack { get; set; }

        bool LoadAssetBundle(string _path, string _name, Resource.Settings _settings = null);
        Task<bool> LoadAssetBundleAsync(string _path, string _name, Resource.Settings _settings = null);

        bool SaveResource(string _path, object _obj);
        bool SaveResource(string _path, Resource.ResourceBase _resource);

        void SetResource(string _path, Resource.ResourceBase _resource);

        Resource.ResourceBase GetResource(string _name);
        Resource.ResourceBase GetOriginallyResource(string _name);

        bool UnloadResource(string _name, bool _forced);
        Task<bool> UnloadResourceAsync(string _name, bool _forced);

        ConcurrentDictionary<string, IObjectPool<GameObject>> objectPool { get; set; }
    }
    public class ResourceManager : MonoBehaviour, IResourceManager
    {
        #region Resource
        public ConcurrentDictionary<string, Resource.ResourceBase> gameResource { get; set; } = new ConcurrentDictionary<string, Resource.ResourceBase>();
        public Action<Exception> errorCallBack { get; set; }

        public virtual bool LoadAssetBundle(string _path, string _name, Resource.Settings _settings = null)
        {
            AssetBundle _ab = null;
            try
            {
                _ab = AssetBundle.LoadFromFile(_path);
                if (_settings == null) _settings = new Resource.Settings() { state = 3 };
                if (!gameResource.TryAdd(_name, new Resource.Resource_AssetBundle(_settings, _ab)))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex);
                if (_ab != null) _ab.Unload(true);
                if (_settings == null) _settings = new Resource.Settings() { state = 4 };
                gameResource.TryAdd(_name, new Resource.Resource_AssetBundle(_settings));

            }
            return false;
        }
        public virtual async Task<bool> LoadAssetBundleAsync(string _path, string _name, Resource.Settings _settings = null)
        {
            AssetBundle _ab = null;
            try
            {
                if (_settings == null) _settings = new Resource.Settings() { state = 2 };
                if (!gameResource.TryAdd(_name, new Resource.Resource_AssetBundle(_settings, _ab)))
                    return false;
                var request = AssetBundle.LoadFromFileAsync(_path);
                await request;
                _ab = request.assetBundle;
                if (_ab == null)
                {
                    gameResource[_name].TriggerEvent(null);
                    return false;
                }
                gameResource[_name].state = 3;
                gameResource[_name].TriggerEvent(_ab);
                return true;
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex);
                if (_ab != null) _ab.Unload(true);
                if (gameResource.ContainsKey(_name))
                {
                    gameResource[_name].TriggerEvent(null);
                }
                else
                {
                    if (_settings == null) _settings = new Resource.Settings() { state = 4 };
                    gameResource.TryAdd(_name, new Resource.Resource_AssetBundle(_settings));
                }
            }
            return false;
        }

        public virtual bool SaveResource(string _name, object _obj)
        {
            if (gameResource.TryAdd(_name, new Resource.Resource_Object(null, _obj)))
            {
                return true;
            }
            return false;
        }
        public virtual bool SaveResource(string _name, Resource.ResourceBase _resource)
        {
            if (gameResource.TryAdd(_name, _resource))
            {
                return true;
            }
            return false;
        }

        public virtual void SetResource(string _name, Resource.ResourceBase _resource)
        {
            gameResource[_name] = _resource;
        }

        public virtual Resource.ResourceBase GetResource(string _name)
        {
            if (_name == null) return null;
            if (gameResource.TryGetValue(_name, out var _value))
            {
                List<string> _list = new List<string>() { _name };
                while (_value != null && _value.replace != null && gameResource.TryGetValue(_value.replace, out var _val))
                {
                    foreach (string _n in _list)
                        if (_n == _value.replace) break;
                    _value = _val;
                    _name = _value.replace;
                    _list.Add(_name);
                }
                return _value;
            }
            return null;
        }
        public virtual Resource.ResourceBase GetOriginallyResource(string _name)
        {
            if (_name == null) return null;
            if (gameResource.TryGetValue(_name, out var _value))
            {
                return _value;
            }
            return null;
        }

        public virtual bool UnloadResource(string _name, bool _forced)
        {
            if (_name != null)
            {
                if (gameResource.TryGetValue(_name, out var _value))
                {
                    if (_value is Resource.Resource_AssetBundle)
                    {
                        var _v = (_value as Resource.Resource_AssetBundle);
                        _v.assetBundle.Unload(_forced);
                        _v.state = 5;
                        gameResource.TryRemove(_name, out _value);
                        return true;
                    }
                    gameResource.TryRemove(_name, out _value);
                    return true;
                }
            }
            return false;
        }
        public virtual async Task<bool> UnloadResourceAsync(string _name, bool _forced)
        {
            if (_name != null)
            {
                if (gameResource.TryGetValue(_name, out var _value))
                {
                    if (_value is Resource.Resource_AssetBundle)
                    {
                        await (_value as Resource.Resource_AssetBundle).assetBundle.UnloadAsync(_forced);
                        gameResource.TryRemove(_name, out _value);
                        return true;
                    }
                    gameResource.TryRemove(_name, out _value);
                    return true;
                }
            }
            return false;
        }
        #endregion
        public ConcurrentDictionary<string, IObjectPool<GameObject>> objectPool { get; set; } = new ConcurrentDictionary<string, IObjectPool<GameObject>>();
    }
    public class Resource
    {
        // state
        // 0 = Disabled, 1 = Not Loaded, 2 = Loading, 3 = Loaded, 4 = Load Failed, 5 = Unloaded
        public class Settings
        {
            public object from = null;
            public ushort reference = 0;
            public bool isPermanent = false;
            public byte state = 1;
            public string replace = null;
        }
        public abstract class ResourceBase : Settings
        {
            public readonly object lockObj = new object();
            public abstract event Action load;
            public abstract bool TriggerEvent(object _obj);
            public ResourceBase(Settings _settings)
            {
                from = _settings.from;
                reference = _settings.reference;
                isPermanent = _settings.isPermanent;
                state = _settings.state;
            }
        }
        public class Resource_AssetBundle : ResourceBase
        {
            public AssetBundle assetBundle;
            public override event Action load;
            public override bool TriggerEvent(object _obj)
            {
                lock(lockObj)
                {
                    if (_obj == null)
                    {
                        assetBundle = null;
                        state = 4;
                        load?.Invoke();
                        return true;
                    }
                    else if (_obj is AssetBundle)
                    {
                        assetBundle = (AssetBundle)_obj;
                        load?.Invoke();
                        return true;
                    }
                }
                return false;
            }
            public Resource_AssetBundle(Settings _settings, AssetBundle _ab = null) : base(_settings)
            {
                assetBundle = _ab;
            }
        }
        public class Resource_UnityObject : ResourceBase
        {
            public GameObject unityGameObject;
            public override event Action load;
            public override bool TriggerEvent(object _obj)
            {
                lock (lockObj)
                {
                    if (_obj == null)
                    {
                        unityGameObject = null;
                        state = 4;
                        load?.Invoke();
                        return true;
                    }
                    else if(_obj is GameObject)
                    {
                        unityGameObject = (GameObject)_obj;
                        load?.Invoke();
                        return true;
                    }
                }
                return false;
            }
            public Resource_UnityObject(Settings _settings, GameObject _obj = null) : base(_settings)
            {
                unityGameObject = _obj;
            }
        }
        public class Resource_Object : ResourceBase
        {
            public object obj;
            public override event Action load;
            public override bool TriggerEvent(object _obj)
            {
                lock (lockObj)
                {
                    obj = _obj;
                    load?.Invoke();
                    return true;
                }
            }
            public Resource_Object(Settings _settings, object _obj) : base(_settings)
            {
                obj = _obj;
            }
        }
        public class Resource_Text : ResourceBase
        {
            public string text;
            public override event Action load;
            public override bool TriggerEvent(object _obj)
            {
                lock (lockObj)
                {
                    if (_obj == null)
                    {
                        text = null;
                        state = 4;
                        load?.Invoke();
                        return true;
                    }
                    else if (_obj is string)
                    {
                        text = (string)_obj;
                        load?.Invoke();
                        return true;
                    }
                }
                return false;
            }
            public Resource_Text(Settings _settings, string _text) : base(_settings)
            {
                text = _text;
            }
        }
    }
}