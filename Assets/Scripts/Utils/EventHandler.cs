
using System;
using System.Collections.Generic;
using Farm.Dialogue;
using UnityEngine;

public static class EventHandler
{
    public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> itemList)
    {
        UpdateInventoryUI?.Invoke(location, itemList);
    }

    public static event Action<int, Vector3> InstantiateItemInScene;
    public static void CallInstantiateItemInScene(int id, Vector3 pos)
    {
        InstantiateItemInScene?.Invoke(id, pos);
    }
    
    public static event Action<int, Vector3, ItemType> DropItemEvent;
    public static void CallDropItemEvent(int id, Vector3 pos, ItemType itemType)
    {
        DropItemEvent?.Invoke(id, pos, itemType);
    }

    public static event Action<ItemDetails, bool> ItemSelectedEvent;
    public static void CallItemSelected(ItemDetails itemDetails, bool isSelected)
    {
        ItemSelectedEvent?.Invoke(itemDetails, isSelected);
    }

    public static event Action<int, int, int, Season> GameMinuteEvent;
    public static void CallGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        GameMinuteEvent?.Invoke(minute, hour, day, season);
    }

    public static event Action<int, Season> GameDayEvent;
    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day, season);
    }

    public static event Action<int, int, int, int, Season> GameDateEvent;
    public static void CallGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        GameDateEvent?.Invoke(hour, day, month, year, season);
    }

    public static event Action<string, Vector3> TransitionEvent;
    public static void CallTransitionEvent(string sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }

    public static event Action BeforeUnloadSceneEvent;
    public static void CallBeforeUnloadSceneEvent()
    {
        BeforeUnloadSceneEvent?.Invoke();
    }
    
    public static event Action AfterLoadedSceneEvent;
    public static void CallAfterLoadedSceneEvent()
    {
        AfterLoadedSceneEvent?.Invoke();
    }
    
    public static event Action<Vector3> MoveToPos;
    public static void CallMoveToPos(Vector3 targetPos)
    {
        MoveToPos?.Invoke(targetPos);
    }

    public static event Action<Vector3, ItemDetails> MouseClickedEvent;
    public static void CallMouseClickedEvent(Vector3 pos, ItemDetails itemDetails)
    {
        MouseClickedEvent?.Invoke(pos, itemDetails);
    }

    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;
    public static void CallExecuteActionAfterAnimation(Vector3 pos, ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimation?.Invoke(pos, itemDetails);
    }

    public static event Action<int, TileDetails> PlantSeedEvent;
    public static void CallPlantSeedEvent(int id, TileDetails tile)
    {
        PlantSeedEvent?.Invoke(id, tile);
    }

    public static event Action<int> HarvestAtPlayerPosition;
    public static void CallHarvestAtPlayerPosition(int id)
    {
        HarvestAtPlayerPosition?.Invoke(id);
    }

    public static event Action RefreshCurrentMap;
    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }

    public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticleEffectType particleEffectType, Vector3 pos)
    {
        ParticleEffectEvent?.Invoke(particleEffectType, pos);
    }

    public static event Action GenerateCropEvent;
    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }
    
    public static event Action<DialoguePiece> ShowDialogueEvent;
    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }
}
