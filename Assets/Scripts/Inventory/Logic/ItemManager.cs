using System;
using Farm.Inventory;
using UnityEngine;

public class ItemManager: MonoBehaviour
{
    public Item itemPrefab;
    private Transform itemParent;

    private void OnEnable()
    {
        EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
        EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
    }

    private void OnDisable()
    {
        EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
        EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
    }

    private void OnAfterLoadedSceneEvent()
    {
        itemParent = GameObject.FindWithTag("ItemParent").transform;
    }

    private void OnInstantiateItemInScene(int id, Vector3 pos)
    {
        var item = Instantiate(itemPrefab, pos, Quaternion.identity, itemParent);
        item.itemID = id;
    }
}