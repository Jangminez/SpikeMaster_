using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ErrorPopup : BasePopup
{
    [SerializeField] private BaseBtn _confirmBtn;
    [SerializeField] private TextMeshProUGUI _errorTMP;

    private Action _onConfirm;

    protected override void SetEvents(bool isOpen)
    {
        if (isOpen)
        {
            _confirmBtn.OnClick += OnClickConfirmBtn;
        }
        else
        {
            _confirmBtn.OnClick -= OnClickConfirmBtn;
        }
    }

    private void OnClickConfirmBtn()
    {
        _onConfirm?.Invoke();
    }

    public void SetMessageWithAction(string msg, Action onConfirm)
    {
        _errorTMP.text = msg;
        _onConfirm = onConfirm;
    }

    public void SetMessage(string msg)
    {
        _errorTMP.text = msg;
        _confirmBtn.SetActive(false);
        
        StartCoroutine(ClosePopupCO());
    }

    private IEnumerator ClosePopupCO()
    {
        yield return new WaitForSecondsRealtime(1f);

        PopupManager.Instance.HideLatest();
    }
}
