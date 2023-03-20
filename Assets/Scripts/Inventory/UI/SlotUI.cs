using System;
using Farm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image slotImg;
    [SerializeField] private TextMeshProUGUI amountText;
    public Image slotHighlight;
    [SerializeField] private Button button;
    private InventoryUI InventoryUI => GetComponentInParent<InventoryUI>();

    public SlotType slotType;
    public bool isSelected;
    public int slotIndex;
    

    public ItemDetails itemDetails;
    public int itemAmount;

    private void Start()
    {
        isSelected = false;
        if (itemDetails.itemID == 0)
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
    }

    public void ClearSlot()
    {
        if (isSelected)
        {
            isSelected = false;
        }

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
