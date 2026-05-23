using System;
using GoogleMobileAds.Api;
using JangLib;

public class AdMobManager : SingletonWithMono<AdMobManager>, IBaseManager
{
    public bool IsInitialized { get; private set; }
    public bool IsNoAds { get; private set; } = false;

    // 테스트 ID
    // private readonly string _interstitialId = "ca-app-pub-3940256099942544/1033173712";
    // private readonly string _rewardedId = "ca-app-pub-3940256099942544/5224354917";

    // 실제 ID
    private readonly string _interstitialId = "ca-app-pub-4315493457302716/8467505385";
    private readonly string _rewardedId = "ca-app-pub-4315493457302716/5348199636";


    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;

    private float _interstitialCooldownMinutes = 5f;
    private DateTime _lastInterstitialTime = DateTime.MinValue;

    public void Init()
    {
        EventManager.Instance.Register<Action>((int)EventType.OnSetNoAds, SetNoAds);

        MobileAds.Initialize(status =>
        {
            EditorLog.Log("[AdMob] SDK Initialized.");

            LoadInterstitialAd();
            LoadRewardedAd();
        });

        IsInitialized = true;
    }

    private void SetNoAds()
    {
        IsNoAds = true;
        EditorLog.Log("[AdMob] 광고 제거 설정 완료!");
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.UnRegister<Action>((int)EventType.OnSetNoAds, SetNoAds);

        if (_interstitialAd != null) _interstitialAd.Destroy();
        if (_rewardedAd != null) _rewardedAd.Destroy();

        base.OnDestroy();
    }

    #region << INTERSTITIAL (전면 광고) >>

    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var adRequest = new AdRequest();
        InterstitialAd.Load(_interstitialId, adRequest, (ad, error) =>
        {
            if (error != null)
            {
                EditorLog.LogError($"[AdMob] Interstitial failed to load: {error}");
                return;
            }
            _interstitialAd = ad;
            EditorLog.Log("[AdMob] Interstitial loaded.");
        });
    }

    public void ShowInterstitialAd()
    {
        if (!IsInitialized) return;
        if (IsNoAds) return;

        TimeSpan elapsed = DateTime.Now - _lastInterstitialTime;
        if (elapsed.TotalMinutes < _interstitialCooldownMinutes)
        {
            double remaining = _interstitialCooldownMinutes - elapsed.TotalMinutes;
            EditorLog.Log($"[AdMob] Interstitial is on cooldown. Remaining: {remaining:F1} min");
            return;
        }

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _lastInterstitialTime = DateTime.Now;
            _interstitialAd.Show();
            _interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitialAd();
        }
        else
        {
            EditorLog.LogWarning("[AdMob] Interstitial ad not ready.");
            LoadInterstitialAd();
        }
    }

    #endregion

    #region << REWARDED (보상형 광고) >>

    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();
        RewardedAd.Load(_rewardedId, adRequest, (ad, error) =>
        {
            if (error != null)
            {
                EditorLog.LogError($"[AdMob] Rewarded ad failed to load: {error}");
                return;
            }
            _rewardedAd = ad;
            EditorLog.Log("[AdMob] Rewarded ad loaded.");
        });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (IsNoAds)
        {
            onRewardEarned?.Invoke();
            return;
        }

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                EditorLog.Log($"[AdMob] Reward earned: {reward.Amount} {reward.Type}");
                onRewardEarned?.Invoke();
            });

            _rewardedAd.OnAdFullScreenContentClosed += () => LoadRewardedAd();
        }
        else
        {
            EditorLog.LogWarning("[AdMob] Rewarded ad not ready.");
            LoadRewardedAd();
        }
    }
    #endregion
}