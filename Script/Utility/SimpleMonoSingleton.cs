using UnityEngine;

namespace GeneralGameDevKit.Utils
{
    //########################## Sample Code ##########################
    //public class SomethingClass : MonoSingleton<SomethingClass>
    //{
    // --- your codes..
    //}
    //
    //public void OtherMethod()
    //{
    //      SomethingClass.Instance.[your-method,fields..];
    //}
    //################################################################

    
    /// <summary>
    /// Singleton class for MonoBehaviour components.
    /// Lifetime is same with scene. 
    /// </summary>
    /// <typeparam name="T"> MonoBehaviour types (Components) </typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance) 
                    return instance;
                
                Debug.LogError($"Singleton({typeof(T)}) is null. It's probably been destroyed.");
                return null;
            }
        }

        public static bool IsInstanceAvail()
        {
            return instance != null;
        }

        /// <summary>
        /// Initialize method that runs when an instance is created
        /// </summary>
        protected virtual void InitializeBody(){}

        /// <summary>
        /// Clean up method that runs on destroy instance.
        /// </summary>
        protected virtual void ClearTask() { }

        private void Awake()
        {
            instance ??= this as T;
            InitializeBody();
        }

        private void OnDestroy()
        {
            ClearTask();
            instance = null;
        }
    }
}