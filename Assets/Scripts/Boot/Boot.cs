using System.Collections;
using System.Collections.Generic;
using Farm.Transition;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WeChatWASM;

public class Boot : MonoBehaviour
{
    public Slider loadingPercent;
    public Text loadingTxt;
    
    //一个组内填写一个资源Key即可，下载时会按照资源组进行下载
    private List<string> downLoadKeyList = new List<string>()
        {
            "Default/Fantasy Cursor Pack/cursor(3).png", "DigTile", "BoxBagTemplate", "MapData_Field", "CropDataList_SO", "LeavesFalling01", "SceneSoundList_SO"
            ,"Recollection - Crystal Caverns FINAL.mp3", "AmbientSounds/countryside1.wav", "OutsideGlobalLight", "MayorShop", "Chusung-210529 SDF"
            ,"Crop/CropBase.prefab", "UI/Inventory/SlotBag.prefab", "Assets/Prefabs/UI/Menu/Panel.prefab", "KenneyUIPackRPGExpansion/arrowBeige_left.png"
            ,"Default/Icon/ui_time_12.png", "gdm-modern-and-sci-fi-icon-pack-32x32-icons/iicon_221.png", "fantasyrpgiconpack/Individual icons (16x16)/103.png"
            ,"Assets/StaticResources/Art/Maps/Collisions@20.png", "Craftland/CL_Crafting.png"
            , "PersistentScene", "UI", "01.Field", "02.Home"
        };
    
    private AsyncOperationHandle downloadDependencies;
    
    // 当前下载文件索引
    private int downLoadIndex = 0;
    // 下载完成文件个数
    private int downLoadCompleteCount = 0;
    // 下载每组资源大小
    List<long> downLoadSizeList = new  List<long>();
    // 下载资源总大小
    private long downLoadTotalSize = 0;
    // 当前下载大小
    private float curDownLoadSize = 0;
    // 下载完成
    private bool isDownLoadFinished = false;
    
    private void Awake()
    {
        // 开始预下载
        StartCoroutine(StartPreload());
    }
    
    public IEnumerator StartPreload()
    {
        Debug.Log("开始下载");
        
        // 初始化 --> 加载远端的配置文件
        yield return Addressables.InitializeAsync();
        
        // 清理缓存
        Caching.ClearCache();

        for (int i = 0; i < downLoadKeyList.Count; i++)
        {
            AsyncOperationHandle<long> size = Addressables.GetDownloadSizeAsync(downLoadKeyList[i]);
            Debug.Log("获取下载内容大小：" + size.Result);
            downLoadSizeList.Add(size.Result);
            downLoadTotalSize += size.Result;
        }

        if (downLoadTotalSize <= 0)
        {
            Debug.LogError("无可预下载内容~");
            Addressables.LoadSceneAsync("PersistentScene", LoadSceneMode.Additive).Completed += _ =>
            {
                Addressables.LoadSceneAsync("UI", LoadSceneMode.Additive).Completed += _ =>
                {
                    TransitionManager.Instance.hasLoadedUI = true;
                };
            };
            yield break;
        }
        isDownLoadFinished = true;

        for (int i = downLoadIndex; i < downLoadKeyList.Count; i++)
        {
            downloadDependencies = Addressables.DownloadDependenciesAsync(downLoadKeyList[i]);
            yield return downloadDependencies;
            if (downloadDependencies.Status == AsyncOperationStatus.Failed)
            {
                downLoadIndex = i;
                isDownLoadFinished = false;
                Debug.Log("下载失败，请重试...");
                yield break;
            }
            downLoadCompleteCount = i + 1;
        }

        Debug.Log("下载完成"); ;
    }
    
    private void Update()
    {
        // 下载是否有效
        if (isDownLoadFinished && downloadDependencies.IsValid())
        {
            curDownLoadSize = 0;
            for (int i = 0; i < downLoadSizeList.Count; i++)
            {
                if (i < downLoadCompleteCount)
                {
                    curDownLoadSize += downLoadSizeList[i];
                }
            }

            if (downLoadCompleteCount < downLoadSizeList.Count - 1)
                curDownLoadSize += downloadDependencies.GetDownloadStatus().Percent;

            float percent = curDownLoadSize * 1.0f / downLoadTotalSize;
            //Debug.Log($"共{downLoadKeyList.Count}个文件，下载到第{downLoadCompleteCount}个文件，当前文件下载进度{downloadDependencies.GetDownloadStatus().Percent}，总下载进度{percent}。");
               
            if (percent < 1)
            {
                loadingTxt.text = "加载中...(" + (percent * 100).ToString("F1") + "%)";
                loadingPercent.value = percent;
                Debug.Log($"正在下载：" + (percent * 100).ToString("F1") + "%");
            }
            else if (downloadDependencies.IsDone)
            {
                isDownLoadFinished = false;
                loadingPercent.value = 1f;
                Debug.Log("下载完成 释放句柄");
                // 下载完成释放句柄
                Addressables.Release(downloadDependencies);
                
                Addressables.LoadSceneAsync("PersistentScene", LoadSceneMode.Additive).Completed += _ =>
                {
                    Addressables.LoadSceneAsync("UI", LoadSceneMode.Additive).Completed += _ =>
                    {
                        TransitionManager.Instance.hasLoadedUI = true;
                    };
                };
            }
        }
    }
}
