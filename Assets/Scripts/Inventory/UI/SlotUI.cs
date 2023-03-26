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
    private InventoryUI InventoryUI => GetComponentInParent<InventoryUI>();

    public SlotType slotType;
    public bool isSelected;
    public int slotIndex;
    

    public ItemDetails itemDetails;
    public int itemAmount;

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
        // if (eventData.pointerCurrentRaycast.gameObject == null && itemDetails.canDropped)
        // {
        //     EventHandler.CallInstantiateItemInScene(itemDetails.itemID, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
        //         -Camera.main.transform.position.z)));
        // }

        if (eventData.pointerCurrentRaycast.gameObject == null) return;
        if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null) return;
        var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();

        if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
        {
            InventoryManager.Instance.SwapItem(slotIndex, targetSlot.slotIndex);
        }

        isSelected = false;
        targetSlot.isSelected = true;
        InventoryUI.UpdateSlotHighlight(targetSlot.slotIndex);
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
