using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

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
        public ConcurrentDictionary<string, Task<AssetBundle>> _AB_ = new ConcurrentDictionary<string, Task<AssetBundle>>();
        public Action<Exception> errorCallBack { get; set; }

        public virtual bool LoadAssetBundle(string _path, string _name, Resource.Settings _settings = null)
        {
            if (_settings == null) _settings = new Resource.Settings() { state = 2 };
            var _resource = gameResource.GetOrAdd(_name, new Resource.Resource_AssetBundle(_settings, null){state = 1}) as Resource.Resource_AssetBundle;
            lock (_resource)
            {
                switch (_resource.state)
                {
                    case 0:
                        return false;
                    case 1:
                        break;
                    case 2:
                        return false;
                    case 3:
                        return true;
                    case 4:
                        return false;
                    case 5:
                        return false;
                    default:
                        return false;
                }
                try
                {
                    _resource.state = 2;
                    AssetBundle _ab = AssetBundle.LoadFromFile(_path);
                    if (_ab == null)
                    {
                        _resource.state = 4;
                        _resource.TriggerEvent(null);
                        return false;
                    }
                    _resource.assetBundle = _ab;
                    _resource.state = 3;    
                    _resource.TriggerEvent(_ab);
                    return true;
                }
                catch (Exception _ex)
                {
                    _resource.state = 4;
                    _resource.TriggerEvent(null);
                    errorCallBack?.Invoke(_ex);
                    if (_resource.assetBundle != null) _resource.assetBundle.Unload(false);
                    return false;
                }
            }
        }
        public virtual async Task<bool> LoadAssetBundleAsync(string _path, string _name, Resource.Settings _settings = null)
        {
            if (_settings == null) _settings = new Resource.Settings() { state = 2 };
            var _resource = gameResource.GetOrAdd(_name, new Resource.Resource_AssetBundle(_settings, null) { state = 1 }) as Resource.Resource_AssetBundle;
            switch (_resource.state)
            {
                case 0:
                    return false;
                case 1:
                    break;
                case 2:
                    {
                        try
                        {
                            if (!_AB_.TryGetValue(_name, out var _task)) return false;
                            await _task;
                            return true;
                        }
                        catch (Exception _ex)
                        {
                            _AB_.TryRemove(_name, out _);
                            errorCallBack?.Invoke(_ex);
                            return false;
                        }
                    }
                case 3:
                    return true;
                case 4:
                    return false;
                case 5:
                    return false;
                default:
                    return false;
            }
            await _resource.semaphore.WaitAsync();
            try
            {
                _resource.state = 2;
                Func<Task<AssetBundle>> _task = async () =>
                {
                    var _r = AssetBundle.LoadFromFileAsync(_path);
                    await _r;
                    return _r.assetBundle;
                };
                var _t = _task?.Invoke();
                AssetBundle _ab = null;
                if (!_AB_.TryAdd(_name, _t))
                {
                    if (!_AB_.TryGetValue(_name, out var existingTask)) return false;
                    await existingTask;
                    if (_resource.state == 3)
                        return true;
                    else
                        return false;
                }
                else _ab = await _t;
                if (_ab == null)
                {
                    _resource.state = 4;
                    _resource.TriggerEvent(null);
                    return false;
                }
                _resource.assetBundle = _ab;
                _resource.state = 3;
                _resource.TriggerEvent(_ab);
                _AB_.TryRemove(_name, out _);
                return true;
            }
            catch (Exception _ex)
            {
                if (_resource.state != 3 && _resource.state != 4)
                {
                    _resource.state = 4;
                    _resource.TriggerEvent(null);
                    _AB_.TryRemove(_name, out _);
                    if (_resource.assetBundle != null) await _resource.assetBundle.UnloadAsync(false);
                }
                errorCallBack?.Invoke(_ex);
                return false;
            }
            finally
            {
                _resource.semaphore.Release();
            }
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
            gameResource.AddOrUpdate(_name, _resource, (_n, _r) => { return _resource; });
        }

        public virtual Resource.ResourceBase GetResource(string _name)
        {
            if (_name == null) return null;
            if (gameResource.TryGetValue(_name, out var _value))
            {
                bool circular = false;
                List<string> _list = new List<string>() { _name };
                while (_value != null && _value.replace != null && gameResource.TryGetValue(_value.replace, out var _val))
                {
                    foreach (string _n in _list)
                        if (_n == _value.replace)
                        {
                            circular = true;
                            break;
                        }
                    if (circular) break;
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
            public abstract event Action loadCallBack;
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
            public readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
            public AssetBundle assetBundle;
            public override event Action loadCallBack;
            public override bool TriggerEvent(object _obj)
            {
                lock(lockObj)
                {
                    if (_obj == null)
                    {
                        assetBundle = null;
                        state = 4;
                        loadCallBack?.Invoke();
                        return true;
                    }
                    else if (_obj is AssetBundle)
                    {
                        assetBundle = (AssetBundle)_obj;
                        loadCallBack?.Invoke();
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
            public override event Action loadCallBack;
            public override bool TriggerEvent(object _obj)
            {
                lock (lockObj)
                {
                    if (_obj == null)
                    {
                        unityGameObject = null;
                        state = 4;
                        loadCallBack?.Invoke();
                        return true;
                    }
                    else if(_obj is GameObject)
                    {
                        unityGameObject = (GameObject)_obj;
                        loadCallBack?.Invoke();
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
            public override event Action loadCallBack;
            public override bool TriggerEvent(object _obj)
            {
                lock (lockObj)
                {
                    obj = _obj;
                    loadCallBack?.Invoke();
                    return true;
                }
            }
            public Resource_Object(Settings _settings, object _obj) : base(_settings)
            {
                obj = _obj;
            }
        }
    }
}