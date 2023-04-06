using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    public TileDetails tileDetails;
    private int _harvestActionCount;
    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;
    private Animator _anim;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;

    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        Debug.Log(tile.seedItemId);
        tileDetails = tile;
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
                if (PlayerTransform.position.x < transform.position.x)
                {
                    _anim.SetTrigger("RotateRight");
                }
                else
                {
                    _anim.SetTrigger("RotateLeft");
                }
            }
            // 播放粒子
            if (cropDetails.hasParticleEffect)
            {
                EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos);
            }
            //播放声音
            if (cropDetails.soundEffect != SoundName.none)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
        }

        if (_harvestActionCount >= requireActionCount)
        {
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                // 生成农作物
                SpawnHarvestItems();
            } else if (cropDetails.hasAnimation)
            {
                if (PlayerTransform.position.x < transform.position.x)
                {
                    _anim.SetTrigger("FallingRight");
                }
                else
                {
                    _anim.SetTrigger("FallingLeft");
                }
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }
        }
    }

    // 播完动画后生成果实
    private IEnumerator HarvestAfterAnimation()
    {
        while (!_anim.GetCurrentAnimatorStateInfo(0).IsName("End"))
        {
            yield return null;
        }
        SpawnHarvestItems();
        
        // 转换新物体
        if (cropDetails.transferItemID > 0)
        {
            CreateTransferCrop();
        }
    }

    // 生成转换的新物体
    private void CreateTransferCrop()
    {
        tileDetails.seedItemId = cropDetails.transferItemID;
        tileDetails.daysSinceLastHarvest = -1;
        tileDetails.growthDays = 0;
        
        EventHandler.CallRefreshCurrentMap();
    }
    
    // 生成果实
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
                    // 世界地图上生成物品
                    // 判断应该生成的物品方向
                    var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;
                    // 一定范围内的随机坐标
                    var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRaduis.x * dirX),
                        transform.position.y + Random.Range(-cropDetails.spawnRaduis.y, cropDetails.spawnRaduis.y), 0);
                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                }
            }
        }

        if (tileDetails != null)
        {
            tileDetails.daysSinceLastHarvest++;
            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes)
            {
                // 可以重复生长
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                EventHandler.CallRefreshCurrentMap();
            }
            else
            {
                // 不可以重复生长
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemId = -1;
                tileDetails.growthDays = -1;
                // TODO 可重置土地挖坑状态
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("crop tiledetails is nil");
        }
    }
}
