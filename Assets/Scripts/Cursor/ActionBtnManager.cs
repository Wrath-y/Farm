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
                _curItem = null;
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

        private void FixedUpdate()
        {
            if (actionBtn == null || _curGrid == null || _curItem == null)
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
            if (_actionBtnImage == null)
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
                var topPos = playerGridPos;
                topPos.x += 1;
                TileDetails nearbyTile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(topPos);
                if (nearbyTile != null && nearbyTile.canFish)
                {
                    SetActionBtnValid();
                    return;
                }
                
                var buttomPos = playerGridPos;
                buttomPos.x -= 1;
                nearbyTile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(buttomPos);
                if (nearbyTile != null && nearbyTile.canFish)
                {
                    SetActionBtnValid();
                    return;
                }
                
                var leftPos = playerGridPos;
                leftPos.y -= 1;
                nearbyTile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(leftPos);
                if (nearbyTile != null && nearbyTile.canFish)
                {
                    SetActionBtnValid();
                    return;
                }
                
                var rightPos = playerGridPos;
                rightPos.y += 1;
                nearbyTile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(rightPos);
                if (nearbyTile != null && nearbyTile.canFish)
                {
                    SetActionBtnValid();
                    return;
                }
                SetActionBtnInValid();
                return;
            }

            SetActionBtnValid();
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