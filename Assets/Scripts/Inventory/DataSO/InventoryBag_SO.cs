using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryBag_SO", menuName = "Inventory/InventoryBag")]
public class InventoryBag_SO : ScriptableObject
{
    public List<InventoryItem> ItemList;
    
    public InventoryItem GetInventoryItem(int ID)
    {
        return ItemList.Find(i => i.itemID == ID);
    }
}
