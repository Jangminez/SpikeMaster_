using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleEffect : MonoBehaviour, IEffect
{
    private ParticleSystem _particle;
    private Coroutine _effectCO;

    public bool IsPlaying => _particle.isPlaying;

    private void Awake() => _particle = GetComponent<ParticleSystem>();

    public void Play(Action onComplete = null)
    {
        _particle.Play();
        
        if (_effectCO != null) StopCoroutine(_effectCO);
        _effectCO = StartCoroutine(WaitForComplete(onComplete));
    }

    private IEnumerator WaitForComplete(Action onComplete)
    {
        yield return new WaitUntil(() => !_particle.isPlaying);

        onComplete?.Invoke();
        gameObject.SetActive(false);
        
        Destroy(gameObject);
    }

    public void Stop()
    {
        _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (_effectCO != null) StopCoroutine(_effectCO);

        Destroy(gameObject);
    }
}