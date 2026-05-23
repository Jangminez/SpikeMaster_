using UnityEngine;

public class SpikeBallSpawner : MonoBehaviour
{
    private CameraManager _cameraMgr;

    [Header("SpikeBall Spawn Settings")]
    [SerializeField] private GameObject _ballPrefab;

    private Vector3 _baseSpawnPos, _basePeakPos, _baseTargetPos;
    private SpikeConfigSO _config;

    public void Init(CameraManager cameraMgr, SpikeConfigSO config)
    {
        _cameraMgr = cameraMgr;
        _config = config;

        CalculateBallSpawnPos();
    }

    private void CalculateBallSpawnPos()
    {
        var spikeCam = _cameraMgr.SpikeCam;

        float verticalSize = spikeCam.Lens.OrthographicSize;
        float horizontalSize = verticalSize * Screen.width / Screen.height;
        Vector3 camPos = spikeCam.transform.position;

        float _minX = camPos.x - horizontalSize;
        float _maxX = camPos.x + horizontalSize;
        float _minY = camPos.y - verticalSize;
        float _maxY = camPos.y + verticalSize;

        _baseSpawnPos = new Vector2(_maxX + _config.SpawnOffset, (_minY + _maxY) * 0.5f);
        _basePeakPos = new Vector2((_minX + _maxX) * 0.5f, _maxY - _config.PeakOffset);
        _baseTargetPos = new Vector2(_minX - _config.SpawnOffset, (_minY + _maxY) * 0.5f);
    }

    public SpikeBall Spawn(QualityType quality)
    {
        (float range, float duration) = GetConfigByQuality(quality);

        float sOffset = Random.Range(-range, range);
        float tOffset = Random.Range(-range, range);

        Vector3 sPos = new Vector3(_baseSpawnPos.x, _baseSpawnPos.y + sOffset, 0);
        Vector3 tPos = new Vector3(_baseTargetPos.x, _baseTargetPos.y + tOffset, 0);
        Vector3 pPos = new Vector3(_basePeakPos.x, _basePeakPos.y + (sOffset + tOffset) * 0.5f, 0);

        GameObject obj = Instantiate(_ballPrefab, sPos, Quaternion.identity);

        if (obj.TryGetComponent(out SpikeBall ball))
        {
            Vector3[] path = new Vector3[] { sPos, pPos, tPos };
            ball.Init(path, duration);
        }

        return ball;
    }

    private (float range, float duration) GetConfigByQuality(QualityType quality)
    {
        return quality switch
        {
            QualityType.Perfect => (0.1f, 1.8f),
            QualityType.Good => (0.5f, 1.3f),
            QualityType.Bad => (1.5f, 0.8f),
            _ => (3.0f, 0.5f)
        };
    }
}
