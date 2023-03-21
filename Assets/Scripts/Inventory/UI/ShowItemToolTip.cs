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
            if (_slotUI.itemAmount == 0)
            {
                InventoryUI.ItemToolTip.gameObject.SetActive(false);
                return;
            }
            
            InventoryUI.ItemToolTip.gameObject.SetActive(true);
            InventoryUI.ItemToolTip.SetupToolTip(_slotUI.itemDetails, _slotUI.slotType);
            InventoryUI.ItemToolTip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
            InventoryUI.ItemToolTip.transform.position = transform.position + Vector3.up * 60;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InventoryUI.ItemToolTip.gameObject.SetActive(false);
        }
    }
}
