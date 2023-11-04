using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Farm.CropPlant;
using Farm.Inventory;
using Farm.Map;
using Farm.Transition;
using LoadAA;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using Image = UnityEngine.UI.Image;

namespace Cursor
{
public class CursorManager : Singleton<CursorManager>, LoadPercent
{
    private Dictionary<string, AsyncOperationHandle> _aaHandles = new Dictionary<string, AsyncOperationHandle>();
    public AssetReference normalRef, toolRef, seedRef, itemRef;
    private Sprite _normal, _tool, _seed, _item;

    private Sprite _curSprite;
    private Image _cursorImage;
    private RectTransform _cursorCanvas;
    
    //建造图标跟随
    private Image _buildImage;

    private Camera _mainCamera;
    private Grid _curGrid;
    private Vector3 _mouseWorldPos;
    private Vector3Int _mouseGridPos;
    private bool _cursorEnable;
    private bool _cursorPositionValid;
    private ItemDetails _curItem;
    private bool _isMobile = true;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;
    
    protected override void Awake()
    {
        base.Awake();
        LoadPercent aa = this;
        aa.RegisterLoadPercent();
        
        AsyncOperationHandle<Sprite> normalHandle = normalRef.LoadAssetAsync<Sprite>();
        normalHandle.Completed += obj => _normal = obj.Result;
        aa.AddHandle("cursor normal", normalHandle);
        
        AsyncOperationHandle<Sprite> toolHandle = toolRef.LoadAssetAsync<Sprite>();
        toolHandle.Completed += obj => _tool = obj.Result;
        aa.AddHandle("cursor tool", toolHandle);
        
        AsyncOperationHandle<Sprite> seedHandle = seedRef.LoadAssetAsync<Sprite>();
        seedHandle.Completed += obj => _seed = obj.Result;
        aa.AddHandle("cursor seed", seedHandle);
        
        AsyncOperationHandle<Sprite> itemHandle = itemRef.LoadAssetAsync<Sprite>();
        itemHandle.Completed += obj => _item = obj.Result;
        aa.AddHandle("cursor item", itemHandle);
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
        EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
    }

    private void Start()
    {
        StartCoroutine(InitIcon());
    }

    private IEnumerator InitIcon()
    {
        while (!TransitionManager.Instance.hasLoadedUI)
        {
            yield return new WaitForSeconds(0.2f);
        }
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.LinuxEditor ||
            Application.platform == RuntimePlatform.LinuxPlayer)
        {
            _isMobile = false;
        }

        _cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        _cursorImage = _cursorCanvas.GetChild(0).GetComponent<Image>();
        //拿到建造图标
        _buildImage = _cursorCanvas.GetChild(1).GetComponent<Image>();
        _buildImage.gameObject.SetActive(false);

        _curSprite = _normal;
        SetCursorImage(_normal);
        
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_cursorCanvas == null)
        {
            return;
        }

        if (_isMobile && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            _cursorImage.transform.position = touch.position;
        }
        if (!_isMobile)
        {
            _cursorImage.transform.position = Input.mousePosition;
        }
        
