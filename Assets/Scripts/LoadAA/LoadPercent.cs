using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LoadAA
{
    public interface LoadPercent
    {
        IEnumerator RegisterLoadPercent()
        {
            while (AAManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            AAManager.Instance.RegisterLoadPercent(this);
        }

        public IEnumerator AddHandle(string key, AsyncOperationHandle handle);
        public Dictionary<string, AsyncOperationHandle> GetHandles();
        
        public IEnumerator Percent()
        {
            Dictionary<string, AsyncOperationHandle> handles = GetHandles();
            foreach (var item in handles)
            {
                var downloadRes = item.Value.GetDownloadStatus();
                float progress = item.Value.PercentComplete; // 获取加载进度
                var percentage = downloadRes.Percent;
                while (!item.Value.IsDone)
                {
                    Debug.Log($"Resource {item.Key} loading progress: {progress * 100}%");
                    // Debug.Log($"Resource {item.Key} downloading progress: {percentage * 100}%");
                    // Debug.Log($"Resource {item.Key} total bytes: {downloadRes.TotalBytes}, downloading bytes: {downloadRes.DownloadedBytes}");
                    yield return null;
                }
                AAManager.Instance.doneResourceNum++;
                Debug.Log($"Resource {item.Key} loading progress: {progress * 100}%");
                Debug.Log($"{AAManager.Instance.doneResourceNum} / {AAManager.Instance.allResourceNum}");
                // Debug.Log($"Resource {item.Key} downloading progress: {percentage * 100}%");
                // Debug.Log($"Resource {item.Key} total bytes: {downloadRes.TotalBytes}, downloading bytes: {downloadRes.DownloadedBytes}");
                Addressables.Release(item.Value);
            }
        }
    }
}