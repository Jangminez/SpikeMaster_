using DG.Tweening;
using UnityEngine;

public class ScaleEffect : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float _targetScale = 1.2f; 
    [SerializeField] private float _duration = 1.0f;  
    [SerializeField] private Ease _easeType = Ease.InOutSine;

    private Vector3 _initialScale;
    private Tween _scaleTween;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        _scaleTween?.Kill();

        transform.localScale = _initialScale;


        _scaleTween = transform.DOScale(_initialScale * _targetScale, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        _scaleTween?.Kill();
    }
}
