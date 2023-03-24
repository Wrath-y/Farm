using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed;

    private Sprite _curSprite;
    private Image _cursorImage;
    private RectTransform _cursorCanvas;

    private void Start()
    {
        _cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        _cursorImage = _cursorCanvas.GetChild(0).GetComponent<Image>();
        
        SetCursorImage(normal);
    }

    private void Update()
    {
        if (_cursorCanvas == null)
        {
            return;
        }

        _cursorImage.transform.position = Input.mousePosition;
    }

    private void SetCursorImage(Sprite sprite)
    {
        _cursorImage.sprite = sprite;
        _cursorImage.color = new Color(1, 1, 1, 1);
    }
}
