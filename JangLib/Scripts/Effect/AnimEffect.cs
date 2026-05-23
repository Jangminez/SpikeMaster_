using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimEffect : MonoBehaviour, IEffect
{
    private Animator _animator;
    private Coroutine _effectCO;
    private static readonly int PlayHash = Animator.StringToHash("Play");

    public bool IsPlaying { get; private set; }

    private void Awake() => _animator = GetComponent<Animator>();

    public void Play(Action onComplete = null)
    {
        Stop();
        _effectCO = StartCoroutine(PlayRoutine(onComplete));
    }

    private IEnumerator PlayRoutine(Action onComplete)
    {
        IsPlaying = true;
        _animator.Play(PlayHash, 0, 0f);

        yield return null;

        float duration = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);

        IsPlaying = false;
        onComplete?.Invoke();
        gameObject.SetActive(false);
    }

    public void Stop()
    {
        if (_effectCO != null) StopCoroutine(_effectCO);
        IsPlaying = false;
    }
}
