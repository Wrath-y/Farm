using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    public SceneRouteDataList_SO sceneRouteDate;
    public List<NPCPosition> npcPositionList;
    
    private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();

    
    protected override void Awake()
    {
        base.Awake();

        InitSceneRouteDict();
    }
    
    // 初始化路径字典
    private void InitSceneRouteDict()
    {
        if (sceneRouteDate.sceneRouteList.Count > 0)
        {
            foreach (SceneRoute route in sceneRouteDate.sceneRouteList)
            {
                var key = route.fromSceneName + route.gotoSceneName;

                if (sceneRouteDict.ContainsKey(key))
                    continue;

                sceneRouteDict.Add(key, route);
            }
        }
    }
    
    // 获得两个场景间的路径
    // fromSceneName 起始场景
    // gotoSceneName 目标场景
    public SceneRoute GetSceneRoute(string fromSceneName, string gotoSceneName)
    {
        return sceneRouteDict[fromSceneName + gotoSceneName];
    }
}
