using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] private Image slotImg;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image slotHighlight;
    [SerializeField] private Button button;

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
