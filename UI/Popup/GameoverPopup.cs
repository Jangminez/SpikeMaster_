using TMPro;
using UnityEngine;

public class GameOverPopup : BasePopup
{
    [Header("Buttons")]
    [SerializeField] private BasicBtn _restartBtn;
    [SerializeField] private BasicBtn _homeBtn;
    [SerializeField] private BasicBtn _bonusBtn;


    [Header("Score Texts")]
    [SerializeField] private TextMeshProUGUI _scoreTMP;
    [SerializeField] private TextMeshProUGUI _bestTMP;
    [SerializeField] private TextMeshProUGUI _bestShadowTMP;


    [Header("Effects")]
    [SerializeField] private GameObject _bestEffect;

    public void SetUI(int score, int bestScore, bool isRewarded)
    {
        _scoreTMP.text = score.ToString();
        _bestTMP.text = bestScore.ToString();
        _bestShadowTMP.text = bestScore.ToString();

        _bestEffect.SetActive(score == bestScore);

        if(isRewarded)
        {
            _bonusBtn.SetActive(false);
        }
    }

    protected override void SetEvents(bool isOpen)
    {
        if (isOpen)
        {
            _restartBtn.OnClick += OnClickRestartBtn;
            _homeBtn.OnClick += OnClickHomeBtn;
            _bonusBtn.OnClick += OnClickBonusBtn;
        }
        else
        {
            _restartBtn.OnClick -= OnClickRestartBtn;
            _homeBtn.OnClick -= OnClickHomeBtn;
            _bonusBtn.OnClick -= OnClickBonusBtn;
        }
    }

    private void OnClickHomeBtn()
    {
        CloseAll();
        LoadSceneManager.LoadSceneAsync(SceneType.MainScene);
    }

    private void OnClickRestartBtn()
    {
        CloseAll();
        LoadSceneManager.LoadSceneAsync(SceneType.PlayScene);
    }

    private void OnClickBonusBtn()
    {
        AdMobManager.Instance.ShowRewardedAd(() =>
        {
            PopupManager.Instance.CloseAll();
            PlaySceneManager.Instance.GetExtraHeart();
        });
    }
}
