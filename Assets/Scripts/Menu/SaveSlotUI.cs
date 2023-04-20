using Farm.Save;
using UnityEngine;
using UnityEngine.UI;
public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button _currentButton;
    private DataSlot _currentData;

    private int Index => transform.GetSiblingIndex();

    private void Awake()
    {
        _currentButton = GetComponent<Button>();
        _currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }

    private void SetupSlotUI()
    {
        _currentData = SaveLoadManager.Instance.dataSlots[Index];

        if (_currentData != null)
        {
            dataTime.text = _currentData.DataTime;
            dataScene.text = _currentData.DataScene;
        }
        else
        {
            dataTime.text = "世界还没创建";
            dataScene.text = "还没开始";
        }
    }

    private void LoadGameData()
    {
        if (_currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
