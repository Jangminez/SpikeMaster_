using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using JangLib;

public class SpikeBall : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CircleCollider2D _col;
    public bool MoveDone { get; private set; }
    public event Action<Vector2> OnHit;

    public void Init(Vector3[] path, float duration)
    {
        _col = GetComponent<CircleCollider2D>();

        MoveDone = false;
        MoveSequence(path, duration);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOKill();

        Camera cam = eventData.pressEventCamera ?? Camera.main;
        if (cam == null) return;

        Vector3 screenPos = new Vector3(eventData.pressPosition.x, eventData.pressPosition.y, 10f);
        Vector2 hitWorldPos = cam.ScreenToWorldPoint(screenPos);

        Vector2 localHitPos = transform.InverseTransformPoint(hitWorldPos);

        float radius = 2f; //_col.radius;

        Vector2 normalizedOffset = new Vector2(
        Mathf.Clamp(localHitPos.x / radius, -1f, 1f),
        Mathf.Clamp(localHitPos.y / radius, -1f, 1f));

        HitSequence(normalizedOffset);
    }

    private void MoveSequence(Vector3[] path, float duration)
    {
        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(() => MoveDone = true);
    }

    private void HitSequence(Vector2 offset)
    {
        Sequence hitSeq = DOTween.Sequence();
        hitSeq.Append(transform.DOScale(transform.localScale * 1.1f, 0.05f).SetEase(Ease.OutBack))
              .AppendInterval(0.2f)
              .Append(transform.DOScale(transform.localScale, 0.05f))
              .OnComplete(() =>
              {
                  EditorLog.Log($"Hit Success: {offset}");
                  OnHit?.Invoke(offset);
              });
    }

    public void Destroy()
    {
        transform.DOKill();
        Destroy(gameObject);
    }
}
