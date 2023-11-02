using System;
using System.Collections;
using System.Collections.Generic;
using LoadAA;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Farm.Inventory
{
    public class InventoryUI : Singleton<InventoryUI>, LoadPercent
    {
        private Dictionary<string, AsyncOperationHandle> _aaHandles = new Dictionary<string, AsyncOperationHandle>();
        
        public ItemToolTip ItemToolTip;
        
        [Header("拖拽图片")]
        public Image dragItem;
        
        [Header("玩家背包UI")]
        [SerializeField] private GameObject bagUI;
        private bool _bagOpened;
        
        [Header("通用背包")]
        [SerializeField] private GameObject baseBag;
        public AssetReference shopSlotPrefabRef;
        private GameObject _shopSlotPrefab;
        
        public AssetReference boxSlotPrefabRef;
        private GameObject _boxSlotPrefab;
        
        [Header("交易UI")]
        public TradeUI tradeUI;
        public TextMeshProUGUI playerMoneyText;
        
        [SerializeField] private SlotUI[] playerSlots;
        [SerializeField] private List<SlotUI> baseBagSlots;

        protected override void Awake()
        {
            base.Awake();
            LoadPercent aa = this;
            aa.RegisterLoadPercent();
            
            var shopSlotPrefabHandle = shopSlotPrefabRef.LoadAssetAsync<GameObject>();
            shopSlotPrefabHandle.Completed += obj => _shopSlotPrefab = obj.Result;
            aa.AddHandle("shopSlotPrefab", shopSlotPrefabHandle);
            
            var boxSlotPrefabHandle = boxSlotPrefabRef.LoadAssetAsync<GameObject>();
            boxSlotPrefabHandle.Completed += obj => _boxSlotPrefab = obj.Result;
            aa.AddHandle("boxSlotPrefab", boxSlotPrefabHandle);
        }
        
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI  -= OnUpdateInventoryUI;
            EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

        private void Start()
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }

            _bagOpened = bagUI.activeInHierarchy;
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
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
                        if (list[i].itemID > 0 && list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            if (item != null)
                            {
                                playerSlots[i].SetSlot(item, list[i].itemAmount);
                            }
                        }
                        else
                        {
                            playerSlots[i].ClearSlot();
                        }
                    }
                    break;
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].SetSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].ClearSlot();
                        }
                    }
                    break;
            }
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }
        
        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell);
        }

        public void OpenBagUI()
        {
            _bagOpened = !_bagOpened;
            
            bagUI.SetActive(_bagOpened);
        }
        
        // 点击商店ESC事件
        public void CloseShopBagUI()
        {
            CloseBaseBagUI(SlotType.Shop);
        }
        
        private void CloseBaseBagUI(SlotType slotType)
        {
            baseBag.SetActive(false);
            ItemToolTip.gameObject.SetActive(false);
            UpdateSlotHighlight(-1);

            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();

            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                _bagOpened = false;
            }
            
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
        
        // 打开通用包裹UI事件
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            GameObject prefab = slotType switch
            {
                SlotType.Shop => _shopSlotPrefab,
                SlotType.Box => _boxSlotPrefab,
                _ => null,
            };

            //生成背包UI
            baseBag.SetActive(true);

            baseBagSlots = new List<SlotUI>();

            for (int i = 0; i < bagData.ItemList.Count; i++)
            {
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1, 0.5f);
                bagUI.SetActive(true);
                _bagOpened = true;
            }
            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.ItemList);
        }
        
        // 关闭通用包裹UI事件
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            CloseBaseBagUI(slotType);
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
        
        public void AddHandle(string key, AsyncOperationHandle handle)
        {
            AAManager.Instance.allResourceNum++;
            _aaHandles.Add(key, handle);
        }

        public Dictionary<string, AsyncOperationHandle> GetHandles()
        {
            return _aaHandles;
        }
    }
}
