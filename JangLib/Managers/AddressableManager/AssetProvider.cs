using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JangLib;
using Cysharp.Threading.Tasks;

public static class AssetProvider
{
    private static bool isInitialized = false;
    private static Dictionary<string, AsyncOperationHandle> loadedHandles = new Dictionary<string, AsyncOperationHandle>();

    /// <summary>
    /// Addressables 시스템 초기화합니다.
    /// </summary>
    public static async UniTask<bool> InitializeAsync()
    {
        if (isInitialized)
        {
            EditorLog.Log("Addressables는 이미 초기화되었습니다.");
            return true;
        }

        var initHandle = Addressables.InitializeAsync();
        await initHandle.ToUniTask();

        if (initHandle.Status == AsyncOperationStatus.Succeeded)
        {
            isInitialized = true;
            EditorLog.Log("Addressables 초기화 성공");
            return true;
        }
        else
        {
            // 에러 로깅
            EditorLog.LogError($"Addressables 초기화 실패: {initHandle.OperationException?.Message}");
            return false;
        }
    }

    /// <summary>
    /// Addressable 에셋을 비동기적으로 로드
    /// </summary>
    public static void LoadAssetAsync<T>(string key, Action<T> onComplete)
    {
        if (loadedHandles.ContainsKey(key) && loadedHandles[key].IsValid())
        {
            if (loadedHandles[key].Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke((T)loadedHandles[key].Result);
                return;
            }
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        loadedHandles[key] = handle;

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"Addressable Asset 로드 실패: {key}");
            }
        };
    }

    /// <summary>
    /// 로드된 에셋을 모두 해제
    /// </summary>
    public static void ReleaseAllAssets()
    {
        foreach (var handle in loadedHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        loadedHandles.Clear();
    }

    /// <summary>
    /// 특정 키의 에셋만 해제
    /// </summary>
    public static void ReleaseAsset(string key)
    {
        if (loadedHandles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            loadedHandles.Remove(key);
        }
    }
}