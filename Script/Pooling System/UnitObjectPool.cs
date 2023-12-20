using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace GeneralGameDevKit.PoolingSystem
{
    /// <summary>
    /// Setting struct for initialize object pool.
    /// </summary>
    public struct PoolInitSettings
    {
        public readonly bool UseCollectionCheck;
        public readonly int DefaultCapacity;
        public readonly int MaxSize;
        public PoolInitSettings(bool useCollectionCheck = true, int defaultCapacity = 5, int maxSize = 1000)
        {
            UseCollectionCheck = useCollectionCheck;
            DefaultCapacity = defaultCapacity;
            MaxSize = maxSize;
        }
    }


    /// <summary>
    /// Basic Object pool implemented based on Unity's ObjectPool.
    /// Can be used its own or utilized by ObjectPoolManager.
    /// </summary>
    public class UnitObjectPool
    {
        private List<GameObject> _activeObj = new();
        private ObjectPool<GameObject> _pool;
        private GameObject _objToPool;
        private Transform _objHolder;

        /// <summary>
        /// ObjectPool constructor
        /// </summary>
        /// <param name="objToPool">object to pool</param>
        /// <param name="objHolder">parent object of pooled objs</param>
        /// <param name="useCollectionCheck">use collection check on release</param>
        /// <param name="defaultCapacity">default size of stack in object pool</param>
        /// <param name="maxSize">max object count. If max is exceeded, the object will destroy on release.</param>
        public UnitObjectPool(GameObject objToPool, Transform objHolder, bool useCollectionCheck = true, int defaultCapacity = 5, int maxSize = 1000)
        {
            _objToPool = objToPool;
            _objHolder = objHolder;
            _pool = new ObjectPool<GameObject>(CreateItem, OnGetItem, null, OnDestroyItem, useCollectionCheck,
                defaultCapacity, maxSize);
        }

        private GameObject CreateItem()
        {
            var createObject = Object.Instantiate(_objToPool, _objHolder);
            return createObject;
        }

        private void OnGetItem(GameObject getItem)
        {
            _activeObj.Add(getItem);
            getItem.gameObject.SetActive(true);
        }

        private void OnDestroyItem(GameObject managedItem)
        {
            Object.Destroy(managedItem);
        }

        /// <summary>
        /// Pop an item from obj pool.
        /// You can use GameObject or components as generic types.
        /// </summary>
        /// <returns>Object</returns>
        public T GetItem<T>(bool getWithInActive = true) where T : Object
        {
            var ret = _pool.Get();
            ret.SetActive(getWithInActive);
            if (typeof(GameObject).IsAssignableFrom(typeof(T)))
            {
                return ret as T;
            }
            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                return ret.GetComponent<T>();
            }

            Debug.LogError($"Type T:{typeof(T)}is not compatible type with ObjectPool.");
            return null;
        }
        
        /// <summary>
        /// Pre-generates a certain amount of items in the pool.
        /// Can be used for the purpose of preventing overhead due to object creation during gameplay.
        /// </summary>
        /// <param name="count">The number of objects to pre-generate.</param>
        public void PrewarmPool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var ret = _pool.Get();
                ret.gameObject.SetActive(false);
                _pool.Release(ret);
            }
        }

        /// <summary>
        /// Returns the item(object) to the object pool.
        /// </summary>
        /// <param name="itemToRet">GameObject to return.</param>
        public bool ReturnItem(GameObject itemToRet)
        {
            if (!_activeObj.Remove(itemToRet))
            {
                Debug.LogWarning($"Object({itemToRet.name}) is not managed by this pool.");
                return false;
            }

            _pool.Release(itemToRet);
            return true;
        }

        /// <summary>
        /// Returns the item(object) to the object pool.
        /// Through a method of returning via components, it accesses the GameObject with the attached component to handle the return.
        /// </summary>
        /// <param name="componentToRet">Components attached GameObject to return</param>
        public void ReturnItem(Component componentToRet)
        {
            var itemToRet = componentToRet.gameObject;

            if (!_activeObj.Remove(itemToRet))
            {
                Debug.LogWarning($"Object({itemToRet.name}) is not managed by this pool.");
                return;
            }

            _pool.Release(componentToRet.gameObject);
        }

        /// <summary>
        /// Destroys all objects in the pool.
        /// </summary>
        public void ClearItem() => _pool.Clear();

        /// <summary>
        /// Destroys the object pool. Afterward, since the pool cannot be used, the reference should be set to null.
        /// </summary>
        public void DestroyPool()
        {
            _pool.Dispose();
            foreach (var obj in _activeObj)
            {
                OnDestroyItem(obj);
            }
            _activeObj.Clear();
            
            _pool = null;
            _activeObj = null;
            _objHolder = null;
            _objToPool = null;
        }
    }
}