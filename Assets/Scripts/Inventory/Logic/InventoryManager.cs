using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using LoadAA;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

namespace Farm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>, ISaveable, LoadPercent
    {
        private Dictionary<string, AsyncOperationHandle> _aaHandles = new Dictionary<string, AsyncOperationHandle>();
        public List<string> aaLoadkeys;
        private Dictionary<string, AsyncOperationHandle<ScriptableObject>> _operationDictionary;

        [FormerlySerializedAs("itemDataList_SO")] [Header("物品数据")]
        [SerializeField]
        private ItemDataList_SO itemDataList;
        private List<ItemDetails> _itemDetailsList = new List<ItemDetails>();
        
        [Header("建造蓝图")]
        public BluePrintDataList_SO bluePrintData;

        [Header("背包数据")] 
        public InventoryBag_SO playerBagTemp;
        public InventoryBag_SO playerBag;
        private InventoryBag_SO _currentBoxBag;

        [Header("交易")]
        public int playerMoney;
        
        private Dictionary<string, List<InventoryItem>> _boxDataDict = new Dictionary<string, List<InventoryItem>>();
        public int BoxDataAmount => _boxDataDict.Count;
        
        public string GUID => GetComponent<DataGUID>().guid;

        protected override void Awake()
        {
            base.Awake();
            // Ready.AddListener(OnAssetsReady);
            StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
        }

        private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
            LoadPercent aa = this;
            aa.RegisterLoadPercent();
            
            if (_operationDictionary == null)
                _operationDictionary = new Dictionary<string, AsyncOperationHandle<ScriptableObject>>();

            AsyncOperationHandle<IList<IResourceLocation>> locations
                = Addressables.LoadResourceLocationsAsync(keys,
                    Addressables.MergeMode.Union, typeof(ScriptableObject));

            yield return locations;

            var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

            foreach (IResourceLocation location in locations.Result) {
                AsyncOperationHandle<ScriptableObject> handle =
                    Addressables.LoadAssetAsync<ScriptableObject>(location);
                handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
                
                aa.AddHandle(location.PrimaryKey, handle);
                loadOps.Add(handle);
            }

            yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

            // Ready.Invoke();
            OnAssetsReady();
        }
        
        private void OnAssetsReady() {
            LoadPercent aa = this;
            foreach (var item in _operationDictionary) {
                switch (item.Key)
                {
                    case "ItemDataList_SO":
                        itemDataList = (ItemDataList_SO)item.Value.Result;
                        StartCoroutine(SetItemDetail());
                        break;
                    case "BluePrintDataList_SO":
                        bluePrintData = (BluePrintDataList_SO)item.Value.Result;
                        break;
                    case "PlayerBagTpl_SO":
                        playerBagTemp = (InventoryBag_SO)item.Value.Result;
                        break;
                }
            }
        }

        private IEnumerator SetItemDetail()
        {
            foreach (var itemDetail in itemDataList.ItemDetailsList)
            {
                ItemDetails tmp = new ItemDetails(); 
                tmp.itemID = itemDetail.itemID;
                tmp.itemName = itemDetail.itemName;
                tmp.itemType = itemDetail.itemType;
                tmp.itemIconRef = itemDetail.itemIconRef;
                tmp.itemOnWorldSpriteRef = itemDetail.itemOnWorldSpriteRef;
                tmp.itemDescription = itemDetail.itemDescription;
                tmp.itemUseRadius = itemDetail.itemUseRadius;
                tmp.canPickup = itemDetail.canPickup;
                tmp.canDropped = itemDetail.canDropped;
                tmp.canCarried = itemDetail.canCarried;
                tmp.itemPrice = itemDetail.itemPrice;
                tmp.sellPercentage = itemDetail.sellPercentage;
                var handle1 = tmp.itemIconRef.LoadAssetAsync<Sprite>();
                while (!handle1.IsDone)
                {
                    // 在此可使用handle.PercentComplete进行进度展示
                    yield return null;
                }
                tmp.itemIcon = handle1.Result;
                            
                var handle2 = tmp.itemOnWorldSpriteRef.LoadAssetAsync<Sprite>();
                while (!handle2.IsDone)
                {
                    // 在此可使用handle.PercentComplete进行进度展示
                    yield return null;
                }
                tmp.itemOnWorldSprite = handle2.Result;
                            
                _itemDetailsList.Add(tmp);
                Addressables.Release(handle1);
                Addressables.Release(handle2);
            }
        }

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            //建造
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }
        
        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        public ItemDetails GetItemDetails(int id)
        {
            return _itemDetailsList.Find(e => e.itemID == id);
        }
        
        private void OnDropItemEvent(int itemID, Vector3 pos, ItemType itemType)
        {
            RemoveItem(itemID, 1);
        }

        private void OnHarvestAtPlayerPosition(int id)
        {
            var index = GetItemIndexInBag(id);
            AddItemAtIndex(id, index, 1);
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }
        
        private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
        {
            RemoveItem(ID, 1);
            BluePrintDetails bluePrint = bluePrintData.GetBluePrintDetails(ID);
            foreach (var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }
        
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            _currentBoxBag = bag_SO;
        }
        
        private void OnStartNewGameEvent(int obj)
        {
            while (playerBagTemp == null)
            {
                Debug.Log("playerBagTemp is nil");
            }
            playerBag = Instantiate(playerBagTemp);
            playerMoney = Settings.playerStartMoney;
            _boxDataDict.Clear();
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }
        
        public int GetItemIndexInBag(int id)
        {
            for (int i = 0; i < playerBag.ItemList.Count; i++)
            {
                if (playerBag.ItemList[i].itemID == id)
                    return i;
            }
            return -1;
        }

        public void PickUpItem(Item item)
        {
            var res = CanAddBagItem(item.itemID);
            AddItem(item.itemID, res);
            ItemDetails itemDetails = GetItemDetails(item.itemID);
            Destroy(item.gameObject);
            
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }

        public struct CanAddBagItemReturns
        {
            public int ExistsIndex;
            public int FirstNilIndex;
        }
        public CanAddBagItemReturns CanAddBagItem(int itemID)
        {
            var res = new CanAddBagItemReturns{ExistsIndex = -1, FirstNilIndex = -1};
            var i = 0;
            var hasAdd = false;
            foreach (var item in playerBag.ItemList)
            {
                if (item.itemID == itemID)
                {
                    res.ExistsIndex = i;
                    return res;
                }
                if (!hasAdd && item.itemID == 0)
                {
                    hasAdd = true;
                    res.FirstNilIndex = i;
                }
                i++;
            }

            return res;
        }

        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.ItemList.Count; i++)
            {
                if (playerBag.ItemList[i].itemID == 0)
                    return true;
            }
            return false;
        }
        
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            if (index == -1 && CheckBagCapacity())    //背包没有这个物品 同时背包有空位
            {
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                for (int i = 0; i < playerBag.ItemList.Count; i++)
                {
                    if (playerBag.ItemList[i].itemID == 0)
                    {
                        playerBag.ItemList[i] = item;
                        break;
                    }
                }
            }
            else    //背包有这个物品
            {
                int currentAmount = playerBag.ItemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };

                playerBag.ItemList[index] = item;
            }
        }
        
        public void AddItem(int itemID, CanAddBagItemReturns res)
        {
            if (res.ExistsIndex == -1 && res.FirstNilIndex == -1)
            {
                return;
            }

            var index = 0;
            InventoryItem newItem = new InventoryItem();
            newItem.itemID = itemID;
            if (res.ExistsIndex != -1)
            {
                newItem.itemAmount = playerBag.ItemList[res.ExistsIndex].itemAmount + 1;
                index = res.ExistsIndex;
            }
            else
            {
                newItem.itemAmount = 1;
                index = res.FirstNilIndex;
            }
            playerBag.ItemList[index] = newItem;
        }
        
        // player背包范围内交换物品
        // fromIndex 起始序号
        // targetIndex 目标数据序号
        public void SwapItem(int currentIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.ItemList[currentIndex];
            InventoryItem targetItem = playerBag.ItemList[targetIndex];

            if (targetItem.itemID != 0)
            {
                playerBag.ItemList[currentIndex] = targetItem;
                playerBag.ItemList[targetIndex] = currentItem;
            }
            else
            {
                playerBag.ItemList[targetIndex] = currentItem;
                playerBag.ItemList[currentIndex] = new InventoryItem();
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }
        
        // 跨背包交换数据
        public void SwapItem(InventoryLocation locationFrom, int fromIndex, InventoryLocation locationTarget, int targetIndex)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);

            InventoryItem currentItem = currentList[fromIndex];

            if (targetIndex < targetList.Count)
            {
                InventoryItem targetItem = targetList[targetIndex];

                if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)  //有不相同的两个物品
                {
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;
                }
                else if (currentItem.itemID == targetItem.itemID) //相同的两个物品
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                else    //目标空格子
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
                EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
            }
        }
        
        // 根据位置返回背包数据列表
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.ItemList,
                InventoryLocation.Box => _currentBoxBag.ItemList,
                _ => null
            };
        }
        
        // 背包内是否有指定类型的物品
        public int HasItem(ItemType itemType)
        {
            foreach (InventoryItem item in playerBag.ItemList)
            {
                ItemDetails itemDetails = GetItemDetails(item.itemID);
                if (itemDetails.itemType == itemType) return item.itemID;
            }

            return 0;
        }

        public void RemoveItem(int id, int removeAmount)
        {
            var index = GetItemIndexInBag(id);

            if (playerBag.ItemList[index].itemAmount > removeAmount)
            {
                var amount = playerBag.ItemList[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID = id, itemAmount = amount };
                playerBag.ItemList[index] = item;
            }
            else if (playerBag.ItemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem();
                playerBag.ItemList[index] = item;
            }
            
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }

        // 交易物品
        // itemDetails 物品信息
        // amount 交易数量
        // isSellTrade 是否卖东西
        public void TradeItem(ItemDetails itemDetails, int amount, bool isSellTrade)
        {
            int cost = itemDetails.itemPrice * amount;
            //获得物品背包位置
            int index = GetItemIndexInBag(itemDetails.itemID);

            if (isSellTrade)    //卖
            {
                if (playerBag.ItemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID, amount);
                    //卖出总价
                    cost = (int)(cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }
            else if (playerMoney - cost >= 0)   //买
            {
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID, index, amount);
                }
                playerMoney -= cost;
            }
            //刷新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }
        
        // 检查建造资源物品库存
        // ID 图纸ID
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluePrintData.GetBluePrintDetails(ID);

            foreach (var resourceItem in bluePrintDetails.resourceItem)
            {
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if (itemStock.itemAmount >= resourceItem.itemAmount)
                {
                    continue;
                }
                else return false;
            }
            return true;
        }
        
        // 查找箱子数据
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (_boxDataDict.ContainsKey(key))
                return _boxDataDict[key];
            return null;
        }

        // 加入箱子数据字典
        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if (!_boxDataDict.ContainsKey(key))
                _boxDataDict.Add(key, box.boxBagData.ItemList);
            Debug.Log(key);
        }
        
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.playerMoney = playerMoney;

            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name, playerBag.ItemList);

            foreach (var item in _boxDataDict)
            {
                saveData.inventoryDict.Add(item.Key, item.Value);
            }
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            playerMoney = saveData.playerMoney;
            playerBag = Instantiate(playerBagTemp);
            if (!saveData.inventoryDict.ContainsKey(playerBag.name))
            {
                Debug.Log($"{playerBag.name} not exists");
            }
            playerBag.ItemList = saveData.inventoryDict[playerBag.name];

            foreach (var item in saveData.inventoryDict)
            {
                if (_boxDataDict.ContainsKey(item.Key))
                {
                    _boxDataDict[item.Key] = item.Value;
                }
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.ItemList);
        }
        
        public IEnumerator AddHandle(string key, AsyncOperationHandle handle)
        {
            while (AAManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            AAManager.Instance.allResourceNum++;
            _aaHandles.Add(key, handle);
        }

        public Dictionary<string, AsyncOperationHandle> GetHandles()
        {
            return _aaHandles;
        }
    }
}
