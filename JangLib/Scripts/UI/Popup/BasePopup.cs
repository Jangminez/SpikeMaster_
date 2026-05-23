using UnityEngine;
using DG.Tweening;

public abstract class BasePopup : MonoBehaviour, IPopup
{
    [SerializeField] protected RectTransform _content;
    [SerializeField] protected BaseBtn _closeBtn;

    protected virtual void Awake()
    {
        if (_content == null)
            _content = GetComponent<RectTransform>();

        if (_closeBtn != null)
        {
            _closeBtn.OnClick += OnCloseBtnClicked;
        }
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);

        _content.DOKill();
        _content.localScale = Vector3.zero;
        _content.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

        SetEvents(true);
    }

    public virtual void Close()
    {
        SetEvents(false);

        _content.DOKill();
        _content.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    protected virtual void OnCloseBtnClicked()
    {
        PopupManager.Instance.HideLatest();
    }

    protected void CloseAll() => PopupManager.Instance.CloseAll();

    protected abstract void SetEvents(bool isOpen);

    public void SetAsLastSibling() => transform.SetAsLastSibling();
}
