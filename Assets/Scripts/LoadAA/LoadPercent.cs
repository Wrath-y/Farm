using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LoadAA
{
    public interface LoadPercent
    {
        IEnumerator RegisterLoadPercent()
        {
            while (AAManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            AAManager.Instance.RegisterLoadPercent(this);
        }

        public IEnumerator AddHandle(string key, AsyncOperationHandle handle);
        public Dictionary<string, AsyncOperationHandle> GetHandles();
        
        public IEnumerator Done()
        {
            Dictionary<string, AsyncOperationHandle> handles = GetHandles();
            foreach (var item in handles)
            {
                // while (!item.Value.IsDone)
                // {
                //     yield return null;
                // }
                AAManager.Instance.doneResourceNum++;
                
                AAManager.Instance.loadOps.Add(item.Value);
            }

            yield return null;
        }
    }
}