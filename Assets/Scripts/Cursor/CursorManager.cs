using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, commodity;

    private Sprite _curSprite;
    private Image _cursorImage;
    private RectTransform _cursorCanvas;

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
    }

    private void Start()
    {
        _cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        _cursorImage = _cursorCanvas.GetChild(0).GetComponent<Image>();
        _curSprite = normal;
        SetCursorImage(normal);
    }

    private void Update()
    {
        if (_cursorCanvas == null)
        {
            return;
        }

        _cursorImage.transform.position = Input.mousePosition;
        if (!InteractWithUI())
        {
            SetCursorImage(_curSprite);
        }
        else
        {
            SetCursorImage(normal);
        }
    }

    private void SetCursorImage(Sprite sprite)
    {
        _cursorImage.sprite = sprite;
        _cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            _curSprite = normal;
            return;
        }
        _curSprite = itemDetails.itemType switch
        {
            ItemType.Seed => seed,
            ItemType.ChopTool => tool,
            ItemType.Commodity => commodity,
            _ => normal
        };
    }

    private bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        return false;
    }
}
