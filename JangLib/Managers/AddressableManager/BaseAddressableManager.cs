using JangLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class BaseAddressableManager<U> : SingletonWithMono<U>, IBaseManager where U : BaseAddressableManager<U>, new()
{
    protected GameResource<GameObject> prefabs = new GameResource<GameObject>();
    protected GameResource<Sprite> sprites = new GameResource<Sprite>();

    public bool IsInitialized { get; private set; } = false;

    [Header("Remote Download Label")]
    [SerializeField] private AssetLabelReference _label;

    #region <<  INIT  >>
        
    public async void Init()
    {
        await InitAssets();

        IsInitialized = true;
    }

    protected virtual async Awaitable InitAssets()
    {
        await prefabs.Init();
        await sprites.Init();
    }

    #endregion

    #region <<  Get Resources  >>

        public GameObject GetPrefab(string fileName) => prefabs.GetResource(fileName);

        public Sprite GetSprite(string fileName) => sprites.GetResource(fileName);

    #endregion
}
