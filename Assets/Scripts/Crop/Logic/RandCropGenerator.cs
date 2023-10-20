using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.CropPlant
{
    [Serializable]
    public class CropItemData
    {
        public GameObject cropItem;
        public int weight;
        public int seedId;
        public int minGrowthDay;
        public int maxGrowthDay;
        public int maxNum;
        public int curNum;
    }

    public class RandCropGenerator : Singleton<RandCropGenerator>
    {
        [Header("地图上生成的item")]
        public List<CropItemData> itemSpawnDataList;
        
        [Header("若useRandomSeed为true将使用seed进行生成")]
        public int seed;
        public bool useRandomSeed;
        
        [Header("地图裂隙性")]
        public float lacunarity;
        
        /**
         * 用于随机算法
         */
        private Dictionary<string, float> _mapData = new Dictionary<string, float>();


        private void OnEnable()
        {
            EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
        }

        private void OnDisable()
        {
            EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;

        }
        
        private void OnAfterLoadedSceneEvent()
        {
            GenerateMap();
        }

        public void DecrCurNum(int seedItemId)
        {
            foreach (var itemSpawn in itemSpawnDataList)
            {
                if (itemSpawn.seedId != seedItemId) continue;
                itemSpawn.curNum--;
            }
        }

        private string GetKey(int x, int y, string sceneName)
        {
            if (sceneName == "")
            {
                sceneName = SceneManager.GetActiveScene().name;
            }
            return x + "x" + y + "y" + sceneName;
        }
        private void InitTilePropertiesDict(MapData_SO mapDataSo)
        {
            // 对于种子的应用
            if (!useRandomSeed) seed = Time.time.GetHashCode();
            UnityEngine.Random.InitState(seed);
            
            float randomOffset = UnityEngine.Random.Range(-10000, 10000);
            
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            
            foreach (TileProperty tileProperty in mapDataSo.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };

                float noiseValue = Mathf.PerlinNoise( tileDetails.gridX * lacunarity + randomOffset, tileDetails.gridY * lacunarity + randomOffset);
                
                if (tileProperty.gridType == GridType.RandCropItem)
                {
                    tileDetails.isRandCropItem = tileProperty.boolTypeValue;
                    
                    GridMapManager.Instance.UpdateTileDetails(tileDetails);
                    
                    _mapData.Add(GetKey(tileDetails.gridX, tileDetails.gridY, mapDataSo.sceneName), noiseValue);
                    if (noiseValue < minValue) minValue = noiseValue;
                    if (noiseValue > maxValue) maxValue = noiseValue;
                }
            }

            // 平滑到0~1
            foreach (TileProperty tileProperty in mapDataSo.tileProperties)
            {
                if (tileProperty.gridType != GridType.RandCropItem) continue;
                _mapData[GetKey(tileProperty.tileCoordinate.x, tileProperty.tileCoordinate.y, mapDataSo.sceneName)] = Mathf.InverseLerp(minValue, maxValue, _mapData[GetKey(tileProperty.tileCoordinate.x, tileProperty.tileCoordinate.y, mapDataSo.sceneName)]);
            }
        }

        public void GenerateMap()
        {
            if (_mapData == null || _mapData.Count == 0)
            {
                foreach (MapData_SO mapData in GridMapManager.Instance.mapDataList)
                {
                    if (mapData.sceneName != "01.Field")
                    {
                        continue;
                    }
                    InitTilePropertiesDict(mapData);
                }
                
                itemSpawnDataList.Sort((data1, data2) => { return data2.weight.CompareTo(data1.weight); });
            }
            
            GenerateTileMap();
        }

        private void GenerateTileMap()
        {
            // 物品
            int weightTotal = 0;
            foreach (var itemSpawn in itemSpawnDataList)
            {
                weightTotal += itemSpawn.weight;
            }

            var tileDetailsDict = GridMapManager.Instance.GetAllTileDetails();

            for (int i = 0; i < tileDetailsDict.Count; i++)
            {
                TileDetails tile = tileDetailsDict.ElementAt(i).Value;
                if (!tile.isRandCropItem) continue;
                
                if (tile.seedItemId > 0) continue;
                
                foreach (var itemSpawn in itemSpawnDataList)
                {
                    if (itemSpawn.curNum >= itemSpawn.maxNum) break;
                    
                    float randValue = UnityEngine.Random.Range(1, weightTotal + 1);
                    float temp = 0;
                    
                    if (GetEightNeighborsGroundCount(tile.gridX, tile.gridY) == 8 && CanGenerate(tile.gridX, tile.gridY))
                    {
                        temp += itemSpawn.weight;
                        if (randValue < temp)
                        {
                            // 命中
                            if (itemSpawn.cropItem)
                            {
                                tile.seedItemId = itemSpawn.seedId;
                                tile.growthDays = UnityEngine.Random.Range(itemSpawn.minGrowthDay, itemSpawn.maxGrowthDay + 1);
                                
                                GridMapManager.Instance.UpdateTileDetails(tile);
                                
                                EventHandler.CallPlantSeedEvent(tile.seedItemId, tile);
                                
                                itemSpawn.curNum++;
                            }
                        }
                    }
                }
            }
        }
        
        private int GetEightNeighborsGroundCount(int x, int y)
        {
            int count = 0;
            // top
            if (IsInMapRange(x, y + 1) && HasCropItem(x, y)) count += 1;
            // bottom
            if (IsInMapRange(x, y - 1) && HasCropItem(x, y)) count += 1;
            // left
            if (IsInMapRange(x - 1, y) && HasCropItem(x, y)) count += 1;
            // right
            if (IsInMapRange(x + 1, y) && HasCropItem(x, y)) count += 1;

            // left top
            if (IsInMapRange(x - 1, y + 1) && HasCropItem(x, y)) count += 1;
            // right top
            if (IsInMapRange(x + 1, y + 1) && HasCropItem(x, y)) count += 1;
            // left bottom
            if (IsInMapRange(x - 1, y - 1) && HasCropItem(x, y)) count += 1;
            // right bottom
            if (IsInMapRange(x + 1, y - 1) && HasCropItem(x, y)) count += 1;
            return count;
        }

        /**
         * if containsKey return true
         */
        private bool IsInMapRange(int x, int y)
        {
            return _mapData.ContainsKey(GetKey(x, y, ""));
        }

        /**
         * if the GridType is RandCropItem and the seedItemId is invalid return true
         */
        private bool HasCropItem(int x, int y)
        {
            TileDetails tileDetails = GridMapManager.Instance.GetTileDetails(GetKey(x, y, ""));
            if (tileDetails == null) return false;
            if (!tileDetails.isRandCropItem) return false;
            if (tileDetails.seedItemId <= 0) return true;

            return false;
        }
        
        private bool CanGenerate(int x, int y)
        {
            return _mapData[GetKey(x, y, "")] > lacunarity;
        }
    }
}