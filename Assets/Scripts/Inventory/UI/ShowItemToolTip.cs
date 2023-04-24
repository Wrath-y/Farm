using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI _slotUI;
        private InventoryUI InventoryUI => GetComponentInParent<InventoryUI>();

        private void Awake()
        {
            _slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_slotUI.itemDetails == null || _slotUI.itemDetails.itemID == 0)
            {
                InventoryUI.ItemToolTip.gameObject.SetActive(false);
                return;
            }
            
            InventoryUI.ItemToolTip.gameObject.SetActive(true);
            InventoryUI.ItemToolTip.SetupToolTip(_slotUI.itemDetails, _slotUI.slotType);
            InventoryUI.ItemToolTip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
            InventoryUI.ItemToolTip.transform.position = transform.position + Vector3.up * 60;
            
            if (_slotUI.itemDetails.itemType == ItemType.Furniture)
            {
                InventoryUI.ItemToolTip.resourcePanel.SetActive(true);
                InventoryUI.ItemToolTip.SetupResourcePanel(_slotUI.itemDetails.itemID);
            }
            else
            {
                InventoryUI.ItemToolTip.resourcePanel.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InventoryUI.ItemToolTip.gameObject.SetActive(false);
        }
    }
}
