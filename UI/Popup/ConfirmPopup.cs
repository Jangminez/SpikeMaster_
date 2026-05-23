using System;
using TMPro;
using UnityEngine;

public class ConfirmPopup : BasePopup
{
    [SerializeField] private BaseBtn _confirmBtn;
    [SerializeField] private TextMeshProUGUI _msgTMP;

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
        _msgTMP.text = msg;
        _onConfirm = onConfirm;
    }
}
