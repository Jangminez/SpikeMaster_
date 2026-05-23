using System.Collections.Generic;
using System.Threading.Tasks;
using JangLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameResource<T> where T : Object
{
    protected Dictionary<string, T> resources = new Dictionary<string, T>();
    protected bool isLoaded = false;

    #region <<  INIT  >>

    public virtual async Awaitable Init(string label = "PreLoad")
    {
        if (isLoaded) return;

        isLoaded = true;
        T[] allResources = await LoadAllResourcesByLabel(label);
        foreach (T resource in allResources)
        {
            AddResource(resource.name, resource);
        }

        EditorLog.Log($"[GameResource] Init : label {label}");
    }
    #endregion

    #region <<  LOAD  >>
    private async Awaitable<T[]> LoadAllResourcesByLabel(string label = "PreLoad")
    {
        var operationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        var locations = await operationHandle.Task;

        List<T> resourceObjects = new List<T>();

        if (locations != null && locations.Count > 0)
        {
            List<Task<T>> loadTasks = new List<Task<T>>();

            foreach (var location in locations)
            {
                var resourceHandle = Addressables.LoadAssetAsync<T>(location.PrimaryKey);
                loadTasks.Add(resourceHandle.Task);
            }

            T[] resources = await Task.WhenAll(loadTasks);

            foreach (var resource in resources)
            {
                if (resource != null)
                {
                    resourceObjects.Add(resource);
                }
            }
        }

        Addressables.Release(operationHandle);
        return resourceObjects.ToArray();
    }

    private T LoadResource(string fileName)
    {
        T resourceObject = FindResource(fileName);

        if (resourceObject != null)
        {
            return resourceObject;
        }

        var resourceHandle = Addressables.LoadAssetAsync<T>(fileName);
        T resource = resourceHandle.WaitForCompletion();

        if (resource != null)
        {
            AddResource(fileName, resource);
        }
        else
        {
            EditorLog.LogWarning($"[GameResource] LoadResource Fail (null)");
        }

        return resource;
    }
    #endregion

    #region <<  GET  >>

    public T GetResource(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;

        return LoadResource(fileName);
    }

    #endregion
    #region <<  UTIL  >>
    protected void AddResource(string key, T resource)
    {
        if (ContainsKey(key))
        {
            return;
        }

        resources.Add(key, resource);
    }

    protected virtual T FindResource(string key)
    {
        if (resources.TryGetValue(key, out T resource))
        {
            return resource;
        }
        return default;
    }

    private bool ContainsKey(string key) => key != null && resources.ContainsKey(key);

    public Dictionary<string, T> GetAllResource() => resources;

    public int GetCount() => resources.Count;

    #endregion
}
