using UnityEngine;

[System.Serializable]
public class ItemDetails
{
    public int ItemId;
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;
    public Sprite itemOnWorldSprite;
    public string itemDescription;
    public int itemUseRadius;
    public bool canPickup;
    public bool canDropped;
    public bool canCarried;
    public bool itemPrice;
    [Range(0, 1)]
    public float sellPercentage;
}