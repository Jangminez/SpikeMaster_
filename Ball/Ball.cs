using System;
using UnityEngine;

public class Ball : MonoBehaviour, IBall
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer _ballSprite;
    [SerializeField] private Transform _ballTr;
    [SerializeField] private Transform _shadowTr;

    [Header("Ball Config")]
    [SerializeField] private BallConfigSO _config;

    [Header("VFX & SFX")]
    [SerializeField] private GameObject _effectPrefab;

    private BallTweenHandler _tweenHandler;
    
    public void SetSprite(Sprite sprite) => _ballSprite.sprite = sprite;

    private void Awake()
    {
        _tweenHandler = new BallTweenHandler(transform, _ballTr, _shadowTr, _config);
    }

    public void ThrowUpwards(Action onComplete = null)
    {
        _tweenHandler.PlayUpward(0.5f, onComplete);
    }

    public void MoveToTarget(Vector2 targetPos, float ballSpeed, bool isToss, Action onPeak = null, Action onComplete = null)
    {
        if(isToss) SoundManager.Instance.PlaySFX(SoundKey.SFX_TOSS);
        else SoundManager.Instance.PlaySFX(SoundKey.SFX_BOUNCE);

        float duration = Vector2.Distance(transform.position, targetPos) / ballSpeed;
        _tweenHandler.PlayNormal(targetPos, duration, onPeak, onComplete);
    }

    public void SpikeToTarget(Vector2 targetPos, float ballSpeed, Action onComplete = null)
    {
        SoundManager.Instance.PlaySFX(SoundKey.SFX_SPIKE);

        float duration = Vector2.Distance(transform.position, targetPos) / ballSpeed;
        _tweenHandler.PlaySpike(targetPos, duration, onComplete);
    }

    public void OutReflect(Vector2 hitDir, float ballSpeed, Action onComplete = null)
    {
        SoundManager.Instance.PlaySFX(SoundKey.SFX_BOUNCE);

        Vector2 exitPos = (Vector2)transform.position + (hitDir.normalized * _config.OutDistance);
        float duration = _config.OutDistance / ballSpeed;
        _tweenHandler.PlayExit(exitPos, duration, onComplete);
    }

    public void OutRandom(Action onComplete = null)
    {
        SoundManager.Instance.PlaySFX(SoundKey.SFX_BOUNCE);

        float randomAngle = UnityEngine.Random.Range(0, 360f);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
        float speed = UnityEngine.Random.Range(_config.MinRandomSpeed, _config.MaxRandomSpeed);

        Vector2 exitPos = (Vector2)transform.position + (direction * _config.OutDistance);
        float duration = _config.OutDistance / speed;

        _tweenHandler.PlayExit(exitPos, duration, onComplete);
    }

    public void ResetPos(Vector2 pos)
    {
        _tweenHandler.KillAll();
        _ballTr.localScale = Vector3.one * _config.BaseScale;
        transform.position = pos;
    }

    public Vector2 GetPosition() => transform.position;

    public void PlayEffect()
    {
        ParticleSystem effect = Instantiate(_effectPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        effect.Play();
    }
}
