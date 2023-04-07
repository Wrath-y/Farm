using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject _menuCanvas;
    public GameObject menuPrefab;

    public Button settingsBtn;
    public GameObject pausePanel;
    public Slider volumeSlider;


    private void Awake()
    {
        // settingsBtn.onClick.AddListener(TogglePausePanel);
        // volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
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
        _menuCanvas = GameObject.FindWithTag("MenuCanvas");
        Instantiate(menuPrefab, _menuCanvas.transform);
    }
    private void OnAfterSceneLoadedEvent()
    {
        if (_menuCanvas.transform.childCount > 0)
            Destroy(_menuCanvas.transform.GetChild(0).gameObject);
    }

    private void TogglePausePanel()
    {
        bool isOpen = pausePanel.activeInHierarchy;

        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            System.GC.Collect();
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }


    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        Instantiate(menuPrefab, _menuCanvas.transform);
    }
}
