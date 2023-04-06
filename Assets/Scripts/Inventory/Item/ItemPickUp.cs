using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        // Start is called before the first frame update
        private void OnTriggerEnter2D(Collider2D other)
        {
            Item item = other.GetComponent<Item>();
            if (item == null) return;
            if (!item.itemDetails.canPickup) return;
            
            //添加到背包
            InventoryManager.Instance.PickUpItem(item);
            
            //播放音效
            EventHandler.CallPlaySoundEvent(SoundName.Pickup);
        }
    }
}