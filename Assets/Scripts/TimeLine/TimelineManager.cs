using System;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector _currentDirector;

    private bool _isDone;
    public bool IsDone { set => _isDone = value; }
    private bool _isPause;
    protected override void Awake()
    {
        base.Awake();
        _currentDirector = startDirector;
    }

    private void OnEnable()
    {
        EventHandler.AfterLoadedSceneEvent += OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        startDirector.played += OnPlayed;
        startDirector.stopped += OnStopped;
    }


    private void OnDisable()
    {
        EventHandler.AfterLoadedSceneEvent -= OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }


    private void Update()
    {
        if (_isPause && Input.GetKeyDown(KeyCode.Space) && _isDone)
        {
            _isPause = false;
            _currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    private void OnStartNewGameEvent(int obj)
    {
        if (startDirector != null)
        {
            Debug.Log(startDirector.name);
            startDirector.Play();
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (!startDirector.isActiveAndEnabled)
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }

    private void OnStopped(PlayableDirector obj)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }

    private void OnPlayed(PlayableDirector obj)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }
    public void PauseTimeline(PlayableDirector director)
    {
        Debug.Log(director.name + " will pause");
        _currentDirector = director;

        _currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        _isPause = true;
    }
}
