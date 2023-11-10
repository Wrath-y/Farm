using System.Collections;
using Cursor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameObject _menuCanvas;
    private GameObject _menuPrefab;

    public AssetReference menuPrefabRef;
    public GameObject gameTimeBox;
    public Button acceleratedTimeButton;
    public Button settingsBtn;
    public GameObject actionBtn;
    public GameObject pausePanel;
    public Slider volumeSlider;


    private void Awake()
    {
        ActionBtnManager.Instance.actionBtn = actionBtn;
        actionBtn.GetComponent<Button>().onClick.AddListener(ActionBtnManager.Instance.Click);
        acceleratedTimeButton.onClick.AddListener(TimeManager.Instance.AcceleratedTime);
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        settingsBtn.onClick.AddListener(TogglePausePanel);

        _menuCanvas = GameObject.FindWithTag("MenuCanvas");
        menuPrefabRef.LoadAssetAsync<GameObject>().Completed += obj =>
        {
            _menuPrefab = obj.Result;
            Instantiate(_menuPrefab, _menuCanvas.transform);
            SceneManager.UnloadSceneAsync("Boot");
        };
        
        gameTimeBox.SetActive(true);
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
        Instantiate(_menuPrefab, _menuCanvas.transform);
        pausePanel.SetActive(false);
    }
    
    public void SaveGame()
    {
        Time.timeScale = 1;
        EventHandler.CallSaveGameEvent();
        pausePanel.SetActive(false);
    }
}
