using System.Linq;
using System.Collections.Generic;
using GeneralGameDevKit.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GeneralGameDevKit.PoolingSystem.Example
{
    /// <summary>
    /// Example class for managing object pools.
    /// Manages multiple object pools using key values.
    /// Developed as a component to share its lifetime with the scene, it is attached to an empty object for use.
    /// </summary>
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {
        private readonly Dictionary<string, UnitObjectPool> _objPools = new();

        protected override void InitializeBody()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// When the scene is unloaded, all currently loaded pools are destroyed.
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            DestroyAllPools();
        }
        
        /// <summary>
        /// Destroy all pools.
        /// </summary>
        public void DestroyAllPools()
        {
            var target = new List<string>();
            target.AddRange(_objPools.Select(kv => kv.Key));

            foreach (var key in target)
            {
                DestroyPool(key);
            }
            
            _objPools.Clear();
        }

        /// <summary>
        /// Checks if there is an object pool corresponding to the given key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>test result</returns>
        public bool IsObjPoolSet(string key) => _objPools.ContainsKey(key);

        /// <summary>
        /// Sets up a new object pool with the original object and the given key.
        /// </summary>
        /// <param name="objToPool">origin object to pooling</param>
        /// <param name="key">The key value to be used for accessing the object pool. <br/>The key value must be unique.</param>
        /// <param name="objHolder">parent object of pooled objects</param>
        /// <param name="initSettings">pool initialization setting</param>
        public void SetObjPool(GameObject objToPool, string key, Transform objHolder = null,
            PoolInitSettings initSettings = default)
        {
            if (initSettings.MaxSize == 0)
                initSettings = new PoolInitSettings(false);
            
            if (_objPools.ContainsKey(key))
            {
                Debug.LogError("Duplicate key value");
                return;
            }

            if (!_objPools.TryAdd(key,
                    new UnitObjectPool(objToPool, objHolder, initSettings.UseCollectionCheck,
                        initSettings.DefaultCapacity, initSettings.MaxSize)))
            {
                Debug.LogError($"Failed to set up the object pool.({objToPool})");
            }
        }

        /// <summary>
        /// Gets an object from the object pool associated with the given key.<br/>
        /// You can directly get the component attached to that object by specifying the generic type.
        /// </summary>
        /// <param name="key">key value of objectPool</param>
        /// <param name="getWithInactive">object activations</param>
        /// <returns>object</returns>
        public T GetObject<T>(string key, bool getWithInactive = false) where T : Object
        {
            if (_objPools.TryGetValue(key, out var targetPool))
            {
                return targetPool.GetItem<T>(!getWithInactive);
            }

            Debug.LogError($"The object pool corresponding to the given key({key}) is not currently managed by the pool manager.");
            return null;
        }

        
        /// <summary>
        /// Access the object pool through the key value and handle the given object for retrieval.
        /// </summary>
        /// <param name="obj">object to return</param>
        /// <param name="key">key of object pool</param>
        /// <returns>Result of the return</returns>
        public bool ReturnObject(GameObject obj, string key)
        {
            if (!_objPools.TryGetValue(key, out var targetPool))
            {
                Debug.LogError($"The object pool corresponding to the given key({key}) is not currently managed by the pool manager.");
                return false;
            }
            obj.SetActive(false);
            return targetPool.ReturnItem(obj);
        }

        /// <summary>
        /// Clean up inactive objects remaining in the pool.
        /// </summary>
        /// <param name="key">key of object pool</param>
        public void ClearPool(string key)
        {
            if (_objPools.TryGetValue(key, out var targetPool))
            {
                targetPool.ClearItem();
            }
            else
            {
                Debug.LogError($"The object pool corresponding to the given key({key}) is not currently managed by the pool manager.");
            }
        }

        /// <summary>
        /// Destroy the corresponding pool.
        /// </summary>
        /// <param name="key">key of object pool</param>
        public void DestroyPool(string key)
        {
            if (_objPools.TryGetValue(key, out var targetPool))
            {
                targetPool.DestroyPool();
                _objPools.Remove(key);
            }
            else
            {
                Debug.LogError($"The object pool corresponding to the given key({key}) is not currently managed by the pool manager.");
            }
        }

        /// <summary>
        /// Pre-generating a certain quantity of objects.
        /// </summary>
        /// <param name="count">count to generated</param>
        /// <param name="key">key of object pool</param>
        public void PrewarmPool(int count, string key)
        {
            if (_objPools.TryGetValue(key, out var targetPool))
            {
                targetPool.PrewarmPool(count);
            }
            else
            {
                Debug.LogError($"The object pool corresponding to the given key({key}) is not currently managed by the pool manager.");
            }
        }
    }
}