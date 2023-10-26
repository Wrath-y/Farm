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
        public List<string> aaLoadkeys;
        private Dictionary<string, AsyncOperationHandle> _operationDictionary;
        public UnityEvent ready;
        
        protected override void Awake()
        {
            base.Awake();
            ready.AddListener(OnAssetsReady);
            ready.AddListener(Init);
            StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
        }
        
        private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys)
        {
            if (_operationDictionary == null)
                _operationDictionary = new Dictionary<string, AsyncOperationHandle>();

            AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(RuleTile));
            yield return locations;

            var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

            foreach (IResourceLocation location in locations.Result) {
                AsyncOperationHandle<RuleTile> handle = Addressables.LoadAssetAsync<RuleTile>(location);
                handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
                loadOps.Add(handle);
            }
            
            locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(MapData_SO));
            yield return locations;

            foreach (IResourceLocation location in locations.Result) {
                AsyncOperationHandle<MapData_SO> handle = Addressables.LoadAssetAsync<MapData_SO>(location);
                handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
                loadOps.Add(handle);
            }

            yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

            ready.Invoke();
        }
        
        private void OnAssetsReady() 
        {
            foreach (var item in _operationDictionary) {
                switch (item.Key)
                {
                    // case "DigTile":
                    //     digTile = (RuleTile)item.Value.Result;
                    //     break;
                    // case "WaterTile":
                    //     waterTile = (RuleTile)item.Value.Result;
                    //     break;
                    // case "MapData_Field":
                    // case "MapData_Home":
                    //     mapDataList.Add((MapData_SO)item.Value.Result);
                    //     break;
                }
            }
        }

        private void Init()
        {
            
        }
    }
}
