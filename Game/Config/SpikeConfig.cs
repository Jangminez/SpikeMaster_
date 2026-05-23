using UnityEngine;

[CreateAssetMenu(fileName = "SpikeConfigSO", menuName = "GameConfig/SpikeConfig")]
public class SpikeConfigSO : ScriptableObject
{
    [Header("Spike Settings")]
    [SerializeField] private float _curveIntensityX = 5f;
    [SerializeField] private float _distanceIntensityY = 3f;

    [Header("SpikeBall Spawn Settings")]
    [SerializeField] private float _spawnOffset = 2f;
    [SerializeField] private float _peakOffset = 2.5f;

    public float CurveIntensityX => _curveIntensityX;
    public float DistanceIntensityY => _distanceIntensityY;

    public float SpawnOffset => _spawnOffset;
    public float PeakOffset => _peakOffset;
}

