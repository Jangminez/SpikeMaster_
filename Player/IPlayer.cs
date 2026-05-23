using UnityEngine;

public interface IPlayer
{
    PlayerType Type { get; }
    void Init();
    void ExecuteAction(QualityType type);
    void PlayAnim(PlayerAnimType type);
    void ResetPlayer();
    void SetActive(bool isActive);
    void SetTransform(Vector3 pos, Quaternion rot);
    Vector2 GetPosition();
}
