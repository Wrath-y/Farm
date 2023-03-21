using System;
using System.Collections;
using System.Collections.Generic;
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
        EventHandler.ItemSelected += OnItemSelected;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelected -= OnItemSelected;
    }

    private void OnItemSelected(ItemDetails itemDetails, bool isSelected)
    {
        PartType curType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            _ => PartType.None
        };

        if (!isSelected)
        {
            curType = PartType.None;
            holdItem.gameObject.SetActive(false);
        }
 
        if (isSelected && curType == PartType.Carry)
        {
            holdItem.sprite = itemDetails.itemOnWorldSprite;
            holdItem.gameObject.SetActive(true);
        }
        
        SwitchAnimator(curType);
    }

    private void SwitchAnimator(PartType partType)
    {
        foreach (var animatorType in animatorTypes)
        {
            if (animatorType.partType != partType) continue;
            _animatorNameDict[animatorType.partName.ToString()].runtimeAnimatorController =
                animatorType.overrideController;
        }
    }
}
