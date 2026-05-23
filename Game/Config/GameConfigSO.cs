using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig/Master")]
public class GameConfig : ScriptableObject
{
    [SerializeField] private RallyConfigSO _rallyConfig;
    [SerializeField] private SpikeConfigSO _spikeConfig;
    [SerializeField] private BallConfigSO _ballConfig;

    public RallyConfigSO RallyConfig => _rallyConfig;
    public SpikeConfigSO SpikeConfig => _spikeConfig;
    public BallConfigSO BallConfig => _ballConfig;
}
