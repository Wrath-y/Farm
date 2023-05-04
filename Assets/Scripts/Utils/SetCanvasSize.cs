using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeChatWASM;

public class SetCanvasSize : MonoBehaviour
{
    public Canvas[] canvasList;

    void Start()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float designWidth = 640; // 设计时使用的屏幕宽度
        float designHeight = 360; // 设计时使用的屏幕高度
        float ratio = Mathf.Min(screenWidth / designWidth, screenHeight / designHeight);
        Debug.Log($"{Screen.width} {Screen.height} {ratio}");
        foreach (Canvas canvas in canvasList)
        {
            canvas.scaleFactor = ratio;
        }
    }
}
