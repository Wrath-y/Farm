using System;
using System.Collections;
using System.Collections.Generic;
using Farm.CropPlant;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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
        public List<CropItemData> itemSpawnDatas;
        
        [Header("若useRandomSeed为true将使用seed进行生成")]
        public int seed;
        public bool useRandomSeed;
        
        [Header("地图裂隙性")]
        public float lacunarity;
        
        [Header("水的概率，地图中小于此概率的区域将被视为水")] [Range(0, 1f)]
        public float waterProbability;
        
        // 存储地图数据的二维数组
        private float[,] mapData; // Ture:ground，Flase:water
        
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
            
            itemSpawnDatas.Sort((data1, data2) => { return data1.weight.CompareTo(data2.weight); });
            
            GenerateTileMap();
        }

        private void GenerateTileMap()
        {
            CleanTileMap();

            // 物品
            int weightTotal = 0;
            for (int i = 0; i < itemSpawnDatas.Count; i++)
            {
                weightTotal += itemSpawnDatas[i].weight;
            }
            
            for (int i = 0; i < itemSpawnDatas.Count; i++)
            {
                foreach (var tile in _tileDetailsDict)
                {
                    if (itemSpawnDatas[i].curNum >= itemSpawnDatas[i].maxNum) return;
                    
                    float randValue = UnityEngine.Random.Range(1, weightTotal + 1);
                    float temp = 0;
                    // Debug.Log($"{tile.Value.gridX}, {tile.Value.gridY} {IsInMapRange(tile.Value.gridX, tile.Value.gridY)} {GetEigthNeighborsGroundCount(tile.Value.gridX, tile.Value.gridY)}");
                    
                    if (IsGround(tile.Value.gridX, tile.Value.gridY) && GetEigthNeighborsGroundCount(tile.Value.gridX, tile.Value.gridY) == 8)
                    {
                        temp += itemSpawnDatas[i].weight;
                        if (randValue < temp)
                        {
                            // 命中
                            if (itemSpawnDatas[i].cropItem)
                            {
                                Vector3 pos = new Vector3(tile.Value.gridX + 0.5f, tile.Value.gridY + 0.5f, 0);
                                Transform cropParent = GameObject.FindWithTag("CropParent").transform;
                                GameObject newCropItem = Instantiate(itemSpawnDatas[i].cropItem, pos, Quaternion.identity, cropParent);
                                
                                CropGenerator cropGenerator = newCropItem.AddComponent<CropGenerator>();
                                cropGenerator.seedItemID = itemSpawnDatas[i].seedId;
                                cropGenerator.growthDays = UnityEngine.Random.Range(itemSpawnDatas[i].minGrowthDay,
                                    itemSpawnDatas[i].maxGrowthDay + 1);
                                itemSpawnDatas[i].curNum++;
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
            
            mapData = new float[mapDataSo.gridWidth+1,mapDataSo.gridHeight+1];
            
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
                    
                    mapData[tileDetails.gridX, tileDetails.gridY] = noiseValue;
                    if (noiseValue < minValue) minValue = noiseValue;
                    if (noiseValue > maxValue) maxValue = noiseValue;
                    
                    continue;
                }
                    
                mapData[Mathf.Abs(tileDetails.gridX), Mathf.Abs(tileDetails.gridY)] = 0;
            }

            // 平滑到0~1
            foreach (TileProperty tileProperty in mapDataSo.tileProperties)
            {
                if (tileProperty.gridType != GridType.RandCropItem) continue;
                mapData[tileProperty.tileCoordinate.x, tileProperty.tileCoordinate.y] = Mathf.InverseLerp(minValue, maxValue, mapData[tileProperty.tileCoordinate.x, tileProperty.tileCoordinate.y]);
            }
        }
        
        public TileDetails GetTileDetails(string key)
        {
            if (!_tileDetailsDict.ContainsKey(key))
            {
                return null;
            }

            return _tileDetailsDict[key];
        }
        
        private int GetEigthNeighborsGroundCount(int x, int y)
        {
            int count = 0;
            // top
            if (IsInMapRange(x, y + 1) && IsGround(x, y + 1)) count += 1;
            // bottom
            if (IsInMapRange(x, y - 1) && IsGround(x, y - 1)) count += 1;
            // left
            if (IsInMapRange(x - 1, y) && IsGround(x - 1, y)) count += 1;
            // right
            if (IsInMapRange(x + 1, y) && IsGround(x + 1, y)) count += 1;

            // left top
            if (IsInMapRange(x - 1, y + 1) && IsGround(x - 1, y + 1)) count += 1;
            // right top
            if (IsInMapRange(x + 1, y + 1) && IsGround(x + 1, y + 1)) count += 1;
            // left bottom
            if (IsInMapRange(x - 1, y - 1) && IsGround(x - 1, y - 1)) count += 1;
            // right bottom
            if (IsInMapRange(x + 1, y - 1) && IsGround(x + 1, y - 1)) count += 1;
            return count;
        }

        public bool IsInMapRange(int x, int y)
        {
            return mapData[x, y] != 0;
        }

        public bool IsGround(int x, int y)
        {
            return mapData[x, y] > waterProbability;
        }


        public void CleanTileMap()
        {
            foreach (var itemSpawn in itemSpawnDatas)
            {
                itemSpawn.curNum = 0;
            }

            Transform cropParent = GameObject.FindWithTag("CropParent").transform;
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
        }

    }
}