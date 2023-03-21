using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [Header("物品数据")]
        public ItemDataList_SO itemDataList_SO;

        [Header("背包数据")] public InventoryBag_SO playerBag;

        private void Start()
        {
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }

        public ItemDetails GetItemDetails(int id)
        {
            return itemDataList_SO.ItemDetailsList.Find(e => e.itemID == id);
        }

        public void PickUpItem(Item item)
        {
            var res = CanAddBagItem(item.itemID);
            AddItem(item.itemID, res);
            ItemDetails itemDetails = GetItemDetails(item.itemID);
            Debug.Log(itemDetails.itemName);
            Destroy(item.gameObject);
            
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }

        public struct CanAddBagItemReturns
        {
            public int ExistsIndex;
            public int FirstNilIndex;
        }
        public CanAddBagItemReturns CanAddBagItem(int itemID)
        {
            var res = new CanAddBagItemReturns{ExistsIndex = -1, FirstNilIndex = -1};
            var i = 0;
            var hasAdd = false;
            foreach (var item in playerBag.ItemList)
            {
                if (item.itemID == itemID)
                {
                    res.ExistsIndex = i;
                    return res;
                }
                if (!hasAdd && item.itemID == 0)
                {
                    hasAdd = true;
                    res.FirstNilIndex = i;
                }
                i++;
            }

            return res;
        }

        public void AddItem(int itemID, CanAddBagItemReturns res)
        {
            if (res.ExistsIndex == -1 && res.FirstNilIndex == -1)
            {
                return;
            }

            var index = 0;
            InventoryItem newItem = new InventoryItem();
            newItem.itemID = itemID;
            if (res.ExistsIndex != -1)
            {
                newItem.itemAmount = playerBag.ItemList[res.ExistsIndex].itemAmount + 1;
                index = res.ExistsIndex;
            }
            else
            {
                newItem.itemAmount = 1;
                index = res.FirstNilIndex;
            }
            playerBag.ItemList[index] = newItem;
        }
        
        public void SwapItem(int currentIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.ItemList[currentIndex];
            InventoryItem targetItem = playerBag.ItemList[targetIndex];

            playerBag.ItemList[currentIndex] = targetItem;
            playerBag.ItemList[targetIndex] = currentItem;
            
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }
    }
}
