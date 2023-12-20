using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GeneralGameDevKit.PoolingSystem.Example
{
    /// <summary>
    /// This is an example class that integrates particle system-based effects with an object pool.
    /// </summary>
    public class VfxUtils
    {
        private const float DefaultFxDuration = 2.0f;
        private static readonly Dictionary<GameObject, string> ActiveFxKeys = new();

        /// <summary>
        /// It replicates and plays the given FX prefab using the provided position, scale, duration, etc.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="targetPosition"></param>
        /// <param name="scale"></param>
        /// <param name="autoReturn"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GameObject PlayFxPrefab(GameObject prefab, Vector3 targetPosition, Vector3 scale, bool autoReturn, float duration = -1.0f)
        {
            return prefab ? PlayFxPrefab_Internal(prefab, targetPosition, scale, autoReturn, duration) : null;
        }

        /// <summary>
        /// Get the longest duration among the particle systems.
        /// </summary>
        /// <param name="vfxToPlay"></param>
        /// <returns></returns>
        private static float GetDuration(GameObject vfxToPlay)
        {
            var pSystems = vfxToPlay.GetComponentsInChildren<ParticleSystem>();
            var duration = DefaultFxDuration;
            if (pSystems != null && pSystems.Length != 0)
            {
                duration = pSystems.Max(pSys => pSys.main.duration);
            }

            return duration;
        }

        /// <summary>
        /// Stops the playback of the FX and returns it.
        /// </summary>
        /// <param name="obj"></param>
        public static void StopFxObject(GameObject obj)
        {
            if (!ActiveFxKeys.ContainsKey(obj))
            {
                Debug.LogWarning($"The object ({obj.name}) is not managed by VfxUtils, so it cannot be stopped using this method.");
                return;
            }

            ObjectPoolManager.Instance.ReturnObject(obj, ActiveFxKeys[obj]);
            ActiveFxKeys.Remove(obj);
        }

        /// <summary>
        /// This is the internal method that actually creates and plays the Fx. <br/>
        /// Utilizing this, you can add public methods with various functionalities.
        /// </summary>
        /// <param name="prefab">prefab of fx object</param>
        /// <param name="targetPosition">position to play fx</param>
        /// <param name="scale">fx scale</param>
        /// <param name="autoReturn">auto release(Automatically returns upon completion of the playback duration.)</param>
        /// <param name="duration">manual duration of fx. If set to less than 0 seconds, the duration will be automatically configured. </param>
        /// <returns>playing fx</returns>
        private static GameObject PlayFxPrefab_Internal(GameObject prefab, Vector3 targetPosition, Vector3 scale, bool autoReturn, float duration)
        {
            var fxName = prefab.name;
            if (!ObjectPoolManager.Instance.IsObjPoolSet(fxName))
            {
                ObjectPoolManager.Instance.SetObjPool(prefab, fxName);
            }

            var fxObj = ObjectPoolManager.Instance.GetObject<GameObject>(fxName, true);
            fxObj.transform.position = targetPosition;
            fxObj.transform.localScale = scale;
            fxObj.transform.SetParent(null);

            fxObj.SetActive(true);

            if (duration <= 0)
            {
                duration = GetDuration(fxObj);
            }

            if (autoReturn)
            {
                ThreadAutoReturnTimer(duration, fxObj);
            }

            ActiveFxKeys.Add(fxObj, fxName);
            return fxObj;
        }

        /// <summary>
        /// This is a thread that automatically stops and returns the FX object after a certain period. <br/>
        /// Although it is implemented using the Task class, since multi-threading is not officially supported, 
        /// it is recommended to inherit MonoBehaviour and use a coroutine, JobSystem, or external libraries like UniTask for implementation.
        /// </summary>
        /// <param name="t"> duration </param>
        /// <param name="objToReturn"> obj to return </param>
        private static async void ThreadAutoReturnTimer(float t, GameObject objToReturn)
        {
            await Task.Delay(TimeSpan.FromSeconds(t));
            StopFxObject(objToReturn);
        }
    }
}