using System;
using System.Collections.Generic;
using Farm.CropPlant;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    [Serializable]
    public class ItemSpawnData
    {
        public GameObject crop;
        public int weight;
    }

    public class MapGenerator : MonoBehaviour
    {
        public Tilemap groundTileMap;
        public Tilemap itemTileMap;

        [Header("地图的宽度和高度")] public int width;
        public int height;

        [Header("若useRandomSeed为true将使用seed进行生成")]
        public int seed;

        public bool useRandomSeed;

        [Header("地图裂隙性")]
        public float lacunarity;

        [Header("水的概率，地图中小于此概率的区域将被视为水")] [Range(0, 1f)]
        public float waterProbability;

        [Header("地图上生成的item")]
        public List<ItemSpawnData> itemSpawnDatas;

        [Header("移除孤岛Tile的次数")]
        public int removeSeparateTileNumberOfTimes;

        [Header("表示地面和物品的瓦片地图")]
        public TileBase groundTile;
        public TileBase waterTile;

        // 存储地图数据的二维数组
        private float[,] mapData; // Ture:ground，Flase:water

        public void GenerateMap()
        {
            // groundTileMap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();

            itemSpawnDatas.Sort((data1, data2) => { return data1.weight.CompareTo(data2.weight); });
            GenerateMapData();
            // 地图处理
            for (int i = 0; i < removeSeparateTileNumberOfTimes; i++)
            {
                if (!RemoveSeparateTile()) // 如果本次操作什么都没有处理，则不进行循环
                {
                    break;
                }

            }

            GenerateTileMap();
        }

        private void GenerateMapData()
        {
            // 对于种子的应用
            if (!useRandomSeed) seed = Time.time.GetHashCode();
            UnityEngine.Random.InitState(seed);

            mapData = new float[width, height];

            float randomOffset = UnityEngine.Random.Range(-10000, 10000);

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * lacunarity + randomOffset, y * lacunarity + randomOffset);
                    mapData[x, y] = noiseValue;
                    if (noiseValue < minValue) minValue = noiseValue;
                    if (noiseValue > maxValue) maxValue = noiseValue;
                }
            }

            // 平滑到0~1
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    mapData[x, y] = Mathf.InverseLerp(minValue, maxValue, mapData[x, y]);
                }
            }
        }

        private bool RemoveSeparateTile()
        {
            bool res = false; // 是否是有效的操作
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 是地面且只有一个邻居也是地面
                    if (IsGround(x, y) && GetFourNeighborsGroundCount(x, y) <= 1)
                    {
                        mapData[x, y] = 0; // 设置为水
                        res = true;
                    }
                }
            }

            return res;
        }

        private int GetFourNeighborsGroundCount(int x, int y)
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
            return count;
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


        private void GenerateTileMap()
        {
            CleanTileMap();

            // 地面
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileBase tile = IsGround(x, y) ? groundTile : waterTile;
                    groundTileMap.SetTile(new Vector3Int(x, y), tile);
                }
            }

            // 物品
            int weightTotal = 0;
            for (int i = 0; i < itemSpawnDatas.Count; i++)
            {
                weightTotal += itemSpawnDatas[i].weight;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsGround(x, y) && GetEigthNeighborsGroundCount(x, y) == 8) // 只有地面可以生成物品
                    {
                        float randValue = UnityEngine.Random.Range(1, weightTotal + 1);
                        float temp = 0;

                        for (int i = 0; i < itemSpawnDatas.Count; i++)
                        {
                            temp += itemSpawnDatas[i].weight;
                            if (randValue < temp)
                            {
                                // 命中
                                if (itemSpawnDatas[i].crop)
                                {
                                    TileDetails tileDetails = new TileDetails
                                    {
                                        gridX = x,
                                        gridY = y,
                                        canDig = true,
                                    };
                                    // Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);
                                    // Transform cropParent = GameObject.FindWithTag("CropParent").transform;
                                    // Instantiate(itemSpawnDatas[i].crop, pos, Quaternion.identity, cropParent);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }


        public bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool IsGround(int x, int y)
        {
            return mapData[x, y] > waterProbability;
        }


        public void CleanTileMap()
        {
            groundTileMap.ClearAllTiles();
            itemTileMap.ClearAllTiles();
            return;
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