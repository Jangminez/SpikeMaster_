using System;
using System.Collections;
using JangLib;
using UnityEngine;

public class SpikeManager : MonoBehaviour
{
    private CameraManager _cameraMgr;
    private SpikeBallSpawner _ballSpawner;

    [Header("Spike Setting")]
    [SerializeField] private Transform _spikeTarget;
    private SpikeConfigSO _config;

    public void Init(SpikeConfigSO spikeConfig, CameraManager cameraMgr)
    {
        _config = spikeConfig;
        _cameraMgr = cameraMgr;
        
        _ballSpawner = GetComponentInChildren<SpikeBallSpawner>();
        _spikeTarget = transform.Find("SpikeTarget");

        _ballSpawner?.Init(_cameraMgr, _config);
    }

    public void StartSpike(QualityType quality, Action<(Vector2 targetPos, bool isHit)> onComplete)
    {
        _cameraMgr.ChangeCamByType(CameraType.Spike, () => StartCoroutine(SpikeCO(quality, onComplete)));
    }

    private IEnumerator SpikeCO(QualityType quality, Action<(Vector2 offset, bool isHit)> onComplete)
    {
        yield return WaitForSecondsCache.Wait(0.5f);

        SpikeBall ball = _ballSpawner.Spawn(quality);

        bool isHit = false;
        Vector2 hitOffset = Vector2.zero;

        ball.OnHit += (offset) =>
        {
            hitOffset = offset;
            isHit = true;
        };

        yield return new WaitUntil(() => ball.MoveDone || isHit);

        Vector3 finalTargetPos = _spikeTarget.position;

        if (isHit)
        {
            finalTargetPos.x -= hitOffset.x * _config.CurveIntensityX;
            finalTargetPos.y -= hitOffset.y * _config.DistanceIntensityY;
        }
        
        if (ball != null) ball.Destroy();

        _cameraMgr.ChangeCamByType(CameraType.TopDown);
        onComplete?.Invoke((finalTargetPos, isHit));
    }
}
