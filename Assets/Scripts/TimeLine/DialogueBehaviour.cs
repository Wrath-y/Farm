using System.Collections;
using System.Collections.Generic;
using Farm.Dialogue;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    private PlayableDirector _director;
    public DialoguePiece dialoguePiece;

    public override void OnPlayableCreate(Playable playable)
    {
        _director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        Debug.Log(dialoguePiece.name + dialoguePiece.hasToPause);
        //呼叫启动UI
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        if (Application.isPlaying)
        {
            if (dialoguePiece.hasToPause)
            {
                //暂停timeline
                TimelineManager.Instance.PauseTimeline(_director);
            }
            else
            {
                EventHandler.CallShowDialogueEvent(null);
            }
        }
    }

    //在Timeline播放期间每帧执行
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isPlaying)
        {
            TimelineManager.Instance.IsDone = dialoguePiece.isDone;
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(null);
    }

    public override void OnGraphStart(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public override void OnGraphStop(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }
}