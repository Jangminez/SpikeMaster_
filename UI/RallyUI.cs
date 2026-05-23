using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RallyUI : MonoBehaviour
{
    [Header("Rally UI")]
    [SerializeField] private TextMeshProUGUI _startTMP;
    [SerializeField] private TextMeshProUGUI _scoreTMP;
    [SerializeField] private TextMeshProUGUI _comboTMP;

    [SerializeField] private List<GameObject> _hearts;

    private Transform _scoreTrans;
    private Transform _comboTrans;
    private Transform _startTrans;

    public void Init()
    {
        _scoreTrans = _scoreTMP.transform;
        _comboTrans = _comboTMP.transform;
        _startTrans = _startTMP.transform;

        EventManager.Instance.Register<Action<int>>((int)EventType.OnPointScore, SetScoreUI);
        EventManager.Instance.Register<Action<int>>((int)EventType.OnComboChanged, SetComboUI);
        EventManager.Instance.Register<Action>((int)EventType.OnRallyStart, HidePressUI);
        EventManager.Instance.Register<Action>((int)EventType.OnRallyReady, ShowPressUI);
        EventManager.Instance.Register<Action<int>>((int)EventType.OnHeartChanged, SetHeartUI);

        _comboTMP.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventManager.Instance.UnRegister<Action<int>>((int)EventType.OnPointScore, SetScoreUI);
        EventManager.Instance.UnRegister<Action<int>>((int)EventType.OnComboChanged, SetComboUI);
        EventManager.Instance.UnRegister<Action>((int)EventType.OnRallyStart, HidePressUI);
        EventManager.Instance.UnRegister<Action>((int)EventType.OnRallyReady, ShowPressUI);
        EventManager.Instance.UnRegister<Action<int>>((int)EventType.OnHeartChanged, SetHeartUI);

        _startTrans.DOKill();
        _scoreTrans.DOKill();
        _comboTrans.DOKill();
    }

    private void ShowPressUI()
    {
        _startTMP.gameObject.SetActive(true);
        _startTrans.DOKill();
        _startTrans.localScale = Vector3.zero;

        _startTrans.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        _startTMP.DOFade(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    private void HidePressUI()
    {
        _startTrans.DOKill();
        _startTrans.DOScale(0f, 0.2f).OnComplete(() => _startTMP.gameObject.SetActive(false));
    }

    private void SetScoreUI(int score)
    {
        _scoreTMP.SetText("{0}", score);

        _scoreTrans.DOKill();
        _scoreTrans.localScale = Vector3.one;
        _scoreTrans.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.2f, 5, 1);
    }

    private void SetComboUI(int combo)
    {
        if (combo <= 0)
        {
            _comboTMP.gameObject.SetActive(false);
            return;
        }

        _comboTMP.gameObject.SetActive(true);
        _comboTMP.SetText($"COMBO: {combo}");

        _comboTrans.DOKill();
        _comboTrans.localScale = Vector3.one;

        float punchPower = Mathf.Min(0.1f + (combo * 0.02f), 0.4f);
        _comboTrans.DOPunchScale(Vector3.one * punchPower, 0.25f, 10, 1);

        if(combo < 6) _comboTMP.color = Color.white;
        else if(combo < 12) _comboTMP.color = Color.yellow;
        else _comboTMP.color = Color.red;
    }

    private void SetHeartUI(int curHeart)
    {
        for (int i = 0; i < _hearts.Count; i++)
        {
            GameObject heart = _hearts[i];
            Transform hTrans = heart.transform;
            bool shouldActive = i < curHeart;

            if (!shouldActive && heart.activeSelf)
            {
                hTrans.DOKill();
                hTrans.DOScale(0f, 0.3f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() => heart.SetActive(false));
            }
            else if (shouldActive && !heart.activeSelf)
            {
                heart.SetActive(true);
                hTrans.DOKill();
                hTrans.localScale = Vector3.zero;
                hTrans.DOScale(1f, 0.4f).SetEase(Ease.OutElastic);
            }
        }
    }
}
