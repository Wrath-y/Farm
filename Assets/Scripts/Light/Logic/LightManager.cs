using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private LightControl[] _sceneLights;
    private LightShift _currentLightShift;
    private Season _currentSeason;
    private float _timeDifference = Settings.lightChangeDuration;

    private void OnEnable()
    {
        EventHandler.AfterLoadedSceneEvent += OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterLoadedSceneEvent -= OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        _currentLightShift = LightShift.Morning;
    }

    private void OnAfterSceneLoadedEvent()
    {
        _sceneLights = FindObjectsOfType<LightControl>();

        foreach (LightControl light in _sceneLights)
        {
            //lightcontrol 改变灯光的方法
            light.ChangeLightShift(_currentSeason, _currentLightShift, _timeDifference);
        }
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        Debug.Log("OnLightShiftChangeEvent");
        _currentSeason = season;
        _timeDifference = timeDifference;
        if (_currentLightShift != lightShift)
        {
            _currentLightShift = lightShift;

            foreach (LightControl light in _sceneLights)
            {
                //lightcontrol 改变灯光的方法
                light.ChangeLightShift(_currentSeason, _currentLightShift, timeDifference);
            }
        }
    }

}