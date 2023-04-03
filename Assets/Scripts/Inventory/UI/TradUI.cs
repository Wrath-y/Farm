using System;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public InputField tradeAmount;
        public Button submitButton;
        public Button cancelButton;

        private ItemDetails _item;
        private bool _isSellTrade;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(TradeItem);
        }

        // 设置TradeUI显示详情
        public void SetupTradeUI(ItemDetails item, bool isSell)
        {
            _item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;
            _isSellTrade = isSell;
            tradeAmount.text = string.Empty;
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
    }
}