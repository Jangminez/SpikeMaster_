using DG.Tweening;
using TMPro;
using UnityEngine;

public class NicknamePopup : BasePopup
{
    private enum NicknameState { Idle, ClientValid, CheckingServer }
    private NicknameState _currentState = NicknameState.Idle;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private BaseBtn _submitBtn;
    [SerializeField] private TextMeshProUGUI _warningTMP;

    private string _lastCheckedName = string.Empty;

    protected override void Awake()
    {
        base.Awake();
        _warningTMP.gameObject.SetActive(false);
    }

    public override void Open()
    {
        base.Open();
        ResetPopup();
    }

    private void ResetPopup()
    {
        _inputField.text = string.Empty;
        _warningTMP.gameObject.SetActive(false);
        _submitBtn.SetInteractable(true);
        _lastCheckedName = string.Empty;

        _inputField.onValueChanged.AddListener(_ =>
        {
            _currentState = NicknameState.Idle;
            _warningTMP.gameObject.SetActive(false);
        });
    }

    protected override void SetEvents(bool isOpen)
    {
        if (isOpen) _submitBtn.OnClick += OnClickSubmit;
        else _submitBtn.OnClick -= OnClickSubmit;
    }

    private void OnClickSubmit()
    {
        string currentName = _inputField.text.Trim();

        if (_currentState == NicknameState.Idle)
        {
            var validation = NicknameValidator.Instance.Validate(currentName);
            if (!validation.isValid)
            {
                ShowMessage(validation.message, Color.red, true);
                return;
            }

            _currentState = NicknameState.ClientValid;
            _lastCheckedName = currentName;
            ShowMessage("No profanity detected! Press again to check availability.", Color.blue, false);
        }
        else if (_currentState == NicknameState.ClientValid && _lastCheckedName == currentName)
        {
            _currentState = NicknameState.CheckingServer;
            RequestSetDisplayName(currentName);
        }
    }

    private void RequestSetDisplayName(string name)
    {
        _submitBtn.SetInteractable(false);
        ShowMessage("Checking with server...", Color.gray, false);

        PlayFabManager.Instance.SetDisplayName(name, (success, errorMsg) =>
        {
            _submitBtn.SetInteractable(true);

            if (success)
            {
                // 최종 성공
                PopupManager.Instance.HideLatest();
                EventManager.Instance.Trigger((int)EventType.OnLoginSuccess, name, PlayFabManager.Instance.PlayFabId);
            }
            else
            {
                _currentState = NicknameState.Idle;
                _lastCheckedName = string.Empty;
                ShowMessage(errorMsg, Color.red, true);
            }
        });
    }

    /// <summary>
    /// 메시지 출력 및 연출 (isError가 true일 때만 흔들림 효과)
    /// </summary>
    private void ShowMessage(string message, Color color, bool isError)
    {
        _warningTMP.text = message;
        _warningTMP.color = color;
        _warningTMP.gameObject.SetActive(true);

        if (isError)
        {
            _warningTMP.transform.DOKill();
            _warningTMP.transform.localPosition = Vector3.zero;
            _warningTMP.transform.DOPunchPosition(Vector3.right * 10f, 0.3f);
        }
    }
}