using System.Linq;
using GeneralGameDevKit.KeyTableSystem.Internal;
using UnityEngine;

namespace Developer.GeneralGameDevKit.TagSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class DevKitTagLoader : MonoBehaviour
    {
        [SerializeField] private KeyTableAsset tagTableAsset;

        private void Awake()
        {
            DevKitTag.LoadTagCollection(tagTableAsset.GetAllKeys().Select(key => new DevKitTag(key.guid)));
        }
    }
}