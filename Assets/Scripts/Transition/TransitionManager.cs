using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Transition
{
    public class TransitionManager : MonoBehaviour
    {
        [SceneName]
        public string startSceneName = "";

        private bool _isFaded;

        private CanvasGroup _fadeCanvasGroup;

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
        }

        private IEnumerator Start()
        {
            _fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
            yield return LoadSceneSetActive(startSceneName);
            EventHandler.CallAfterLoadedSceneEvent();
        }
        
        private void OnTransitionEvent(string targetSceneName, Vector3 targetPos)
        {
            if (!_isFaded)
            {
                StartCoroutine(Transition(targetSceneName, targetPos));
            }
            
        }

        private IEnumerator Transition(string sceneName, Vector3 targetPos)
        {
            EventHandler.CallBeforeUnloadSceneEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallMoveToPos(targetPos);
            EventHandler.CallAfterLoadedSceneEvent();
            yield return Fade(0);
        }

        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newScene);
        }

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
    }
}
