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
            
            InventoryManager.Instance.PickUpItem(item);
        }
    }
}