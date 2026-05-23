using UnityEngine;
using UnityEngine.UI;

public class SettingPlayPopup : BasePopup
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Buttons")]
    [SerializeField] private BasicBtn _homeBtn;
    [SerializeField] private BasicBtn _rePlayBtn;

    public override void Open()
    {
        base.Open();

        var volume = SoundManager.Instance.GetVolumes();
        _bgmSlider.value = volume.bgm;
        _sfxSlider.value = volume.sfx;
    }

    public override void Close()
    {
        SoundManager.Instance.SaveVolume();
        base.Close();
    }
    
    protected override void SetEvents(bool isOpen)
    {
        if (isOpen)
        {
            _homeBtn.OnClick += OnClickHomeBtn;
            _rePlayBtn.OnClick += OnClickReplayBtn;

            _bgmSlider.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.BGM, value));
            _sfxSlider.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.SFX, value));
        }
        else
        {
            _homeBtn.OnClick -= OnClickHomeBtn;
            _rePlayBtn.OnClick -= OnClickReplayBtn;

            _bgmSlider.onValueChanged.RemoveAllListeners();
            _sfxSlider.onValueChanged.RemoveAllListeners();
        }
    }

    private void OnClickHomeBtn()
    {
        CloseAll();
        LoadSceneManager.LoadSceneAsync(SceneType.MainScene);
    }

    private void OnClickReplayBtn()
    {
        CloseAll();
        LoadSceneManager.LoadSceneAsync(SceneType.PlayScene);
    }
}
