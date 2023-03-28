using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemToolTip ItemToolTip;
        public Image dragItem;
        [SerializeField] private GameObject bagUI;
        private bool _bagOpened;
        [SerializeField] private SlotUI[] playerSlots;

        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI  -= OnUpdateInventoryUI;
            EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        }

        private void Start()
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }

            _bagOpened = bagUI.activeInHierarchy; 
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }
        }

        private void OnBeforeUnloadSceneEvent()
        {
            UpdateSlotHighlight(-1);
        }
        
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            Debug.Log(item.itemID + item.itemName);
                            playerSlots[i].SetSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].ClearSlot();
                        }
                    }

                    break;
            }
        }

        public void OpenBagUI()
        {
            _bagOpened = !_bagOpened;
            
            bagUI.SetActive(_bagOpened);
        }

        public void UpdateSlotHighlight(int index)
        {
            foreach (var slot in playerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }
    }
}
