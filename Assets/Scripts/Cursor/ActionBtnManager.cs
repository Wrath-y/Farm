using System;
using Farm.Inventory;
using Farm.Map;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cursor
{
    public class ActionBtnManager: Singleton<ActionBtnManager>
    {
        public GameObject actionBtn;
        private Image _actionBtnImage;
        private Grid _curGrid;
        private ItemDetails _curItem;
        private bool _actionBtnValid;
        private Transform PlayerTransform => FindObjectOfType<Player>().transform;
        
        private void OnEnable()
        {
            EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
            EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
            EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        }

        private void OnDisable()
        {
            EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
            EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
            EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        }

        private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
        {
            if (!isSelected)
            {
                actionBtn.gameObject.SetActive(false);
                return;
            }

            _curItem = itemDetails;
            actionBtn.gameObject.SetActive(itemDetails.itemType == ItemType.FishingRod ? true : false);
        }

        private void OnBeforeUnloadSceneEvent()
        {
            actionBtn.gameObject.SetActive(false);
        }
        
        private void OnAfterLoadedSceneEvent()
        {
            _curGrid = FindObjectOfType<Grid>();
            actionBtn.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (actionBtn == null || _actionBtnImage == null || _curGrid == null)
            {
                return;
            }
            CheckActioBtnValid();
        }

        // ActionBtn点击事件
        public void Click()
        {
            Debug.Log("click");
            if (!_actionBtnValid) return;
            
            // 背包内是否有鱼饵
            int itemId = InventoryManager.Instance.HasItem(ItemType.FishBait);
            if (itemId == 0)
            {
                Debug.Log("没有鱼饵");
                return;
            }
            
            InventoryManager.Instance.RemoveItem(itemId, 1);
        }
        
        // 判断当前位置是否可点击
        private void CheckActioBtnValid()
        {
            if (_actionBtnImage != null)
            {
                _actionBtnImage = actionBtn.GetComponent<Image>();
            }
            
            // 获取玩家网格坐标
            var playerGridPos = _curGrid.WorldToCell(PlayerTransform.position);
            
            // 获取玩家坐标对应的tile
            TileDetails curTile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(playerGridPos);
            if (curTile == null)
            {
                SetActionBtnInValid();
                return;
            }
            
            // 当前item当前tile是否可执行钓鱼动作
            if (_curItem.itemType != ItemType.FishingRod)
            {
                SetActionBtnInValid();
                return;
            }

            if (!curTile.canFish)
            {
                SetActionBtnInValid();
                return;
            }

            SetActionBtnValid();
            
            return;
        }
        
        private void SetActionBtnValid()
        {
            _actionBtnValid = true;
            _actionBtnImage.color = new Color(1, 1, 1, 1);
        }

        private void SetActionBtnInValid()
        {
            _actionBtnValid = false;
            _actionBtnImage.color = new Color(1, 0, 0, 0.5f);
        }
    }
}