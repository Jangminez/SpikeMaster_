using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingMainPopup : BasePopup
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("User UI")]
    [SerializeField] private BaseBtn _googleLinkBtn;
    [SerializeField] private BaseBtn _deleteAccountBtn;
    [SerializeField] private TextMeshProUGUI _linkBtnTMP;

    public override void Open()
    {
        base.Open();

        var volume = SoundManager.Instance.GetVolumes();
        _bgmSlider.value = volume.bgm;
        _sfxSlider.value = volume.sfx;

        UpdateLinkButtonUI();
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
            _bgmSlider.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.BGM, value));
            _sfxSlider.onValueChanged.AddListener((value) => SoundManager.Instance.SetVolume(SoundType.SFX, value));

            _googleLinkBtn.OnClick += OnClickGoogleLink;
            _deleteAccountBtn.OnClick += OnClickDeleteAccount;
        }
        else
        {
            _bgmSlider.onValueChanged.RemoveAllListeners();
            _sfxSlider.onValueChanged.RemoveAllListeners();

            _googleLinkBtn.OnClick -= OnClickGoogleLink;
            _deleteAccountBtn.OnClick -= OnClickDeleteAccount;
        }
    }

    private void UpdateLinkButtonUI()
    {
        string lastMethod = PlayerPrefs.GetString("LastLoginMethod", "None");

        if (lastMethod == LoginMethod.Google.ToString())
        {
            _linkBtnTMP.text = "Linked";
            _googleLinkBtn.SetInteractable(false);
        }
        else
        {
            _linkBtnTMP.text = "Link to Google";
            _googleLinkBtn.SetInteractable(true);
        }
    }

    private void OnClickGoogleLink()
    {
        var popup = PopupManager.Instance.Show<AlertPopup>("popup_alert");
        popup.SetAlert("Linking to Google...");

        PlayFabManager.Instance.LinkGoogleAccount(() =>
        {
            if (popup != null)
            {
                var popup = PopupManager.Instance.Show<AlertPopup>("popup_alert");
                popup.SetAlert("Successfully Linked!");

                UpdateLinkButtonUI();
            }
        });
    }

    private void OnClickDeleteAccount()
    {
        var popup = PopupManager.Instance.Show<ConfirmPopup>("popup_confirm");
        popup.SetMessageWithAction("Are you sure you want to delete your account?", () =>
        {
            PlayFabManager.Instance.DeleteAccount();
        });
    }
}
