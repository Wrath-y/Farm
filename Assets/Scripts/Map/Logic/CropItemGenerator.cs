using System;
using System.Collections;
using System.Collections.Generic;
using Farm.CropPlant;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

namespace Farm.Map
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

    public class CropItemGenerator : Singleton<CropItemGenerator>
    {
        private Dictionary<string, TileDetails> _tileDetailsDict = new Dictionary<string, TileDetails>();

        [Header("地图上生成的item")]
        public List<CropItemData> itemSpawnDataList;
        
        [Header("若useRandomSeed为true将使用seed进行生成")]
        public int seed;
        public bool useRandomSeed;
        
        [Header("地图裂隙性")]
        public float lacunarity;
        
        // 存储地图数据的二维数组
        private Dictionary<string, float> _mapData = new Dictionary<string, float>();
        
        private Dictionary<string, float> _mapDataUsed = new Dictionary<string, float>();
        
        private void OnEnable()
        {
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.GameDayEvent -= OnGameDayEvent;

        }

        private void OnGameDayEvent(int day, Season season)
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            if (_tileDetailsDict == null || _tileDetailsDict.Count == 0)
            {
                foreach (MapData_SO mapData in GridMapManager.Instance.mapDataList)
                {
                    if (mapData.sceneName != "01.Field")
                    {
                        continue;
                    }
                    InitTilePropertiesDict(mapData);
                }
            }
            
            itemSpawnDataList.Sort((data1, data2) => { return data2.weight.CompareTo(data1.weight); });
            
            GenerateTileMap();
        }

        private void GenerateTileMap()
        {
            CleanTileMap();

            // 物品
            int weightTotal = 0;
            foreach (var itemSpawn in itemSpawnDataList)
            {
                weightTotal += itemSpawn.weight;
            }

            foreach (var itemSpawn in itemSpawnDataList)
            {
                foreach (var tile in _tileDetailsDict)
                {
                    if (itemSpawn.curNum >= itemSpawn.maxNum) break;
                    
                    float randValue = UnityEngine.Random.Range(1, weightTotal + 1);
                    float temp = 0;
                    
                    if (IsInMapRange(tile.Value.gridX, tile.Value.gridY) && GetEightNeighborsGroundCount(tile.Value.gridX, tile.Value.gridY) == 8 && CanGenerate(tile.Value.gridX, tile. Value.gridY))
                    {
                        temp += itemSpawn.weight;
                        if (randValue < temp)
                        {
                            // 命中
                            if (itemSpawn.cropItem)
                            {
                                Vector3 pos = new Vector3(tile.Value.gridX + 0.5f, tile.Value.gridY + 0.5f, 0);
                                Transform cropParent = GameObject.FindWithTag("RandCropParent").transform;
                                GameObject newCropItem = Instantiate(itemSpawn.cropItem, pos, Quaternion.identity, cropParent);
                                
                                CropGenerator cropGenerator = newCropItem.AddComponent<CropGenerator>();
                                cropGenerator.seedItemID = itemSpawn.seedId;
                                cropGenerator.growthDays = UnityEngine.Random.Range(itemSpawn.minGrowthDay, itemSpawn.maxGrowthDay + 1);

                                _mapDataUsed.Add(tile.Value.gridX + "," + tile.Value.gridY, 1);
                                
                                itemSpawn.curNum++;
                            }
                        }
                    }
                }
            }
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
                    string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapDataSo.sceneName;
                    TileDetails oldTileDetails = GetTileDetails(key);
                    if (oldTileDetails != null)
                    {
                        tileDetails = oldTileDetails;
                    }

                    if (oldTileDetails != null)
                    {
                        _tileDetailsDict[key] = tileDetails;
                    }
                    else
                    {
                        _tileDetailsDict.Add(key, tileDetails);
                    }
                    
                    _mapData.Add(tileDetails.gridX + "," + tileDetails.gridY, noiseValue);
                    if (noiseValue < minValue) minValue = noiseValue;
                    if (noiseValue > maxValue) maxValue = noiseValue;
                }
            }

            // 平滑到0~1
            foreach (TileProperty tileProperty in mapDataSo.tileProperties)
            {
                if (tileProperty.gridType != GridType.RandCropItem) continue;
                _mapData[tileProperty.tileCoordinate.x + "," + tileProperty.tileCoordinate.y] = Mathf.InverseLerp(minValue, maxValue, _mapData[tileProperty.tileCoordinate.x + "," + tileProperty.tileCoordinate.y]);
            }
        }
        
        private TileDetails GetTileDetails(string key)
        {
            if (!_tileDetailsDict.ContainsKey(key))
            {
                return null;
            }

            return _tileDetailsDict[key];
        }
        
        private int GetEightNeighborsGroundCount(int x, int y)
        {
            int count = 0;
            // top
            if (IsInMapRange(x, y + 1) && IsUsefulTile(x, y)) count += 1;
            // bottom
            if (IsInMapRange(x, y - 1) && IsUsefulTile(x, y)) count += 1;
            // left
            if (IsInMapRange(x - 1, y) && IsUsefulTile(x, y)) count += 1;
            // right
            if (IsInMapRange(x + 1, y) && IsUsefulTile(x, y)) count += 1;

            // left top
            if (IsInMapRange(x - 1, y + 1) && IsUsefulTile(x, y)) count += 1;
            // right top
            if (IsInMapRange(x + 1, y + 1) && IsUsefulTile(x, y)) count += 1;
            // left bottom
            if (IsInMapRange(x - 1, y - 1) && IsUsefulTile(x, y)) count += 1;
            // right bottom
            if (IsInMapRange(x + 1, y - 1) && IsUsefulTile(x, y)) count += 1;
            return count;
        }

        /**
         * if containsKey return true
         */
        private bool IsInMapRange(int x, int y)
        {
            return _mapData.ContainsKey(x + "," + y);
        }

        /**
         * if not containsKey and more big than lacunarity return true
         */
        private bool IsUsefulTile(int x, int y)
        {
            return !_mapDataUsed.ContainsKey(x + "," + y);
        }
        
        private bool CanGenerate(int x, int y)
        {
            return _mapData[x + "," + y] > lacunarity;
        }

        public void CleanTileMap()
        {
            foreach (var itemSpawn in itemSpawnDataList)
            {
                itemSpawn.curNum = 0;
            }

            Transform cropParent = GameObject.FindWithTag("RandCropParent").transform;
            // 获取所有子元素的 Transform
            Transform[] children = cropParent.GetComponentsInChildren<Transform>();

            // 遍历并销毁所有子元素
            foreach (Transform child in children)
            {
                // 跳过父 Transform
                if (child == cropParent.transform)
                {
                    continue;
                }

                // 销毁子元素的 GameObject
                Destroy(child.gameObject);
            }

            _mapDataUsed = new Dictionary<string, float>();
        }

    }
}