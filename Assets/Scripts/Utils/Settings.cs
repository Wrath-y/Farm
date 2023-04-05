using System;
using UnityEngine;

public class Settings
{
    public const float ItemFadeDuration = 0.35f;
    public const float TargetAlpha = 0.45f;

    public const float SecondThreshold = 0.1f; // 数值越小时间越快
    public const int SecondHold = 59;
    public const int MinuteHold = 59;
    public const int HourHold = 23;
    public const int DayHold = 10; // 一个月有10天
    public const int SeasonHold = 3;

    public const float FadeDuration = 0.5f;

    public const float HoldHarvestDuration = 1f; // 捡起道具后举起图标的持续时间
    
    // 镰刀割草数量
    public const int ReapAmount = 2;
    // 玩家穿过草，草的摇晃时间
    public const float ReapAnimDuration = 0.04f;
    
    // NPC网格移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f;   //20*20 占 1 unit
    public const float animationBreakTime = 5f; //动画间隔时间
    public const int maxGridSize = 9999;
    
    //灯光
    public const float lightChangeDuration = 25f;
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);

    public static Vector3 playerStartPos = new Vector3(-1.7f, -5f, 0);
    public const int playerStartMoney = 100;
}
