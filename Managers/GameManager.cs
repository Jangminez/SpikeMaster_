using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using JangLib;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : SingletonWithMono<GameManager>
{
    private readonly List<IBaseManager> _preLoadManagerList = new();
    private readonly List<IBaseManager> _managerList = new();

    public bool IsPreLoaded { get; private set; }
    public bool IsInitialized { get; private set; }

    private bool isExit;

    protected override void Awake()
    {
        base.Awake();
        
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
#endif

        Application.targetFrameRate = 60;

        AddPreLoadManagers();
        AddManagers();
    }

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !isExit)
        {
            HandleBackKey();
        }
    }

    private void AddPreLoadManagers()
    {
        _preLoadManagerList.Add(EventManager.Instance);
        _preLoadManagerList.Add(AdMobManager.Instance);
        _preLoadManagerList.Add(PlayFabManager.Instance);
        _preLoadManagerList.Add(ResourceManager.Instance);
    }

    private void AddManagers()
    {
#if ENABLE_IAP
        _managerList.Add(IAPManager.Instance);
#endif
        _managerList.Add(NicknameValidator.Instance);
        _managerList.Add(SoundManager.Instance);
        _managerList.Add(PopupManager.Instance);
    }

    private IEnumerator Initialize()
    {
        yield return StartCoroutine(InitializeManagers());
        InitializeCompleted();
    }

    private IEnumerator InitializeManagers()
    {
        foreach (var manager in _preLoadManagerList)
        {
            manager.Init();
            yield return new WaitUntil(() => manager.IsInitialized);
        }

        PreLoadCompleted();
        yield return null;

        foreach (var manager in _managerList)
        {
            manager.Init();
            yield return new WaitUntil(() => manager.IsInitialized);
        }
    }

    private void PreLoadCompleted()
    {
        IsPreLoaded = true;
    }

    private void InitializeCompleted()
    {
        IsInitialized = true;
    }

    private void HandleBackKey()
    {
        if (!IsInitialized) return;

        isExit = true;
        var popup = PopupManager.Instance.Show<ExitPopup>("popup_exit");

        if (popup != null)
        {
            // 콜백 함수 전달
            popup.SetData(
                onConfirm: () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                },
                onCancel: () =>
                {
                    isExit = false;
                }
            );
        }
    }
}
