using System;
using UnityEngine;

public class ExitPopup : BasePopup
{
    [SerializeField] private BaseBtn _exitBtn;

    private Action _onConfirm;
    private Action _onCancel;

    protected override void SetEvents(bool isOpen)
    {
        if(isOpen)
        {
            _exitBtn.OnClick += OnClickExitBtn;
        }
        else
        {
            _exitBtn.OnClick -= OnClickExitBtn;
        }
    }
    public void SetData(Action onConfirm, Action onCancel)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }

    private void OnClickExitBtn()
    {
        _onConfirm?.Invoke();
    }

    protected override void OnCloseBtnClicked()
    {
        _onCancel?.Invoke();
        base.OnCloseBtnClicked();
    }
}
