using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class TimeUI : MonoBehaviour
{
    public List<string> aaLoadkeys;
    private Dictionary<string, AsyncOperationHandle> _operationDictionary;
    public UnityEvent ready;
    
    // public RectTransform dayNightImage;
    // public RectTransform clockParent;
    public Image seasonImage;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI timeText;
    private Sprite[] _seasonSprites = new Sprite[4];

    private List<GameObject> _clockBlocks = new List<GameObject>();
    
    protected void Awake()
    {
        // for (int i = 0; i < clockParent.childCount; i++)
        // {
        //     _clockBlocks.Add(clockParent.GetChild(i).gameObject);
        //     clockParent.GetChild(i).gameObject.SetActive(false);
        // }
        ready.AddListener(OnAssetsReady);
        StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
    }
        
    private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys)
    {
        if (_operationDictionary == null)
            _operationDictionary = new Dictionary<string, AsyncOperationHandle>();

        AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(Sprite));
        yield return locations;

        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        foreach (IResourceLocation location in locations.Result) {
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(location);
            handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        ready.Invoke();
    }
        
    private void OnAssetsReady()
    {
        int index = 0;
        foreach (var item in _operationDictionary)
        {
            _seasonSprites[index] = (Sprite)item.Value.Result;
            index++;
        }
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
        seasonImage.sprite = _seasonSprites[(int)season];
        
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
