using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightPattenList_SO", menuName = "Light/LightPatten")]
public class LightPattenList_SO : ScriptableObject
{
    public List<LightDetails> lightPattenList;

    // 根据季节和周期返回灯光详情
    // season 季节
    //lightShift 周期
    public LightDetails GetLightDetails(Season season, LightShift lightShift)
    {
        return lightPattenList.Find(l => l.season == season && l.lightShift == lightShift);
    }
}


[System.Serializable]
public class LightDetails
{
    public Season season;
    public LightShift lightShift;
    public Color lightColor;
    public float lightAmount;
}