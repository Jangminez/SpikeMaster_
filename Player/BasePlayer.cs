using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class BasePlayer : MonoBehaviour, IPlayer
{
    protected bool isInitialized = false;

    private PlayerAnimHandler _animHandler;
    public PlayerType Type { get; protected set; }

    [Header("Init Pos & Rot")]
    [SerializeField] private Vector3 _spawnPos;
    [SerializeField] private Quaternion _spawnRot;

    [Header("Point Transform")]
    [SerializeField] private Transform _pointTr;

    public void Init()
    {
        _animHandler = new PlayerAnimHandler(GetComponent<Animator>());
        SetTransform(_spawnPos, _spawnRot);

        OnInit();
        isInitialized = true;
    }

    public virtual void ResetPlayer()
    {
        PlayAnim(PlayerAnimType.Idle);
    }

    protected abstract void OnInit();

    public virtual void ExecuteAction(QualityType quality)
    {
        // Show Text Effect
        TextEffectHelper.ShowQualityAtWorldPos(transform, 1f, quality);
    }

    public void PlayAnim(PlayerAnimType type) => _animHandler?.PlayAnim(type);
    public void SetActive(bool isActive) => gameObject.SetActive(isActive);
    public Vector2 GetPosition() => _pointTr.position;

    public void SetTransform(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}
