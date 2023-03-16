using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public ItemDataList_SO itemDataList_SO;

        public ItemDetails GetItemDetails(int id)
        {
            return itemDataList_SO.ItemDetailsList.Find(e => e.itemID == id);
        }
    }
}
