using System.Collections;
using TMPro;
using UnityEngine;

public class AlertPopup : BasePopup
{
    [SerializeField] private TextMeshProUGUI _msgTMP;

    public void SetAlert(string msg)
    {
        _msgTMP.text = msg;
        StartCoroutine(ClosePopupCO());
    }
    
    private IEnumerator ClosePopupCO()
    {
        yield return new WaitForSecondsRealtime(1f);

        PopupManager.Instance.HideLatest();
    }
    
    protected override void SetEvents(bool isOpen)
    {
    }
}
