using System;
using Cysharp.Threading.Tasks;
using JangLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetDownloader
{
    /// <summary>
    /// 지정된 레이블에 대해 다운로드가 필요한 총 크기를 비동기적으로 확인합니다.
    /// </summary>
    /// <returns>성공 여부와 다운로드 크기(byte)를 반환합니다.</returns>
    public static async UniTask<(bool success, long sizeInBytes)> GetDownloadSizeAsync(AssetLabelReference label)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(label);

        try
        {
            await sizeHandle.ToUniTask();
            long size = sizeHandle.Result;
            return (true, size);
        }
        catch (Exception e)
        {
            EditorLog.LogError($"Addressable 다운로드 크기 확인 실패: {e.Message}");
            return (false, 0);
        }
        finally
        {
            Addressables.Release(sizeHandle);
        }
    }

    /// <summary>
    /// 지정된 레이블의 모든 에셋 다운로드를 시작합니다.
    /// </summary>
    /// <returns>다운로드 성공 여부</returns>
    public static async UniTask<bool> StartDownloadAsync(AssetLabelReference label, Action<float> onProgress)
    {
        EditorLog.Log($"Addressable 에셋 다운로드 시작: {label.labelString}");

        var downloadHandle = Addressables.DownloadDependenciesAsync(label, false);

        try
        {
            await downloadHandle.ToUniTask((MonoBehaviour)Progress.Create<float>(p => onProgress?.Invoke(p)));
            return downloadHandle.Status == AsyncOperationStatus.Succeeded;
        }
        catch (Exception e)
        {
            EditorLog.LogError($"다운로드 중 예외 발생: {e.Message}");
            return false;
        }
        finally
        {
            Addressables.Release(downloadHandle);
        }
    }
}
