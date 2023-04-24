using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

public class PoolManager : MonoBehaviour
{
    public List<string> aaLoadkeys;
    private Dictionary<string, AsyncOperationHandle<GameObject>> _operationDictionary;
    public UnityEvent ready;
    
    public List<GameObject> poolPrefabs;
    private List<ObjectPool<GameObject>> _poolEffectList = new List<ObjectPool<GameObject>>();
    private Queue<GameObject> _soundQueue = new Queue<GameObject>();

    protected void Awake()
    {
        ready.AddListener(OnAssetsReady);
        ready.AddListener(Init);
        StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
    }
    
    private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
        if (_operationDictionary == null)
            _operationDictionary = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(GameObject));

        yield return locations;
        
        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        foreach (IResourceLocation location in locations.Result) {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(location);
            handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        ready.Invoke();
    }
    
    private void OnAssetsReady() {
        foreach (var item in _operationDictionary) {
            poolPrefabs.Add(item.Value.Result);
        }
    }
    
    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += InitSoundEffect;
    }

    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
    }

    private void Init()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        foreach (GameObject item in poolPrefabs)
        {
            Transform parent = new GameObject(item.name).transform;
            parent.SetParent(transform);

            var newPool = new ObjectPool<GameObject>(
                    () => Instantiate(item, parent),
                    e => { e.SetActive(true); },
                    e => { e.SetActive(false); },
                    e => { Destroy(e); }
                    );
            
            _poolEffectList.Add(newPool);
        }
    }

    private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        // TODO 补全Prefab
        ObjectPool<GameObject> objPool = effectType switch
        {
            ParticleEffectType.LeavesFalling01 => _poolEffectList[0],
            ParticleEffectType.LeavesFalling02 => _poolEffectList[1],
            ParticleEffectType.Rock => _poolEffectList[2],
            ParticleEffectType.ReapableScenery => _poolEffectList[3],
            _ => null,
        };

        if (objPool == null)
        {
            return;
        }
        GameObject obj = objPool.Get();
        obj.transform.position = pos;
        StartCoroutine(ReleaseRoutine(objPool, obj));
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool, GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }
    
    private void CreateSoundPool()
    {
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);

        for (int i = 0; i < 20; i++)
        {
            GameObject newObj = Instantiate(poolPrefabs[4], parent);
            newObj.SetActive(false);
            _soundQueue.Enqueue(newObj);
        }
    }

    private GameObject GetPoolObject()
    {
        if (_soundQueue.Count < 2)
            CreateSoundPool();
        return _soundQueue.Dequeue();
    }

    private void InitSoundEffect(SoundDetails soundDetails)
    {
        var obj = GetPoolObject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);
        StartCoroutine(DisableSound(obj, soundDetails.soundClip.length));
    }

    private IEnumerator DisableSound(GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
        _soundQueue.Enqueue(obj);
    }
}
