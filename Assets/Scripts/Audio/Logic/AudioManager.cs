using System;
using System.Collections;
using System.Collections.Generic;
using LoadAA;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : Singleton<AudioManager>, LoadPercent
{
    private Dictionary<string, AsyncOperationHandle> _aaHandles = new Dictionary<string, AsyncOperationHandle>();
    public List<string> aaLoadkeys;
    private Dictionary<string, AsyncOperationHandle<ScriptableObject>> _operationDictionary;
    public UnityEvent ready;
    
    [Header("音乐数据库")]
    public SoundDetailsList_SO soundDetailsData;
    public SceneSoundList_SO sceneSoundData;
    [Header("Audio Source")]
    public AudioSource ambientSource;
    public AudioSource gameSource;

    private Coroutine _soundRoutine;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Snapshots")]
    public AudioMixerSnapshot normalSnapShot;
    public AudioMixerSnapshot ambientSnapShot;
    public AudioMixerSnapshot muteSnapShot;
    private float _musicTransitionSecond = 8f;
    
    public float MusicStartSecond => Random.Range(5f, 15f);

    protected override void Awake()
    {
        base.Awake();
        LoadPercent aa = this;
        aa.RegisterLoadPercent();
        
        ready.AddListener(OnAssetsReady);
        StartCoroutine(LoadAndAssociateResultWithKey(aaLoadkeys));
    }
    
    private IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
        if (_operationDictionary == null)
            _operationDictionary = new Dictionary<string, AsyncOperationHandle<ScriptableObject>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(ScriptableObject));

        yield return locations;
        
        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        LoadPercent aa = this;
        foreach (IResourceLocation location in locations.Result) {
            AsyncOperationHandle<ScriptableObject> handle = Addressables.LoadAssetAsync<ScriptableObject>(location);
            handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
            
            aa.AddHandle(location.PrimaryKey, handle);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        ready.Invoke();
    }
    
    private void OnAssetsReady() {
        LoadPercent aa = this;
        foreach (var item in _operationDictionary) {
            switch (item.Key)
            {
                case "SoundDetailsList_SO":
                    soundDetailsData = (SoundDetailsList_SO)item.Value.Result;
                    for (int i = 0; i < soundDetailsData.soundDetailsList.Count; i++)
                    {
                        if (soundDetailsData.soundDetailsList[i].soundClipRef == null) continue;

                        int index = i;

                        var handle = soundDetailsData.soundDetailsList[i].soundClipRef.LoadAssetAsync<AudioClip>();
                        handle.Completed += (obj) =>
                        {
                            soundDetailsData.soundDetailsList[index].soundClip = obj.Result;
                        };
                        aa.AddHandle($"SoundDetailsList_SO_{i}", handle);
                    }
                    break;
                case "SceneSoundList_SO":
                    sceneSoundData = (SceneSoundList_SO)item.Value.Result;
                    break;
            }
        }
    }
    
    private void OnEnable()
    {
        EventHandler.AfterLoadedSceneEvent += OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterLoadedSceneEvent -= OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }
    
    private void OnApplicationQuit()
    {
        for (int i = 0; i < soundDetailsData.soundDetailsList.Count; i++)
        {
            soundDetailsData.soundDetailsList[i].soundClip = null;
        }
    }

    private void OnEndGameEvent()
    {
        if (_soundRoutine != null)
            StopCoroutine(_soundRoutine);
        muteSnapShot.TransitionTo(1f);
    }

    private void OnPlaySoundEvent(SoundName soundName)
    {
        var soundDetails = soundDetailsData.GetSoundDetails(soundName);
        if (soundDetails != null)
            EventHandler.CallInitSoundEffect(soundDetails);
    }

    private void OnAfterSceneLoadedEvent()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        SceneSoundItem sceneSound = sceneSoundData.GetSceneSoundItem(currentScene);
        if (sceneSound == null)
        {
            Debug.Log("sceneSound is nil");
            return;
        }

        SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.ambient);
        SoundDetails music = soundDetailsData.GetSoundDetails(sceneSound.music);

        if (_soundRoutine != null)
            StopCoroutine(_soundRoutine);
        _soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));
    }


    private IEnumerator PlaySoundRoutine(SoundDetails music, SoundDetails ambient)
    {
        if (music != null && ambient != null)
        {
            PlayAmbientClip(ambient, 1f);
            yield return new WaitForSeconds(MusicStartSecond);
            PlayMusicClip(music, _musicTransitionSecond);
        }
    }

    // 播放背景音乐
    private void PlayMusicClip(SoundDetails soundDetails, float transitionTime)
    {
        audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));
        gameSource.clip = soundDetails.soundClip;
        if (gameSource.isActiveAndEnabled)
            gameSource.Play();

        normalSnapShot.TransitionTo(transitionTime);
    }


    // 播放环境音效
    private void PlayAmbientClip(SoundDetails soundDetails, float transitionTime)
    {
        audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));
        ambientSource.clip = soundDetails.soundClip;
        if (ambientSource.isActiveAndEnabled)
            ambientSource.Play();

        ambientSnapShot.TransitionTo(transitionTime);
    }


    private float ConvertSoundVolume(float amount)
    {
        return (amount * 100 - 80);
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", (value * 100 - 80));
    }
    
    public void AddHandle(string key, AsyncOperationHandle handle)
    {
        AAManager.Instance.allResourceNum++;
        _aaHandles.Add(key, handle);
    }

    public Dictionary<string, AsyncOperationHandle> GetHandles()
    {
        return _aaHandles;
    }
}
