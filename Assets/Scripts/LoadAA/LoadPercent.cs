using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LoadAA
{
    public interface LoadPercent
    {
        void RegisterLoadPercent()
        {
            AAManager.Instance.RegisterLoadPercent(this);
        }
        
        public void AddHandle(AsyncOperationHandle handle);
        
        public IEnumerator Percent(AsyncOperationHandle handle, string key)
        {
            while (!handle.IsDone)
            {
                float progress = handle.PercentComplete; // 获取加载进度
                Debug.Log($"Resource {key} loading progress: {progress * 100}%");
                var downloadRes = handle.GetDownloadStatus();
                var percentage = downloadRes.Percent;
                Debug.Log($"Resource {key} downloading progress: {percentage * 100}%");
                Debug.Log($"Resource {key} total bytes: {downloadRes.TotalBytes}, downloading bytes: {downloadRes.DownloadedBytes}");
                yield return null;
            }
        }
    }
}