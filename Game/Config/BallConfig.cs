using UnityEngine;

[CreateAssetMenu(fileName = "BallConfigSO", menuName = "GameConfig/BallConfig")]
public class BallConfigSO : ScriptableObject
{
    [Header("Scale Settings")]
    [SerializeField] private float _baseScale = 2.5f;
    [SerializeField] private float _jumpScaleMultiplier = 2f;

    [Header("Shadow Settings")]
    [SerializeField] private Vector2 _shadowInitOffset = new Vector2(0.1f, -0.1f);
    [SerializeField] private float _shadowHeightMultiplier = 3.5f;

    [Header("Out Game Settings")]
    [SerializeField] private float _outDistance = 20f;
    [SerializeField] private float _minRandomSpeed = 5f;
    [SerializeField] private float _maxRandomSpeed = 20f;

    public float BaseScale => _baseScale;
    public float JumpScaleMultiplier => _jumpScaleMultiplier;
    public Vector2 ShadowInitOffset => _shadowInitOffset;
    public float ShadowHeightMultiplier => _shadowHeightMultiplier;
    public float OutDistance => _outDistance;
    public float MinRandomSpeed => _minRandomSpeed;
    public float MaxRandomSpeed => _maxRandomSpeed;
}