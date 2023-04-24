using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class NPCManager : Singleton<NPCManager>
{
    public List<string> aaLoadkeys;
    private Dictionary<string, AsyncOperationHandle<SceneRouteDataList_SO>> _operationDictionary;
    public UnityEvent ready;
    
    public SceneRouteDataList_SO sceneRouteDate;
    public List<NPCPosition> npcPositionList;
    
    private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();

    
    protected override void Awake()
    {
        base.Awake();

        ready.AddListener(OnAssetsReady);
        ready.AddListener(InitSceneRouteDict);
        StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
    }
    
    private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
        if (_operationDictionary == null)
            _operationDictionary = new Dictionary<string, AsyncOperationHandle<SceneRouteDataList_SO>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(SceneRouteDataList_SO));

        yield return locations;
        
        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        foreach (IResourceLocation location in locations.Result) {
            AsyncOperationHandle<SceneRouteDataList_SO> handle = Addressables.LoadAssetAsync<SceneRouteDataList_SO>(location);
            handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        ready.Invoke();
    }
    
    private void OnAssetsReady() {
        foreach (var item in _operationDictionary) {
            switch (item.Key)
            {
                case "SceneRouteDataList_SO":
                    sceneRouteDate = item.Value.Result;
                    break;
            }
        }
    }
    
    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }
    
    private void OnStartNewGameEvent(int obj)
    {
        foreach (var character in npcPositionList)
        {
            character.npc.position = character.position;
            character.npc.GetComponent<NPCMovement>().currentScene = character.startScene;
        }
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
