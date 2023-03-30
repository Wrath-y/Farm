using System;
using System.Collections;
using System.Collections.Generic;
using Farm.CropPlant;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    public class GridMapManager : Singleton<GridMapManager>
    {
        public RuleTile digTile;
        public RuleTile waterTile;
        private Tilemap _digTilemap;
        private Tilemap _waterTilemap;
        public List<MapData_SO> mapDataList;
        private Season _curSeason;

        // 场景名称+坐标对应的瓦片信息
        private Dictionary<string, TileDetails> _tileDetailsDict = new Dictionary<string, TileDetails>();
        // 场景是否是第一次加载,用于判断是否预先生成农作物
        private Dictionary<string, bool> _firstLoadDict = new Dictionary<string, bool>();
        private Grid _curGrid;
        
        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap;

        }

        private void Start()
        {
            foreach (MapData_SO mapData in mapDataList)
            {
                _firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }
        }

        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach (TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }
                
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;
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

        public TileDetails GetTileDetailsByMouseGridPos(Vector3Int mouseGridPos)
        {
            return GetTileDetails(mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name);
        }

        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (_digTilemap == null)
            {
                Debug.Log("_digTilemap == null");
                return;
            }
            _digTilemap.SetTile(pos, digTile);
        }

        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (_waterTilemap == null)
            {
                Debug.Log("_waterTilemap == null");
                return;
            }
            _waterTilemap.SetTile(pos, waterTile);
        }
        
        // DisplayMap 显示地图瓦片
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in _tileDetailsDict)
            {
                var key = tile.Key;
                var tileDetails = tile.Value;
                if (!key.Contains(sceneName))
                {
                    continue;
                }

                if (tileDetails.daysSinceDug > -1)
                {
                    SetDigGround(tileDetails);
                }

                if (tileDetails.daysSinceWatered > -1)
                {
                    SetWaterGround(tileDetails);
                }

                if (tileDetails.seedItemId > -1)
                {
                    EventHandler.CallPlantSeedEvent(tileDetails.seedItemId, tileDetails);
                }
            }
        }

        private void RefreshMap()
        {
            if (_digTilemap != null)
            {
                _digTilemap.ClearAllTiles();
            }
            if (_waterTilemap != null)
            {
                _waterTilemap.ClearAllTiles();
            }

            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            DisplayMap(SceneManager.GetActiveScene().name);
        }
        
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if (!_tileDetailsDict.ContainsKey(key))
            {
                _tileDetailsDict.Add(key, tileDetails);
                return;
            }
            
            _tileDetailsDict[key] = tileDetails;
        }

        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
            Crop curCrop = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>())
                {
                    return colliders[i].GetComponent<Crop>();
                }
            }

            return curCrop;
        }

        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            var curTile = GetTileDetailsByMouseGridPos(_curGrid.WorldToCell(mouseWorldPos));

            if (curTile == null)
            {
                return;
            }

            Crop curCrop = GetCropObject(mouseWorldPos);
            switch (itemDetails.itemType)
            {
                // TODO 物品使用实际功能
                case ItemType.Seed:
                    EventHandler.CallPlantSeedEvent(itemDetails.itemID, curTile);
                    EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                    if (itemDetails.itemID > -1)
                    {
                        curTile.seedItemId = itemDetails.itemID;
                    }
                    break;
                case ItemType.Commodity:
                    EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                    break;
                case ItemType.HoeTool:
                    SetDigGround(curTile);
                    curTile.daysSinceDug = 0;
                    curTile.canDig = false;
                    curTile.canDropItem = false;
                    break;
                case ItemType.WaterTool:
                    SetWaterGround(curTile);
                    curTile.daysSinceWatered = 0;
                    break;
                case ItemType.BreakTool:
                case ItemType.ChopTool:
                    // 执行收割方法
                    if (curCrop == null)
                    {
                        Debug.Log("curCrop == null");
                        break;
                    }
                    curCrop.ProcessToolAction(itemDetails, curCrop.tileDetails);
                    break;
                case ItemType.CollectTool:
                    // 执行收割方法
                    if (curCrop == null)
                    {
                        Debug.Log("curCrop == null");
                        break;
                    }
                    curCrop.ProcessToolAction(itemDetails, curTile);
                    break;
            }
            
            UpdateTileDetails(curTile);
        }

        private void OnAfterLoadedSceneEvent()
        {
            _curGrid = FindObjectOfType<Grid>();
            _digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            _waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

            if (_firstLoadDict[SceneManager.GetActiveScene().name])
            {
                // 预先生成农作物
                EventHandler.CallGenerateCropEvent();
                _firstLoadDict[SceneManager.GetActiveScene().name] = false;
            }

            RefreshMap();
        }

        private void OnGameDayEvent(int day, Season season)
        {
            _curSeason = season;
            foreach (var tile in _tileDetailsDict)
            {
                if (tile.Value.daysSinceWatered > -1)
                {
                    tile.Value.daysSinceWatered = -1;
                }

                if (tile.Value.daysSinceDug > -1)
                {
                    tile.Value.daysSinceDug++;
                }

                if (tile.Value.daysSinceDug > 5 && tile.Value.seedItemId <= 0)
                {
                    Debug.Log("restore tile");
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }

                if (tile.Value.seedItemId > -1)
                {
                    tile.Value.growthDays++;
                }
            }

            RefreshMap();
        }
    }
}
