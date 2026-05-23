using System;
using DG.Tweening;
using UnityEngine;

public class BallTweenHandler
{
    private readonly Transform _rootTr;
    private readonly Transform _ballTr;
    private readonly Transform _shadowTr;
    private readonly BallConfigSO _config;

    private Sequence _moveSeq;
    private Sequence _effectSeq;

    public BallTweenHandler(Transform root, Transform ball, Transform shadow, BallConfigSO config)
    {
        _rootTr = root;
        _ballTr = ball;
        _shadowTr = shadow;
        _config = config;
    }

    public void KillAll()
    {
        _moveSeq?.Kill();
        _effectSeq?.Kill();
    }

    public void PlayNormal(Vector2 targetPos, float duration, Action onPeak, Action onComplete)
    {
        KillAll();
        
        float halfDuration = duration * 0.5f;

        _moveSeq = DOTween.Sequence()
            .Append(_rootTr.DOMove(targetPos, duration).SetEase(Ease.Linear))
            .AppendInterval(0.05f)
            .OnComplete(() => onComplete?.Invoke());

        _effectSeq = DOTween.Sequence()
            .Append(_ballTr.DOScale(_config.BaseScale * _config.JumpScaleMultiplier, halfDuration).SetEase(Ease.OutQuad))
            .Join(_shadowTr.DOLocalMove(_config.ShadowInitOffset * _config.ShadowHeightMultiplier, halfDuration).SetEase(Ease.OutQuad))
            .AppendCallback(() => onPeak?.Invoke())

            .Append(_ballTr.DOScale(_config.BaseScale, halfDuration).SetEase(Ease.InQuad))
            .Join(_shadowTr.DOLocalMove(_config.ShadowInitOffset, halfDuration).SetEase(Ease.InQuad));
    }

    public void PlaySpike(Vector2 targetPos, float duration, Action onComplete)
    {
        KillAll();
      
        _ballTr.localScale = Vector3.one * (_config.BaseScale * 1.5f);

        _moveSeq = DOTween.Sequence()
            .Append(_rootTr.DOMove(targetPos, duration).SetEase(Ease.InSine))
            .OnComplete(() => onComplete?.Invoke());

        _effectSeq = DOTween.Sequence()
            .Append(_ballTr.DOScale(_config.BaseScale, duration).SetEase(Ease.InCubic))
            .Join(_shadowTr.DOLocalMove(_config.ShadowInitOffset, duration).SetEase(Ease.InCubic));
    }

    public void PlayExit(Vector2 exitPos, float duration, Action onComplete)
    {
        KillAll();

        _moveSeq = DOTween.Sequence()
            .Append(_rootTr.DOMove(exitPos, duration).SetEase(Ease.OutQuad))
            .OnComplete(() => onComplete?.Invoke());

        _effectSeq = DOTween.Sequence()
            .Append(_ballTr.DOScale(_config.BaseScale * 3f, duration))
            .OnComplete(() => _ballTr.localScale = Vector3.one * _config.BaseScale);
    }

    public void PlayUpward(float duration, Action onComplete = null)
    {
        KillAll();

        _ballTr.localScale = Vector3.one * _config.BaseScale;

        _moveSeq = DOTween.Sequence()
            .Append(_ballTr.DOScale(_config.BaseScale * 1.5f, duration))
            .OnComplete(() => onComplete?.Invoke());
    }
}