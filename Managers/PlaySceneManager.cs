using System.Collections;
using JangLib;
using UnityEngine;

public class PlaySceneManager : SingletonWithScene<PlaySceneManager>
{
    [Header("Managers")]
    [SerializeField] private CameraManager _cameraMgr;
    [SerializeField] private RallyFlowManager _rallyMgr;
    [SerializeField] private SpikeManager _spikeMgr;

    [Header("Game Configs")]
    [SerializeField] private GameConfig _gameConfig;

    public CameraManager Cam => _cameraMgr;
    public RallyFlowManager RallyFlow => _rallyMgr;
    public SpikeManager Spike => _spikeMgr;

    private int _curHeart;
    private int _curScore;
    private int _bestScore;

    private bool _isRewarded = false;

    protected override void Awake()
    {
        base.Awake();

        _curHeart = 3;
        _curScore = 0;
    }

    private void Start()
    {
        StartCoroutine(InitManagers());
    }

    private IEnumerator InitManagers()
    {
        yield return new WaitUntil(() => GameManager.Instance.IsInitialized);
        yield return new WaitUntil(() => PlayFabManager.Instance.IsLogin);

        PlayFabManager.Instance.GetBestScore((best) => _bestScore = best);

        SoundManager.Instance.PlayBGM(SoundKey.BGM_PLAY);

        EventManager.Instance.Trigger((int)EventType.OnPointScore, _curScore);
        EventManager.Instance.Trigger((int)EventType.OnHeartChanged, _curHeart);

        _cameraMgr.Init();
        _rallyMgr.Init(_gameConfig.RallyConfig);
        _spikeMgr.Init(_gameConfig.SpikeConfig, _cameraMgr);
        
        _rallyMgr.StartRally();
    }

    public void AddScore(int score)
    {
        _curScore += score;
        EventManager.Instance.Trigger((int)EventType.OnPointScore, _curScore);
    }

    public void GetExtraHeart()
    {
        _isRewarded = true;

        _curHeart++;
        _rallyMgr.StartRally();
        EventManager.Instance.Trigger((int)EventType.OnHeartChanged, _curHeart);
    }

    public void Miss()
    {
        _curHeart--;
        EventManager.Instance.Trigger((int)EventType.OnHeartChanged, _curHeart);

        if (_curHeart == 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        _rallyMgr.StopRally();

        if (_curScore > _bestScore)
        {
            _bestScore = _curScore;
            PlayFabManager.Instance.SaveBestScore(_bestScore);
        }

        var popup = PopupManager.Instance.Show<GameOverPopup>("popup_gameover");
        popup.SetUI(_curScore, _bestScore, _isRewarded);

        AdMobManager.Instance.ShowInterstitialAd();
    }
}
