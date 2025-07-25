using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;
        private SlotUI _slotUI;
        private bool _canUse = true;

        private void Awake()
        {
            _slotUI = GetComponent<SlotUI>();
        }
        
        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        }

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            _canUse = gameState == GameState.Gameplay;
        }

        private void Update()
        {
            if (Input.GetKeyDown(key) && _canUse)
            {
                if (_slotUI.itemDetails != null)
                {
                    _slotUI.isSelected = !_slotUI.isSelected;
                    if (_slotUI.isSelected)
                    {
                        _slotUI.InventoryUI.UpdateSlotHighlight(_slotUI.slotIndex);
                    }
                    else
                    {
                        _slotUI.InventoryUI.UpdateSlotHighlight(-1);
                    }
                    EventHandler.CallItemSelected(_slotUI.itemDetails, _slotUI.isSelected);
                }
            }
        }
    }   
}
