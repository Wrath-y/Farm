using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LoadAA
{
    public class Image : MonoBehaviour
    {
        public AssetReference spriteRef;
        public UnityEngine.UI.Image.Type imageType;
        public int pixelsPerUnitMultiplier;
        private Dictionary<string, AsyncOperationHandle> _operationDictionary;
        
        protected void Start()
        {
            spriteRef.LoadAssetAsync<Sprite>().Completed += (obj) =>
            {
                var img = gameObject.GetComponent<UnityEngine.UI.Image>();
                img.sprite = obj.Result;
                img.type = imageType;
                if (imageType == UnityEngine.UI.Image.Type.Sliced)
                {
                    img.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
                }
            };
        }
    }
}
