using UnityEngine;

public class AddressableManager : BaseAddressableManager<AddressableManager>
{
    protected GameResource<SoundDataSO> sounds = new GameResource<SoundDataSO>();

    protected override async Awaitable InitAssets()
    {
        await base.InitAssets();
        await sounds.Init();
    }

    #region <<  Get Resources  >>
    public SoundDataSO GetSound(string fileName) => sounds.GetResource(fileName);
    #endregion
}
