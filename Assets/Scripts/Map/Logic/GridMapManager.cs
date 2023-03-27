using System;
using System.Collections;
using System.Collections.Generic;
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

        private Dictionary<string, TileDetails> _tileDetailsDict = new Dictionary<string, TileDetails>();
        private Grid _curGrid;

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;

        }

        private void Start()
        {
            foreach (MapData_SO mapData in mapDataList)
            {
                InitTileDetailsDict(mapData);
            }
        }

        private void InitTileDetailsDict(MapData_SO mapData)
        {
            Debug.Log("mapData.tileProperties.count: " + mapData.tileProperties.Count);
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
            DisplayMap(SceneManager.GetActiveScene().name);
        }
        
        private void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if (!_tileDetailsDict.ContainsKey(key))
            {
                Debug.Log("UpdateTileDetails: key not exists");
                return;
            }

            _tileDetailsDict[key] = tileDetails;
        }

        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            var curTile = GetTileDetailsByMouseGridPos(_curGrid.WorldToCell(mouseWorldPos));

            if (curTile == null)
            {
                return;
            }

            switch (itemDetails.itemType)
            {
                case ItemType.Commodity:
                    EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos);
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
            }
            
            UpdateTileDetails(curTile);
        }

        private void OnAfterLoadedSceneEvent()
        {
            _curGrid = FindObjectOfType<Grid>();
            _digTilemap = GameObject.FindWithTag("Dig")?.GetComponent<Tilemap>();
            _waterTilemap = GameObject.FindWithTag("Water")?.GetComponent<Tilemap>();

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
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                }
            }

            RefreshMap();
        }
    }
}
