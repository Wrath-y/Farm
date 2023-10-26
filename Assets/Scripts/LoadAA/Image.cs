using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LoadAA
{
    public class Image : Singleton<Image>
    {
        public AssetReference spriteRef;
        public List<string> aaLoadkeys;
        private Dictionary<string, AsyncOperationHandle> _operationDictionary;
        public UnityEvent ready;
        
        protected override void Awake()
        {
            base.Awake();

            spriteRef.LoadAssetAsync<Sprite>().Completed += (obj) =>
            {
                var img = gameObject.GetComponent<UnityEngine.UI.Image>();
                img.sprite = (Sprite)obj.Result;
                img.type = UnityEngine.UI.Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1;
            };
        }
    }
}
