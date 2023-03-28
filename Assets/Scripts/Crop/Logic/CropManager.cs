using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;
        private Transform _cropParent;
        private Grid _curGrid;
        private Season _curSeason;
        
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

        public CropDetails GetCropDetails(int id)
        {
            return cropData.cropDetailsList.Find(c => c.seedItemID == id);
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

            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, _cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;

            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            _curSeason = season;
        }
        
        private void OnAfterLoadedSceneEvent()
        {
            _curGrid = FindObjectOfType<Grid>();
            _cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int id, TileDetails tileDetails)
        {
            CropDetails curCrop = GetCropDetails(id);
            Debug.Log("OnPlantSeedEvent"+id);
            if (curCrop == null || !SeasonAvailable(curCrop))
            {
                return;
            }

            if (curCrop.seedItemID <= 0)
            {
                Debug.Log("tileDetails.seedItemId = id");
                tileDetails.seedItemId = id;
                tileDetails.growthDays = 0;
            }

            DisplayCropPlant(tileDetails, curCrop);
        }
    }
}
