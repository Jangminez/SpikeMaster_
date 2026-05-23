using UnityEngine;

public interface IRallyInputHandler
{
    public QualityType CheckQuality(Vector2 playerPos, Vector2 ballPos);
}

public class RallyInputHandler : IRallyInputHandler
{
    private readonly RallyConfigSO _config;

    public RallyInputHandler(RallyConfigSO config)
    {
        _config = config;
    }

    /// <summary>
    /// 거리 기반 퀄리티 판정
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="ballPos"></param>
    /// <returns></returns>
    public QualityType CheckQuality(Vector2 playerPos, Vector2 ballPos)
    {
        float distance = Vector2.Distance(playerPos, ballPos);

        if (distance <= _config.PerfectRange) return QualityType.Perfect;
        if (distance <= _config.GoodRange) return QualityType.Good;
        if (distance <= _config.BadRange) return QualityType.Bad;
        else return QualityType.Miss;
    }
}
