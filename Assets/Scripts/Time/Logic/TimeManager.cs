using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
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

    private void Awake()
    {
        NewGameTime();
    }

    private void Start()
    {
        EventHandler.CallGameMinuteEvent(_gameTime.Minute, _gameTime.Hour);
        EventHandler.CallGameDateEvent(_gameTime.Hour, _gameTime.Day, _gameTime.Month, _gameTime.Year, _gameTime.Season);
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
            _tikTime -= Settings.SecondThreshold;
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
            _gameTime.Day++;
            EventHandler.CallGameDayEvent(_gameTime.Day, _gameTime.Season);
            EventHandler.CallGameDateEvent(_gameTime.Hour, _gameTime.Day, _gameTime.Month, _gameTime.Year, _gameTime.Season);
        }
    }

    private void NewGameTime()
    {
        _gameTime = new DateTimeData
        {
            Year = 2022,
            Month = 1,
            Day = 1,
            Hour = 7,
            Season = Season.春天,
            MonthInSeason = 3
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
                        EventHandler.CallGameDayEvent(_gameTime.Day, _gameTime.Season);
                    }
                }
                EventHandler.CallGameDateEvent(_gameTime.Hour, _gameTime.Day, _gameTime.Month, _gameTime.Year, _gameTime.Season);
            }
            EventHandler.CallGameMinuteEvent(_gameTime.Minute, _gameTime.Hour);
        }
    }
}
