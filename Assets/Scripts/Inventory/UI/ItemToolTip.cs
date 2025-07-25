using Farm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;
    
    [Header("建造")]
    public GameObject resourcePanel;
    [SerializeField] private Image[] resourceItem;

    public void SetupToolTip(ItemDetails itemDetails, SlotType slotType)
    {
        nameText.text = itemDetails.itemName;
        typeText.text = GetItemType(itemDetails.itemType);
        descText.text = itemDetails.itemDescription;
        var price = itemDetails.itemPrice;
        if (slotType == SlotType.Bag)
        {
            price = (int)(price * itemDetails.sellPercentage);
        }
        valueText.text = price.ToString();
        
        if (!(itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture))
        {
            bottomPart.SetActive(false);
        }
        else
        {
            bottomPart.SetActive(true);

        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private string GetItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool => "工具",
            ItemType.ChopTool => "工具",
            ItemType.CollectTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.ReapTool => "工具",
            ItemType.WaterTool => "工具",
            _ => "无"
        };
    }
    
    public void SetupResourcePanel(int ID)
    {
        var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(ID);
        for (int i = 0; i < resourceItem.Length; i++)
        {
            if (i < bluePrintDetails.resourceItem.Length)
            {
                var item = bluePrintDetails.resourceItem[i];
                if (item.itemID == 0)
                {
                    resourceItem[i].gameObject.SetActive(false);
                    continue;
                }
                resourceItem[i].gameObject.SetActive(true);
                resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
            }
            else
            {
                resourceItem[i].gameObject.SetActive(false);
            }
        }
    }
}
