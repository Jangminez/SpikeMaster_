using System.Collections;
using JangLib;
using UnityEngine;

public class MainSceneManager : SingletonWithScene<MainSceneManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        StartCoroutine(InitManagers());
    }

    private IEnumerator InitManagers()
    {
        yield return new WaitUntil(() => GameManager.Instance.IsInitialized);

        SoundManager.Instance.PlayBGM(SoundKey.BGM_MAIN);

        if(!PlayFabManager.Instance.IsLogin)
        {
            PlayFabManager.Instance.TryAutoLogin();
        }
    }
    
    public void Play() => LoadSceneManager.LoadSceneAsync(SceneType.PlayScene);
}
