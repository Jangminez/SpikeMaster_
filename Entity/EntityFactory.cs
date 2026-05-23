using UnityEngine;
using JangLib;

public static class EntityPath
{
    // --------- Player
    public static readonly string Player_OH = "Prefabs/Player/Player_Spiker";
    public static readonly string Player_Li = "Prefabs/Player/Player_Libero";
    public static readonly string Player_Se = "Prefabs/Player/Player_Setter";

    // --------- Enemy
    public static readonly string Enemy_Server = "Prefabs/Enemy/Enemy_Server";
    public static readonly string Enemy_Libero = "Prefabs/Enemy/Enemy_Libero";

    // --------- Ball
    public static readonly string Rally_Ball = "Prefabs/Ball/Rally_Ball";
    public static readonly string Spike_Ball = "Prefabs/Ball/Spike_Ball";
}

public static class EntityFactory
{
    public static T CreateEntity<T>(string path) where T : class
    {
        GameObject obj = ResourceManager.Instance.Instantiate(path);

        if (obj == null) return null;

        if (obj.TryGetComponent(out T entity))
        {
            return entity;
        }
        else
        {
            EditorLog.LogWarning($"[EntityFactory] {path} 오브젝트에 {typeof(T).Name} 컴포넌트가 존재하지 않습니다.");
            UnityEngine.Object.Destroy(obj);
            return null;
        }
    }

    public static T CreateEntity<T>(string path, Vector3 pos, Quaternion rot) where T : class
    {
        GameObject obj = ResourceManager.Instance.Instantiate(path);
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        
        if (obj == null) return null;

        if (obj.TryGetComponent(out T entity))
        {
            return entity;
        }
        else
        {
            EditorLog.LogWarning($"[EntityFactory] {path} 오브젝트에 {typeof(T).Name} 컴포넌트가 존재하지 않습니다.");
            UnityEngine.Object.Destroy(obj);
            return null;
        }
    }
}
