using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Inventory;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
    public SpriteRenderer holdItem;
    public List<AnimatorType> animatorTypes;
    private Animator[] _animators;
    private Dictionary<string, Animator> _animatorNameDict = new Dictionary<string, Animator>();

    private void Awake()
    {
        _animators = GetComponentsInChildren<Animator>();
        foreach (var animator in _animators)
        {
            _animatorNameDict.Add(animator.name, animator);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }
    
    private void SwitchAnimator(PartType partType)
    {
        foreach (var item in animatorTypes)
        {
            if (item.partType == partType)
            {
                _animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
            else if (item.partType == PartType.None)
            {
                _animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
        }
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        // TODO 不同工具返回不同动画
        PartType curType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.Furniture => PartType.Carry,
            _ => PartType.None
        };

        if (!isSelected)
        {
            curType = PartType.None;
            holdItem.enabled = false;
        }
 
        if (isSelected)
        {
            if (curType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            else
            {
                holdItem.enabled = false;
            }
        }
        
        SwitchAnimator(curType);
    }

    private void OnBeforeUnloadSceneEvent()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    private void OnHarvestAtPlayerPosition(int id)
    {
        Debug.Log("AnimatorOverride OnHarvestAtPlayerPosition id: "+id);
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(id).itemOnWorldSprite;
        Debug.Log("AnimatorOverride OnHarvestAtPlayerPosition" + InventoryManager.Instance.GetItemDetails(id).itemIcon.name + InventoryManager.Instance.GetItemDetails(id).itemOnWorldSprite.name);
        if (holdItem.enabled == false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(Settings.HoldHarvestDuration);
        holdItem.enabled = false;
    }
}
