using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LoadAA
{
    public class AAManager : Singleton<AAManager>
    {
        private List<LoadPercent> loadPercents = new List<LoadPercent>();
        public List<AsyncOperationHandle> loadOps = new List<AsyncOperationHandle>();
        public int allResourceNum;
        public int doneResourceNum;
        
        public void RegisterLoadPercent(LoadPercent loadPercent)
        {
            if (!loadPercents.Contains(loadPercent))
                loadPercents.Add(loadPercent);
        }
        
        private IEnumerator CreateGenericGroupOperation()
        {
            yield return new WaitForSeconds(0.5f);
            
            for (int i = 0; i < loadPercents.Count; i++)
            {
                LoadPercent l = loadPercents[i];
                StartCoroutine(l.Done());
                loadPercents.Remove(l);
            }
                
            if (doneResourceNum < allResourceNum)
            {
                StartCoroutine(CreateGenericGroupOperation());
            }
            
            yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);
            Debug.Log("aa资源已释放");
        }

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(CreateGenericGroupOperation());
        }
    }
}