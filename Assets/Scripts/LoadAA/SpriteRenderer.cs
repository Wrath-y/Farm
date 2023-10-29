using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace LoadAA
{
    public class SpriteRenderer : MonoBehaviour
    {
        public AssetReference spriteRef;
        private AsyncOperationHandle<Sprite> _opHandle;
        
        public IEnumerator Start()
        {
            _opHandle = spriteRef.LoadAssetAsync<Sprite>();
            yield return _opHandle;
            
            _opHandle.Completed += (obj) =>
            {
                var sr = gameObject.GetComponent<UnityEngine.SpriteRenderer>();
                sr.sprite = obj.Result;
            };
        }
    }
}