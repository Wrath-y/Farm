using System;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public Text tradeAmount;
        public Button submitButton;
        public Button cancelButton;
        public Button buy1;
        public Button buy10;
        public Button buy50;

        private ItemDetails _item;
        private bool _isSellTrade;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(TradeItem);
            buy1.onClick.AddListener(AddTradeAmount1);
            buy10.onClick.AddListener(AddTradeAmount10);
            buy50.onClick.AddListener(AddTradeAmount50);
        }

        // 设置TradeUI显示详情
        public void SetupTradeUI(ItemDetails item, bool isSell)
        {
            _item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;
            _isSellTrade = isSell;
            tradeAmount.text = "0";
        }

        private void TradeItem()
        {
            var amount = Convert.ToInt32(tradeAmount.text);

            InventoryManager.Instance.TradeItem(_item, amount, _isSellTrade);

            CancelTrade();
        }


        private void CancelTrade()
        {
            gameObject.SetActive(false);
        }
        
        private void AddTradeAmount1()
        {
            AddTradeAmount(1);
        }
        
        private void AddTradeAmount10()
        {
            AddTradeAmount(10);
        }
        
        private void AddTradeAmount50()
        {
            AddTradeAmount(50);
        }

        private void AddTradeAmount(int amount)
        {
            if (int.TryParse(tradeAmount.text, out int currentAmount))
            {
                int newAmount = currentAmount + amount;
                tradeAmount.text = newAmount.ToString();
            }
            else
            {
                Debug.LogError($"Trade amount {tradeAmount.text} is not a valid integer.");
            }
        }
    }
}