using System;
using UnityEngine;

public interface IBall
{
    void SetSprite(Sprite sprite);
    void MoveToTarget(Vector2 target, float speed, bool isToss, Action onPeak, Action onComplete = null);
    Vector2 GetPosition();
    void ResetPos(Vector2 pos);

    void SpikeToTarget(Vector2 target, float speed, Action onLanded);
    void ThrowUpwards(Action onDone);
    void OutRandom(Action onDone);
    void OutReflect(Vector2 dir, float speed, Action onCompleted = null);
    void PlayEffect();
    Transform transform { get; }
}
