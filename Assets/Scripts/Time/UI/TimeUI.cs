using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    // public RectTransform dayNightImage;
    // public RectTransform clockParent;
    public Image seasonImage;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI timeText;
    public Sprite[] seasonSprites;

    private List<GameObject> _clockBlocks = new List<GameObject>();

    private void Awake()
    {
        // for (int i = 0; i < clockParent.childCount; i++)
        // {
        //     _clockBlocks.Add(clockParent.GetChild(i).gameObject);
        //     clockParent.GetChild(i).gameObject.SetActive(false);
        // }
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
    }

    private void OnGameMinuteEvent(int min, int hour, int day, Season season)
    {
        timeText.text = hour.ToString("00") + ":" + min.ToString("00");
    }

    private void OnGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        dateText.text = year + "年" + month.ToString("00") + "月" + day.ToString("00") + "日";
        seasonImage.sprite = seasonSprites[(int)season];
        
        // SwitchHour(hour);
    }

    private void SwitchHour(int hour)
    {
        SwitchHourImage(hour);
        DayNightImageRotate(hour);
    }
    
    private void SwitchHourImage(int hour)
    {
        int index = hour / 4;
        if (index == 0)
        {
            foreach (var clockBlock in _clockBlocks)
            {
                clockBlock.SetActive(false);
            }
            _clockBlocks[0].SetActive(true);

            return;
        }
        for (int i = 0; i < _clockBlocks.Count; i++) 
        {
            if (i < index + 1)
            { 
                _clockBlocks[i].SetActive(true);
                
            }
            else 
            { 
                _clockBlocks[i].SetActive(false);
            }
        }
    }

    private void DayNightImageRotate(int hour)
    {
        // dayNightImage.DORotate(new Vector3(0, 0, hour * 15 - 90), 1f);
    }
}
