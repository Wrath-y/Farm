using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    private TileDetails _tileDetails;
    private int _harvestActionCount;
    private Animator _anim;
    private Transform playerTransform => FindObjectOfType<Player>().transform;

    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        Debug.Log(tile.seedItemId);
        _tileDetails = tile;
        // 工具使用次数
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

        _anim = GetComponentInChildren<Animator>();

        // 点击计数器
        if (_harvestActionCount < requireActionCount)
        {
            _harvestActionCount++;
            // 判断是否有动画、树木
            if (_anim != null && cropDetails.hasAnimation)
            {
                if (playerTransform.position.x < transform.position.x)
                {
                    _anim.SetTrigger("RotateRight");
                }
                else
                {
                    _anim.SetTrigger("RotateLeft");
                }
            }
            // TODO 播放粒子
            // TODO 播放声音
        }

        if (_harvestActionCount >= requireActionCount)
        {
            if (cropDetails.generateAtPlayerPosition)
            {
                // 生成农作物
                SpawnHarvestItems();
            } else if (cropDetails.hasAnimation)
            {
                
            }
        }
    }

    public void SpawnHarvestItems()
    {
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;
            if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
            {
                amountToProduce = cropDetails.producedMinAmount[i];
            }
            else
            {
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
            }
            
            // 生成指定数量的物品
            for (int j = 0; j < amountToProduce; j++)
            {
                if (cropDetails.generateAtPlayerPosition)
                {
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }
                else
                {
                    // TODO 世界地图上生成物品
                    
                }
            }
        }

        if (_tileDetails != null)
        {
            _tileDetails.daysSinceLastHarvest++;
            if (cropDetails.daysToRegrow > 0 && _tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes)
            {
                // 可以重复生长
                _tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                EventHandler.CallRefreshCurrentMap();
            }
            else
            {
                // 不可以重复生长
                _tileDetails.daysSinceLastHarvest = -1;
                _tileDetails.seedItemId = -1;
                _tileDetails.growthDays = -1;
                // TODO 可重置土地挖坑状态
            }
            Destroy(gameObject);
        }
    }
}
