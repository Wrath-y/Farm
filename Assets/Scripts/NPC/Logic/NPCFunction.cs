using Farm.CropPlant;
using UnityEngine;

public class NPCFunction : MonoBehaviour
{
    public NPCShopType shopType;
    private InventoryBag_SO _shopData;
    private bool _isOpen;
    
    private void OnEnable()
    {
        EventHandler.GameDayEvent += OnGameDayEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameDayEvent -= OnGameDayEvent;
    }
    
    private void Update()
    {
        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            //关闭背包
            CloseShop();
        }
    }
    
    private void OnGameDayEvent(int day, Season season)
    {
        switch (shopType)
        {
            case NPCShopType.Crop:
                RefreshCrop();
                break;
        }
    }

    public void RefreshCrop()
    {
        var cropDetailsList = CropManager.Instance.cropData.cropDetailsList;
        int totalElements = cropDetailsList.Count;
            
        // 使用 System.Random 类生成随机数
        System.Random random = new System.Random();

        // 使用循环从 cropDetailsList 中随机获取4个元素
        var data = new InventoryItem();
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = random.Next(0, totalElements);
            CropDetails randomCrop = cropDetailsList[randomIndex];
            
            data.itemID = randomCrop.seedItemID;
            data.itemAmount = 1;
            _shopData.ItemList.Add(data);
        }
    }

    public void OpenShop()
    {
        _isOpen = true;
        EventHandler.CallBaseBagOpenEvent(SlotType.Shop, _shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public void CloseShop()
    {
        _isOpen = false;
        EventHandler.CallBaseBagCloseEvent(SlotType.Shop, _shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }
}