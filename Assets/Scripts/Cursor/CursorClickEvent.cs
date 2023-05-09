using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorClickEvent : Singleton<CursorClickEvent>
{
    // 判断鼠标点击位置是否是NPC
    public bool IsShowNpcDialogue(Vector3 mouseWorldPos)
    {
        if (GameObject.FindGameObjectWithTag("DialogBox"))
        {
            return true;
        }
        Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].GetComponent<Dialogue>())
            {
                return true;
            }
        }

        return false;
    }
}
