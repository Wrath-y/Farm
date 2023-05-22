using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Farm.CropPlant;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

public class NPCFunction : MonoBehaviour
{
    public List<string> aaLoadkeys;
    private Dictionary<string, AsyncOperationHandle<ScriptableObject>> _operationDictionary;

    public int shopItemCount;
    public NPCShopType shopType;
    private InventoryBag_SO _shopData;
    private List<CropDetails> _seedCropDetails;
    private bool _isOpen;

    private void Awake()
    {
        // Ready.AddListener(OnAssetsReady);
        StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
    }
    
    private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
        if (_operationDictionary == null)
            _operationDictionary = new Dictionary<string, AsyncOperationHandle<ScriptableObject>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(ScriptableObject));

        yield return locations;

        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        foreach (IResourceLocation location in locations.Result) {
            AsyncOperationHandle<ScriptableObject> handle = Addressables.LoadAssetAsync<ScriptableObject>(location);
            handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        // Ready.Invoke();
        OnAssetsReady();
    }
    
    private void OnAssetsReady() {
        foreach (var item in _operationDictionary) {
            switch (item.Key)
            {
                case "MayorShop":
                    _shopData = (InventoryBag_SO)item.Value.Result;
                    break;
            }
        }
    }
    
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
        _shopData.ItemList = new List<InventoryItem>();
        var cropDetailsList = CropManager.Instance.cropData.cropDetailsList;
        if (_seedCropDetails == null)
        {
            // 遍历 cropDetailsList 变量获取所有 seedItemID 属性值第一位是2 的元素
            _seedCropDetails = cropDetailsList.Where(crop => crop.seedItemID.ToString()[0] == '2').ToList();
        }
        
        int totalElements = _seedCropDetails.Count;
            
        // 使用 System.Random 类生成随机数
        System.Random random = new System.Random();

        // 使用循环从 cropDetailsList 中随机获取4个元素
        var data = new InventoryItem();
        for (int i = 0; i < shopItemCount; i++)
        {
            int randomIndex = random.Next(0, totalElements);
            CropDetails randomCrop = _seedCropDetails[randomIndex];
            
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