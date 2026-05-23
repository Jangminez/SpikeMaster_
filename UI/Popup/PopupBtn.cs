using UnityEngine;

public class PopupBtn : BaseBtn
{
    [SerializeField] private string _popupKey;

    protected override void OnButtonClick()
    {
        PopupManager.Instance.Show<IPopup>(_popupKey);
    }
}
