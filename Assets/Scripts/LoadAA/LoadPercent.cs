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

        public void AddHandle(string key, AsyncOperationHandle handle);
        public Dictionary<string, AsyncOperationHandle> GetHandles();
        
        public IEnumerator Percent()
        {
            Dictionary<string, AsyncOperationHandle> handles = GetHandles();
            foreach (var item in handles)
            {
                while (!item.Value.IsDone)
                {
                    float progress = item.Value.PercentComplete; // 获取加载进度
                    Debug.Log($"Resource {item.Key} loading progress: {progress * 100}%");
                    var downloadRes = item.Value.GetDownloadStatus();
                    var percentage = downloadRes.Percent;
                    Debug.Log($"Resource {item.Key} downloading progress: {percentage * 100}%");
                    Debug.Log(
                        $"Resource {item.Key} total bytes: {downloadRes.TotalBytes}, downloading bytes: {downloadRes.DownloadedBytes}");
                    yield return null;
                }
            }
        }
    }
}