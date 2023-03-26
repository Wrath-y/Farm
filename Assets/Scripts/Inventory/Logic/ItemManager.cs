using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Farm.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager: MonoBehaviour
{
    public Item itemPrefab;
    private Transform _itemParent;
    private Dictionary<string, List<SceneItem>> _sceneItemDict = new Dictionary<string, List<SceneItem>>();

    private void OnEnable()
    {
        EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
        EventHandler.DropItemEvent += OnDropItemEvent;
        EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
    }

    private void OnDisable()
    {
        EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
        EventHandler.DropItemEvent -= OnDropItemEvent;
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
    }

    private void OnBeforeUnloadSceneEvent()
    {
        GetAllSceneItems();
    }
    
    private void OnAfterLoadedSceneEvent()
    {
        _itemParent = GameObject.FindWithTag("ItemParent")?.transform;
        RecreateAllItems();
    }

    private void OnInstantiateItemInScene(int id, Vector3 pos)
    {
        var item = Instantiate(itemPrefab, pos, Quaternion.identity, _itemParent);
        item.itemID = id;
    }

    private void OnDropItemEvent(int id, Vector3 pos)
    {
        var item = Instantiate(itemPrefab, pos, Quaternion.identity, _itemParent);
        item.itemID = id;
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
}