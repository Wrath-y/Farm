using Farm.CropPlant;
using UnityEngine;

namespace Farm.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;
        public ItemDetails itemDetails;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _coll;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
            }
        }

        public void Init(int id)
        {
            itemID = id;
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            if (itemDetails == null)
            {
                Debug.Log("InventoryManager.Instance.GetItemDetails return nil itemID: "+itemID);
                return;
            }

            _spriteRenderer.sprite = itemDetails.itemOnWorldSprite == null
                ? itemDetails.itemIcon
                : itemDetails.itemOnWorldSprite;

            Vector2 newSize = new Vector2(_spriteRenderer.sprite.bounds.size.x, _spriteRenderer.sprite.bounds.size.y);
            _coll.size = newSize;
            _coll.offset = new Vector2(0, _spriteRenderer.sprite.bounds.center.y);

            if (itemDetails.itemType == ItemType.ReapableScenery)
            {
                // 添加割草生成农作物component
                gameObject.AddComponent<ReapItem>();
                gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.itemID);
                
                // 添加杂草摇晃脚本component
                gameObject.AddComponent<ItemInteractive>();
            }
        }
    }
}
