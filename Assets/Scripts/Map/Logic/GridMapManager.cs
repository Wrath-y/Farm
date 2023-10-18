using System;
using System.Collections;
using System.Collections.Generic;
using Farm.CropPlant;
using Farm.Save;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    public class GridMapManager : Singleton<GridMapManager>, ISaveable
    {
        public List<string> aaLoadkeys;
        private Dictionary<string, AsyncOperationHandle> _operationDictionary;
        public UnityEvent ready;
        
        public RuleTile digTile;
        public RuleTile waterTile;
        public List<MapData_SO> mapDataList;
        
        private Tilemap _digTilemap;
        private Tilemap _waterTilemap;

        // 场景名称+坐标对应的瓦片信息
        private Dictionary<string, TileDetails> _tileDetailsDict = new Dictionary<string, TileDetails>();
        // 场景是否是第一次加载,用于判断是否预先生成农作物
        private Dictionary<string, bool> _firstLoadDict = new Dictionary<string, bool>();
        private List<ReapItem> _reapItemsInRadius;
        private Grid _curGrid;
        
        public string GUID => GetComponent<DataGUID>().guid;
        
        protected override void Awake()
        {
            base.Awake();
            ready.AddListener(OnAssetsReady);
            ready.AddListener(Init);
            StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
        }
        
        private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
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
        
        private void OnAssetsReady() {
            foreach (var item in _operationDictionary) {
                switch (item.Key)
                {
                    case "DigTile":
                        digTile = (RuleTile)item.Value.Result;
                        break;
                    case "WaterTile":
                        waterTile = (RuleTile)item.Value.Result;
                        break;
                    case "MapData_Field":
                    case "MapData_Home":
                        mapDataList.Add((MapData_SO)item.Value.Result);
                        break;
                }
            }
        }
        
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

        private void Init()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            
            foreach (MapData_SO mapData in mapDataList)
            {
                _firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }
        }

        // 执行实际工具或物品功能
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
                    EventHandler.CallPlaySoundEvent(SoundName.Plant);
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
                    EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                    break;
                case ItemType.WaterTool:
                    SetWaterGround(curTile);
                    curTile.daysSinceWatered = 0;
                    EventHandler.CallPlaySoundEvent(SoundName.Water);
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
                    EventHandler.CallPlaySoundEvent(SoundName.Basket);
                    break;
                case ItemType.ReapTool:
                    var reapCount = 0;
                    for (int i = 0; i < _reapItemsInRadius.Count; i++)
                    {
                        if (reapCount > Settings.ReapAmount)
                        {
                            break;
                        }
                        EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, _reapItemsInRadius[i].transform.position + Vector3.up);
                        _reapItemsInRadius[i].SpawnHarvestItems();
                        Destroy(_reapItemsInRadius[i].gameObject);
                        reapCount++;
                    }
                    EventHandler.CallPlaySoundEvent(SoundName.Reap);
                    break;
                case ItemType.Furniture:
                    //在地图上生成物品 ItemManager
                    //移除当前物品（图纸）InventoryManager
                    //移除资源物品 InventoryManger
                    EventHandler.CallBuildFurnitureEvent(itemDetails.itemID, mouseWorldPos);
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
                _firstLoadDict[SceneManager.GetActiveScene().name] = false;
                // 预先生成农作物
                EventHandler.CallGenerateCropEvent();
            }

            RefreshMap();
        }

        private void OnGameDayEvent(int day, Season season)
        {
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
                    case GridType.RandCropItem:
                        tileDetails.isRandCropItem = tileProperty.boolTypeValue;
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

        public TileDetails GetTileDetails(string key)
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
                Debug.LogError($"_digTilemap {tile.gridX}, {tile.gridY} is nil");
                return;
            }
            _digTilemap.SetTile(pos, digTile);
        }

        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (_waterTilemap == null)
            {
                Debug.LogError($"_waterTilemap {tile.gridX}, {tile.gridY} is nil");
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

        // 判断鼠标点击位置的农作物
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

        // 返回工具范围内的杂草
        public bool HaveReapableItemsInRadius(Vector3 mouseWorldPos, ItemDetails tool)
        {
            _reapItemsInRadius = new List<ReapItem>();

            Collider2D[] colliders = new Collider2D[20];
            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            var item = colliders[i].GetComponent<ReapItem>();
                            _reapItemsInRadius.Add((item));
                        }
                    }
                }
            }

            return _reapItemsInRadius.Count > 0;
        }

        // 根据场景名字构建网格范围，输出范围和原点
        // return 是否有当前场景的信息
        public bool GetGridDimensions(string sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero; // 网格范围
            gridOrigin = Vector2Int.zero; // 网格原点

            foreach (var mapData in mapDataList)
            {
                if (mapData.sceneName == sceneName)
                {
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                }
            }

            return false;
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = _tileDetailsDict;
            saveData.firstLoadDict = _firstLoadDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            _tileDetailsDict = saveData.tileDetailsDict;
            _firstLoadDict = saveData.firstLoadDict;
        }
    }
}
