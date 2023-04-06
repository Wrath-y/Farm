using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Farm.Inventory;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager: MonoBehaviour, ISaveable
{
    public Item itemPrefab;
    public Item bounceItemPrefab;
    private Transform _itemParent;
    private Transform PlayerTrans => FindObjectOfType<Player>().transform;
    //记录场景Item
    private Dictionary<string, List<SceneItem>> _sceneItemDict = new Dictionary<string, List<SceneItem>>();
    //记录场景家具
    private Dictionary<string, List<SceneFurniture>> _sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();
    
    public string GUID => GetComponent<DataGUID>().guid;

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    
    private void OnEnable()
    {
        EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
        EventHandler.DropItemEvent += OnDropItemEvent;
        EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
        //建造
        EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
        EventHandler.DropItemEvent -= OnDropItemEvent;
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
        EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnBeforeUnloadSceneEvent()
    {
        GetAllSceneItems();
        GetAllSceneFurniture();
    }
    
    private void OnAfterLoadedSceneEvent()
    {
        _itemParent = GameObject.FindWithTag("ItemParent")?.transform;
        RecreateAllItems();
        RebuildFurniture();
    }

    private void OnInstantiateItemInScene(int id, Vector3 pos)
    {
        var item = Instantiate(bounceItemPrefab, pos, Quaternion.identity, _itemParent);
        item.itemID = id;
        item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);
    }
    
    private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
    {
        BluePrintDetails bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(ID);
        var buildItem = Instantiate(bluePrint.buildPrefab, mousePos, Quaternion.identity, _itemParent);
        if (buildItem.GetComponent<Box>())
        {
            buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDataAmount;
            buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
        }
    }

    private void OnDropItemEvent(int id, Vector3 mousePos, ItemType itemType)
    {
        if (itemType == ItemType.Seed)
        {
            return;
        }
        var item = Instantiate(bounceItemPrefab, PlayerTrans.position, Quaternion.identity, _itemParent);
        item.itemID = id;

        var dir = (mousePos - PlayerTrans.position).normalized;
        item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
    }
    
    private void OnStartNewGameEvent(int obj)
    {
        _sceneItemDict.Clear();
        _sceneFurnitureDict.Clear();
    }

    private void GetAllSceneItems()
    {
        List<SceneItem> curSceneItems = new List<SceneItem>();

        foreach (var item in FindObjectsOfType<Item>())
        {
            curSceneItems.Add(new SceneItem
            {
                itemID = item.itemID,
                position = new SerializableVector3(item.transform.position)
            });
        }

        if (_sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
        {
            _sceneItemDict[SceneManager.GetActiveScene().name] = curSceneItems;
        }
        else
        {
            _sceneItemDict.Add(SceneManager.GetActiveScene().name, curSceneItems);
        }
    }

    private void RecreateAllItems()
    {
        List<SceneItem> curSceneItems = new List<SceneItem>();

        if (_sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out curSceneItems))
        {
            if (curSceneItems == null)
            {
                return;
            }

            foreach (var item in FindObjectsOfType<Item>())
            {
                Destroy(item.gameObject);
            }

            foreach (var item in curSceneItems)
            {
                Item newItem = Instantiate(itemPrefab, item.position.ToVector3(), Quaternion.identity, _itemParent);
                newItem.Init(item.itemID);
            }
        }
    }
    
    // 获得场景所有家具
    private void GetAllSceneFurniture()
    {
        List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();

        foreach (var item in FindObjectsOfType<Furniture>())
        {
            SceneFurniture sceneFurniture = new SceneFurniture
            {
                itemID = item.itemID,
                position = new SerializableVector3(item.transform.position)
            };
            if (item.GetComponent<Box>())
                sceneFurniture.boxIndex = item.GetComponent<Box>().index;

            currentSceneFurniture.Add(sceneFurniture);
        }

        if (_sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
        {
            //找到数据就更新item数据列表
            _sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
        }
        else    //如果是新场景
        {
            _sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
        }
    }
    
    
    // 重建当前场景家具
    private void RebuildFurniture()
    {
        List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();

        if (_sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurniture))
        {
            if (currentSceneFurniture != null)
            {
                foreach (SceneFurniture sceneFurniture in currentSceneFurniture)
                {
                    BluePrintDetails bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(sceneFurniture.itemID);
                    var buildItem = Instantiate(bluePrint.buildPrefab, sceneFurniture.position.ToVector3(), Quaternion.identity, _itemParent);
                    if (buildItem.GetComponent<Box>())
                    {
                        buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                    }
                }
            }
        }
    }
    
    public GameSaveData GenerateSaveData()
    {
        GetAllSceneItems();
        GetAllSceneFurniture();

        GameSaveData saveData = new GameSaveData();
        saveData.sceneItemDict = _sceneItemDict;
        saveData.sceneFurnitureDict = _sceneFurnitureDict;

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        _sceneItemDict = saveData.sceneItemDict;
        _sceneFurnitureDict = saveData.sceneFurnitureDict;

        RecreateAllItems();
        RebuildFurniture();
    }
}