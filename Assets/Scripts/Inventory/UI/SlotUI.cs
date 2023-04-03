using System;
using Farm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image slotImg;
    [SerializeField] private TextMeshProUGUI amountText;
    public Image slotHighlight;
    [SerializeField] private Button button;

    public SlotType slotType;
    public bool isSelected;
    public int slotIndex;
    

    public ItemDetails itemDetails;
    public int itemAmount;
    
    public InventoryLocation Location
    {
        get
        {
            return slotType switch
            {
                SlotType.Bag => InventoryLocation.Player,
                SlotType.Box => InventoryLocation.Box,
                _ => InventoryLocation.Player
            };
        }
    }
    
    public InventoryUI InventoryUI => GetComponentInParent<InventoryUI>();

    private void Start()
    {
        isSelected = false;
        if (itemDetails == null || itemDetails.itemID == 0)
        {
            ClearSlot();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemAmount == 0) return;
        isSelected = !isSelected;
        slotHighlight.gameObject.SetActive(isSelected);
        InventoryUI.UpdateSlotHighlight(slotIndex);

        if (slotType == SlotType.Bag)
        {
            EventHandler.CallItemSelected(itemDetails, isSelected);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemAmount == 0) return;
        InventoryUI.dragItem.enabled = true;
        InventoryUI.dragItem.sprite = slotImg.sprite;

        isSelected = true;
        InventoryUI.UpdateSlotHighlight(slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        InventoryUI.dragItem.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryUI.dragItem.enabled = false;
        if (eventData.pointerCurrentRaycast.gameObject == null) return;
        if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null) return;
        
        var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
        int targetIndex = targetSlot.slotIndex;

        //在Player自身背包范围内交换
        if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
        {
            InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
        }
        else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)  //买
        {
            EventHandler.CallShowTradeUI(itemDetails, false);
        }
        else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)  //卖
        {
            EventHandler.CallShowTradeUI(itemDetails, true);
        }
        else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
        {
            //跨背包数据交换物品
            InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
        }
        InventoryUI.UpdateSlotHighlight(-1);
    }

    public void ClearSlot()
    {
        if (isSelected)
        {
            isSelected = false;
            InventoryUI.UpdateSlotHighlight(-1);
            EventHandler.CallItemSelected(itemDetails, isSelected);
        }

        itemDetails = new ItemDetails();
        slotImg.enabled = false;
        amountText.text = string.Empty;
        button.interactable = false;
    }

    public void SetSlot(ItemDetails item, int amount)
    {
        itemDetails = item;
        slotImg.sprite = item.itemIcon;
        slotImg.enabled = true;
        itemAmount = amount;
        amountText.text = amount.ToString();
        button.interactable = true;
    }
}
