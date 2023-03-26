using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, commodity;

    private Sprite _curSprite;
    private Image _cursorImage;
    private RectTransform _cursorCanvas;

    private Camera _mainCamera;
    private Grid _curGrid;
    private Vector3 _mouseWorldPos;
    private Vector3Int _mouseGridPos;
    private bool _cursorEnable;
    private bool _cursorPositionValid;
    private ItemDetails _curItem;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;

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
        _cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        _cursorImage = _cursorCanvas.GetChild(0).GetComponent<Image>();
        _curSprite = normal;
        SetCursorImage(normal);
        
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_cursorCanvas == null)
        {
            return;
        }

        _cursorImage.transform.position = Input.mousePosition;
        if (!InteractWithUI() && _cursorEnable)
        {
            SetCursorImage(_curSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(normal);
        }
    }

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && _cursorPositionValid)
        {
            EventHandler.CallMouseClickedEvent(_mouseWorldPos, _curItem);
        }
    }

    private void SetCursorImage(Sprite sprite)
    {
        _cursorImage.sprite = sprite;
        _cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void SetCursorValid()
    {
        _cursorImage.color = new Color(1, 1, 1, 1);
        _cursorPositionValid = true;
    }

    private void SetCursorInValid()
    {
        _cursorImage.color = new Color(1, 0, 0, 0.5f);
        _cursorPositionValid = false;
    }

    private bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        return false;
    }

    private void CheckCursorValid()
    {
        _mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));
        _mouseGridPos = _curGrid.WorldToCell(_mouseWorldPos);
        var playerGridPos = _curGrid.WorldToCell(PlayerTransform.position);
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
        
        switch (_curItem.itemType)
        {
            case ItemType.Commodity:
                if (curTile.canDropItem && _curItem.canDropped) SetCursorValid(); else SetCursorInValid(); 
                break;
        }
    }
    
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            _cursorEnable = false;
            _curSprite = normal;
            return;
        }

        _curItem = itemDetails;
        _curSprite = itemDetails.itemType switch
        {
            ItemType.Seed => seed,
            ItemType.ChopTool => tool,
            ItemType.Commodity => commodity,
            _ => normal
        };
        _cursorEnable = true;
    }

    private void OnBeforeUnloadSceneEvent()
    {
        _cursorEnable = false;
    }
    
    private void OnAfterLoadedSceneEvent()
    {
        _curGrid = FindObjectOfType<Grid>();
    }
}
