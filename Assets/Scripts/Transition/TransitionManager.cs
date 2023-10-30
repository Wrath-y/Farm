using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Farm.Transition
{
    public class TransitionManager : Singleton<TransitionManager>, ISaveable
    {
        [SceneName]
        public string startSceneName = "";

        private bool _isFaded;
        private CanvasGroup _fadeCanvasGroup;
        public string GUID => GetComponent<DataGUID>().guid;

        protected override void Awake()
        {
            base.Awake();
            // Screen.SetResolution(1920, 1080, FullScreenMode.Windowed, 0);
            // SceneManager.LoadScene("UI", LoadSceneMode.Additive);
            Addressables.LoadSceneAsync("UI", LoadSceneMode.Additive);
        }
        
        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            _fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }

        private void OnTransitionEvent(string targetSceneName, Vector3 targetPos)
        {
            Debug.Log("transition");
            if (!_isFaded)
            {
                StartCoroutine(Transition(targetSceneName, targetPos));
            }
        }
        
        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private IEnumerator Transition(string sceneName, Vector3 targetPos)
        {
            EventHandler.CallBeforeUnloadSceneEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            yield return LoadSceneSetActive(sceneName);
            //移动人物坐标
            EventHandler.CallMoveToPos(targetPos);
            EventHandler.CallAfterLoadedSceneEvent();
            yield return Fade(0);
        }

        // 加载场景并设置为激活
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            // yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            var opHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return opHandle;
        
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newScene);
        }

        // 淡入淡出场景
        // targetAlpha 1是黑，0是透明
        private IEnumerator Fade(float targetAlpha)
        {
            _isFaded = true;
            _fadeCanvasGroup.blocksRaycasts = true;
            float speed = Mathf.Abs(_fadeCanvasGroup.alpha - targetAlpha) / Settings.FadeDuration;

            while (!Mathf.Approximately(_fadeCanvasGroup.alpha, targetAlpha))
            {
                _fadeCanvasGroup.alpha = Mathf.MoveTowards(_fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }
            
            _fadeCanvasGroup.blocksRaycasts = false;
            _isFaded = false;
        }
        
        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1f);

            if (SceneManager.GetActiveScene().name != "PersistentScene")    //在游戏过程中 加载另外游戏进度
            {
                EventHandler.CallBeforeUnloadSceneEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterLoadedSceneEvent();
            yield return Fade(0);
        }


        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeUnloadSceneEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }



        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }
}
