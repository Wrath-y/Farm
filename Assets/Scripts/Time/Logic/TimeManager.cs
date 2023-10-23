using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>, ISaveable
{
    private struct DateTimeData
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public Season Season;
        public int MonthInSeason;
    }

    private DateTimeData _gameTime;
    private bool _gamePause;
    private float _tikTime;
    private bool _isResetSecondThreshold = false;
    
    //灯光时间差
    private float timeDifference;
    
    public TimeSpan GameTime => new TimeSpan(_gameTime.Hour, _gameTime.Minute, _gameTime.Second);
    
    public string GUID => GetComponent<DataGUID>().guid;

    private void OnEnable()
    {
        EventHandler.BeforeUnloadSceneEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
        _gamePause = true;
    }

    private void Update()
    {
        if (_gamePause)
        {
            return;
        }

        _tikTime += Time.deltaTime;
        if (_tikTime > Settings.SecondThreshold)
        {
            _tikTime = 0f;
            UpdateGameTime();
        }

        if (Input.GetKey(KeyCode.T))
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }

        if (Input.GetKey(KeyCode.G))
        {
            // 时间过去10天
            AcceleratedTime();
        }
    }
    
    private void OnAfterSceneLoadedEvent()
    {
        _gamePause = false;
        EventHandler.CallGameDateEvent(_gameTime.Hour, _gameTime.Day, _gameTime.Month, _gameTime.Year, _gameTime.Season);
        EventHandler.CallGameMinuteEvent(_gameTime.Minute, _gameTime.Hour, _gameTime.Day, _gameTime.Season);
        //切换灯光
        EventHandler.CallLightShiftChangeEvent(_gameTime.Season, GetCurrentLightShift(), timeDifference);

    }

    private void OnBeforeSceneUnloadEvent()
    {
        _gamePause = true;
    }
    
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        _gamePause = gameState == GameState.Pause;
    }
    
    private void OnEndGameEvent()
    {
        _gamePause = true;
    }
    private void OnStartNewGameEvent(int obj)
    {
        NewGameTime();
        _gamePause = false;
    }

    private void NewGameTime()
    {
        _gameTime = new DateTimeData
        {
            Year = 2023,
            Month = 1,
            Day = 1,
            Hour = 7,
            Season = Season.春天,
        };
    }

    private void UpdateGameTime()
    {
        _gameTime.Second++;
        if (_gameTime.Second > Settings.SecondHold)
        {
            _gameTime.Minute++;
            _gameTime.Second = 0;

            if (_gameTime.Minute > Settings.MinuteHold)
            {
                _gameTime.Hour++;
                _gameTime.Minute = 0;

                if (_gameTime.Hour > Settings.HourHold)
                {
                    _gameTime.Day++;
                    _gameTime.Hour = 0;

                    if (_gameTime.Day > Settings.DayHold)
                    {
                        _gameTime.Month++;
                        _gameTime.Day = 1;

                        if (_gameTime.Month > 12)
                        {
                            _gameTime.Month = 1;
                        }

                        _gameTime.MonthInSeason--;
                        if (_gameTime.MonthInSeason == 0)
                        {
                            _gameTime.MonthInSeason = 3;
                            
                            int seasonNumber = (int)_gameTime.Season + 1;
                            if (seasonNumber > Settings.SeasonHold)
                            {
                                seasonNumber = 0;
                                _gameTime.Year++;
                            }
                            _gameTime.Season = (Season)seasonNumber;
                        }
                    }
                    EventHandler.CallGameDayEvent(_gameTime.Day, _gameTime.Season);
                }
                EventHandler.CallGameDateEvent(_gameTime.Hour, _gameTime.Day, _gameTime.Month, _gameTime.Year, _gameTime.Season);
            }
            EventHandler.CallGameMinuteEvent(_gameTime.Minute, _gameTime.Hour, _gameTime.Day, _gameTime.Season);
            //切换灯光
            EventHandler.CallLightShiftChangeEvent(_gameTime.Season, GetCurrentLightShift(), timeDifference);
        }
    }
    
    // 返回lightshift同时计算时间差
    private LightShift GetCurrentLightShift()
    {
        if (GameTime >= Settings.morningTime && GameTime < Settings.nightTime)
        {
            timeDifference = (float)(GameTime - Settings.morningTime).TotalMinutes;
            return LightShift.Morning;
        }

        if (GameTime < Settings.morningTime || GameTime >= Settings.nightTime)
        {
            timeDifference = Mathf.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
            return LightShift.Night;
        }

        return LightShift.Morning;
    }

    public void AcceleratedTime()
    {
        for (int i = 0; i < 10; i++)
        {
            _gameTime.Day++;
            UpdateGameTime();
            EventHandler.CallGameMinuteEvent(_gameTime.Minute, _gameTime.Hour, _gameTime.Day, _gameTime.Season);
            EventHandler.CallGameDateEvent(_gameTime.Hour, _gameTime.Day, _gameTime.Month, _gameTime.Year, _gameTime.Season);
            EventHandler.CallGameDayEvent(_gameTime.Day, _gameTime.Season);
        }
    }
    
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("gameYear", _gameTime.Year);
        saveData.timeDict.Add("gameSeason", (int)_gameTime.Season);
        saveData.timeDict.Add("gameMonth", _gameTime.Month);
        saveData.timeDict.Add("gameDay", _gameTime.Day);
        saveData.timeDict.Add("gameHour", _gameTime.Hour);
        saveData.timeDict.Add("gameMinute", _gameTime.Minute);
        saveData.timeDict.Add("gameSecond", _gameTime.Second);

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        _gameTime.Year = saveData.timeDict["gameYear"];
        _gameTime.Season = (Season)saveData.timeDict["gameSeason"];
        _gameTime.Month = saveData.timeDict["gameMonth"];
        _gameTime.Day = saveData.timeDict["gameDay"];
        _gameTime.Hour = saveData.timeDict["gameHour"];
        _gameTime.Minute = saveData.timeDict["gameMinute"];
        _gameTime.Second = saveData.timeDict["gameSecond"];
    }
}