        if (!InteractWithUI() && _cursorEnable)
        {
            SetCursorImage(_curSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(_normal);
            _buildImage.gameObject.SetActive(false);
        }
    }
    
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            _curItem = null;
            _cursorEnable = false;
            _curSprite = _normal;
            _buildImage.gameObject.SetActive(false);
            return;
        }
        
        _curItem = itemDetails;
        // TODO 新类型需添加鼠标样式
        _curSprite = itemDetails.itemType switch
        {
            ItemType.Seed => _seed,
            ItemType.Commodity => _item,
            ItemType.ChopTool => _tool,
            ItemType.HoeTool => _tool,
            ItemType.WaterTool => _tool,
            ItemType.BreakTool => _tool,
            ItemType.ReapTool => _tool,
            ItemType.Furniture => _tool,
            ItemType.CollectTool => _tool,
            _ => _normal
        };
        _cursorEnable = true;
        
        //显示建造物品图片
        if (itemDetails.itemType == ItemType.Furniture)
        {
            _buildImage.gameObject.SetActive(true);
            _buildImage.sprite = itemDetails.itemOnWorldSprite;
            // _buildImage.SetNativeSize();
        }
    }

    private void OnBeforeUnloadSceneEvent()
    {
        _cursorEnable = false;
    }
    
    private void OnAfterLoadedSceneEvent()
    {
        _curGrid = FindObjectOfType<Grid>();
    }

    /**
     *  检查是否有触摸/点击输入且光标位置有效 
     */
    private void CheckPlayerInput()
    {
        if (Input.touchCount > 0 && _cursorPositionValid)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log($"Mobile CheckPlayerInput {_curItem.itemID}");
                EventHandler.CallMouseClickedEvent(_mouseWorldPos, _curItem);
                return;
            }
        }

        if (Input.GetMouseButtonDown(0) && _cursorPositionValid)
        {
            Debug.Log($"PC CheckPlayerInput {_curItem.itemID}");
            EventHandler.CallMouseClickedEvent(_mouseWorldPos, _curItem);
        }
    }

    private void SetCursorImage(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.Log("SetCursorImage's sprite is nil");
        }
        if (_cursorImage == null)
        {
            Debug.Log("SetCursorImage's _cursorImage is nil");
        }
        _cursorImage.sprite = sprite;
        _cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void SetCursorValid()
    {
        _cursorPositionValid = true;
        _cursorImage.color = new Color(1, 1, 1, 1);
        _buildImage.color = new Color(1, 1, 1, 0.5f);
    }

    private void SetCursorInValid()
    {
        _cursorPositionValid = false;
        _cursorImage.color = new Color(1, 0, 0, 0.5f);
        _buildImage.color = new Color(1, 0, 0, 0.5f);
    }

    /**
     * 检查当前的事件系统是否存在，并且鼠标是否悬停在UI对象上方
     */
    private bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        return false;
    }

    public Vector3 GetMouseWorldPos()
    {
        return _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            -_mainCamera.transform.position.z));
    }

    private void CheckCursorValid()
    {
        _mouseWorldPos = GetMouseWorldPos();
        
        _mouseGridPos = _curGrid.WorldToCell(_mouseWorldPos);
        var playerGridPos = _curGrid.WorldToCell(PlayerTransform.position);
        //建造图片跟随移动
        _buildImage.rectTransform.position = Input.mousePosition;
        
        if (Math.Abs(_mouseGridPos.x - playerGridPos.x) > _curItem.itemUseRadius || Math.Abs(_mouseGridPos.y - playerGridPos.y) > _curItem.itemUseRadius)
        {
            SetCursorInValid();
            return;
        }
        
        TileDetails curTile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(_mouseGridPos);
        if (curTile == null)
        {
            SetCursorInValid();
            return;
        }
        CropDetails curCrop = CropManager.Instance.GetCropDetails(curTile.seedItemId);
        Crop crop = GridMapManager.Instance.GetCropObject(_mouseWorldPos);
        
        // TODO 补充所有物品类型的判读
        switch (_curItem.itemType)
        {
            case ItemType.Seed:
                if (curTile.daysSinceDug > -1 && curTile.seedItemId <= 0) SetCursorValid(); else SetCursorInValid();
                break;
            case ItemType.Commodity:
                if (curTile.canDropItem && _curItem.canDropped) SetCursorValid(); else SetCursorInValid();
                break;
            case ItemType.HoeTool:
                if (curTile.canDig) SetCursorValid(); else SetCursorInValid();
                break;
            case ItemType.WaterTool:
                if (curTile.daysSinceDug > -1 && curTile.daysSinceWatered == -1) SetCursorValid(); else SetCursorInValid();
                break;
            case ItemType.BreakTool:
            case ItemType.ChopTool:
                if (crop != null)
                {
                    Debug.Log($"{crop.CanHarvest}");
                    if (crop.CanHarvest && crop.cropDetails.CheckToolAvailable(_curItem.itemID))
                    {
                        SetCursorValid();
                    }
                    else
                    {
                        SetCursorInValid();
                    }
                }
                else
                {
                    Debug.Log($"crop is nil");
                    SetCursorInValid();
                }
                break;
            case ItemType.CollectTool:
                if (curCrop == null)
                {
                    SetCursorInValid();
                    break;
                }

                if (!curCrop.CheckToolAvailable(_curItem.itemID))
                {
                    SetCursorInValid();
                    break;
                }
                if (curTile.growthDays >= curCrop.TotalGrowthDays) SetCursorValid(); else SetCursorInValid();
                break;
            case ItemType.ReapTool:
                if (GridMapManager.Instance.HaveReapableItemsInRadius(_mouseWorldPos, _curItem)) SetCursorValid(); else SetCursorInValid();
                break;
            case ItemType.Furniture:
                _buildImage.gameObject.SetActive(true);
                var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(_curItem.itemID);

                if (curTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(_curItem.itemID) && !HaveFurnitureInRadius(bluePrintDetails))
                    SetCursorValid();
                else
                    SetCursorInValid();
                break;
        }
    }
    
    private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
    {
        var buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = _mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;

        var otherColl = Physics2D.OverlapBox(point, size, 0);
        if (otherColl != null)
            return otherColl.GetComponent<Furniture>();
        return false;
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