using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Farm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public List<string> aaLoadkeys;
        private Dictionary<string, AsyncOperationHandle<ScriptableObject>> _operationDictionary;
        
        public CropDataList_SO cropData;
        private Transform _cropParent;
        private Transform _randCropParent;
        private Season _curSeason;
        
        protected override void Awake()
        {
            base.Awake();
            // Ready.AddListener(OnAssetsReady);
            StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
        }
        
        private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
            if (_operationDictionary == null)
                _operationDictionary = new Dictionary<string, AsyncOperationHandle<ScriptableObject>>();

            AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(ScriptableObject));

            yield return locations;

            var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

            foreach (IResourceLocation location in locations.Result) {
                AsyncOperationHandle<ScriptableObject> handle = Addressables.LoadAssetAsync<ScriptableObject>(location);
                handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
                loadOps.Add(handle);
            }

            yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

            // Ready.Invoke();
            OnAssetsReady();
        }
        
        private void OnAssetsReady() {
            foreach (var item in _operationDictionary) {
                switch (item.Key)
                {
                    case "CropDataList_SO":
                        cropData = (CropDataList_SO)item.Value.Result;
                        break;
                }
            }
        }
        
        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        public CropDetails GetCropDetails(int seedItemId)
        {
            return cropData.cropDetailsList.Find(c => c.seedItemID == seedItemId);
        }

        private bool SeasonAvailable(CropDetails crop)
        {
            for (int i = 0; i < crop.seasons.Length; i++)
            {
                if (crop.seasons[i] == _curSeason)
                {
                    return true;
                }
            }

            return false;
        }

        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            // 成长阶段
            var growthStages = cropDetails.growthDays.Length;
            int curStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;

            for (int i = growthStages - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= dayCounter)
                {
                    curStage = i;
                    break;
                }

                dayCounter -= cropDetails.growthDays[i];
            }

            GameObject cropPrefab = cropDetails.growthPrefabs[curStage];
            Sprite cropSprite = cropDetails.growthSprites[curStage];

            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);

            if (tileDetails.isRandCropItem)
            {
                if (_randCropParent == null)
                {
                    _randCropParent = GameObject.FindWithTag("CropParent").transform;
                }
                GameObject randCropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, _randCropParent);
                randCropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;

                randCropInstance.GetComponent<Crop>().cropDetails = cropDetails;
                randCropInstance.GetComponent<Crop>().tileDetails = tileDetails;
                return;
            }
            if (_cropParent == null)
            {
                _cropParent = GameObject.FindWithTag("CropParent").transform;
            }
            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, _cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;

            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            _curSeason = season;
        }
        
        private void OnAfterLoadedSceneEvent()
        {
            _cropParent = GameObject.FindWithTag("CropParent").transform;
            _randCropParent = GameObject.FindWithTag("RandCropParent").transform;
        }

        public void OnPlantSeedEvent(int id, TileDetails tileDetails)
        {
            CropDetails curCrop = GetCropDetails(id);
            
            if (curCrop == null) Debug.LogError($"crop {id} is nil");

            if (!SeasonAvailable(curCrop)) return;
            
            if (tileDetails.seedItemId == -1)    //用于第一次种植
            {
                tileDetails.seedItemId = id;
                tileDetails.growthDays = 0;
            }
            
            //显示农作物
            DisplayCropPlant(tileDetails, curCrop);
        }
    }
}
