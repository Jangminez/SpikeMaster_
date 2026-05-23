using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalInputPanel : MonoBehaviour, IPointerDownHandler
{
    private Image _img;

    private void Start()
    {
        _img = GetComponent<Image>();
        EventManager.Instance.Register<Action<bool>>((int)EventType.OnSpike, SetRaycast);
    }

    private void OnDestroy()
    {
        EventManager.Instance.UnRegister<Action<bool>>((int)EventType.OnSpike, SetRaycast);
    }
    
    private void SetRaycast(bool doSpike)
    {
        _img.enabled = !doSpike;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        EventManager.Instance.Trigger((int)EventType.GlobalTouch);
    }
}