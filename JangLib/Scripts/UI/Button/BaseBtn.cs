using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class BaseBtn : MonoBehaviour
{
    protected Button _myBtn;
    public Action OnClick; // 외부에서 이벤트 등록 시 사용

    protected virtual void Awake()
    {
        _myBtn = GetComponent<Button>();
        _myBtn.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundKey.SFX_CLICK);
        }
        
        OnClick?.Invoke();
        OnButtonClick();
    }

    /// <summary>
    /// 이 버튼이 눌렸을 때 실행될 고유 로직을 여기에 구현합니다.
    /// </summary>
    protected abstract void OnButtonClick();

    public void SetInteractable(bool isInteractable) => _myBtn.interactable = isInteractable;
    public void SetActive(bool isActive) => gameObject.SetActive(isActive);
}
