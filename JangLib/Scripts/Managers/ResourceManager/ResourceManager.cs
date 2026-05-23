using System.Collections.Generic;
using JangLib;
using UnityEngine;

public class ResourceManager : SingletonWithMono<ResourceManager>, IBaseManager
{
    public bool IsInitialized { private set; get; }

    public readonly Dictionary<string, GameObject> _objDict = new();
    public readonly Dictionary<string, SoundDataSO> _soundDict = new();
    public readonly Dictionary<string, Sprite> _spriteDict = new();

    public void Init()
    {
        IsInitialized = true;
    }

    #region <<  GET METHODS  >>

    public GameObject GetPrefab(string path)
    {
        if (_objDict.TryGetValue(path, out GameObject prefab)) return prefab;

        GameObject obj = Resources.Load<GameObject>(path);
        if (obj == null)
        {
            AlertNull(path);
            return null;
        }

        _objDict.Add(path, obj);
        return obj;
    }

    public SoundDataSO GetSound(string path)
    {
        if (_soundDict.TryGetValue(path, out SoundDataSO sound)) return sound;

        SoundDataSO obj = Resources.Load<SoundDataSO>(path);
        if (obj == null)
        {
            AlertNull(path);
            return null;
        }

        _soundDict.Add(path, obj);
        return obj;
    }

    public Sprite GetSprite(string path)
    {
        if (_spriteDict.TryGetValue(path, out Sprite sprite)) return sprite;

        Sprite obj = Resources.Load<Sprite>(path);
        if (obj == null)
        {
            AlertNull(path);
            return null;
        }

        _spriteDict.Add(path, obj);
        return obj;
    }

    #endregion

    #region <<  UTIL  >>

    /// <summary>
    /// 프리팹 로드와 동시에 생성을 지원하는 편의 메서드
    /// </summary>
    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = GetPrefab(path);
        return prefab != null ? Instantiate(prefab, parent) : null;
    }

    /// <summary>
    /// 씬 전환 시 메모리 정리를 위해 캐시 비우기
    /// </summary>
    public void ClearAllCache()
    {
        _objDict.Clear();
        _soundDict.Clear();
        _spriteDict.Clear();
        Resources.UnloadUnusedAssets();

        EditorLog.Log("[ResourceManager] 모든 리소스 캐시가 정리되었습니다.");
    }

    private void AlertNull(string path)
    {
        EditorLog.LogWarning($"[ResourceManager] {path} 에 해당하는 리소스가 존재하지 않습니다.");
    }

    #endregion
}
