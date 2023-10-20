using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject _menuCanvas;
    public GameObject menuPrefab;

    public Button acceleratedTimeButton;
    public Button mobileSettingsBtn;
    public Button settingsBtn;
    public GameObject pausePanel;
    public Slider volumeSlider;


    private void Awake()
    {
        if (GameObject.FindWithTag("JoyStick").GetComponent<VariableJoystick>() == null)
        {
            settingsBtn.onClick.AddListener(TogglePausePanel);
        }
        else
        {
            mobileSettingsBtn.onClick.AddListener(TogglePausePanel);
        }
        acceleratedTimeButton.onClick.AddListener(TimeManager.Instance.AcceleratedTime);
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }

    private void OnEnable()
    {
        EventHandler.AfterLoadedSceneEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterLoadedSceneEvent -= OnAfterSceneLoadedEvent;
    }


    private void Start()
    {
        RectTransform joyStickBoxRect = GameObject.FindWithTag("JoyStickBox").GetComponent<RectTransform>();
        // 是安卓平板
        if ((float)Screen.width / Screen.height < 1.7f)
        {
            Vector2 anPos = joyStickBoxRect.anchoredPosition;
            anPos.x = 40;
            anPos.y = 60;
            joyStickBoxRect.anchoredPosition = anPos;
        }
        // 是Ipad
        if ((float)Screen.width / Screen.height < 1.5f)
        {
            Vector2 anPos = joyStickBoxRect.anchoredPosition;
            anPos.x = 40;
            anPos.y = 60;
            joyStickBoxRect.anchoredPosition = anPos;
        }
        _menuCanvas = GameObject.FindWithTag("MenuCanvas");
        Instantiate(menuPrefab, _menuCanvas.transform);
    }
    private void OnAfterSceneLoadedEvent()
    {
        if (_menuCanvas.transform.childCount > 0)
            Destroy(_menuCanvas.transform.GetChild(0).gameObject);
    }

    public void TogglePausePanel()
    {
        bool isOpen = pausePanel.activeInHierarchy;

        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
        else
        {
            System.GC.Collect();
            pausePanel.SetActive(true);
            Time.timeScale = 0;
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        }
    }


    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        Instantiate(menuPrefab, _menuCanvas.transform);
        pausePanel.SetActive(false);
    }
    
    public void SaveGame()
    {
        Time.timeScale = 1;
        EventHandler.CallSaveGameEvent();
        pausePanel.SetActive(false);
    }
}
