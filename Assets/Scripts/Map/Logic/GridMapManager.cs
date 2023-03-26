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

        private Dictionary<string, TileDetails> _tileDetailsDict = new Dictionary<string, TileDetails>();
        private Grid _curGrid;

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
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
        }

        private void OnAfterLoadedSceneEvent()
        {
            _curGrid = FindObjectOfType<Grid>();
            _digTilemap = GameObject.FindWithTag("Dig")?.GetComponent<Tilemap>();
            _waterTilemap = GameObject.FindWithTag("Water")?.GetComponent<Tilemap>();
        }
    }
}
