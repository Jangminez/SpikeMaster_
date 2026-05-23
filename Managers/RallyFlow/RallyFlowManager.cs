using System;
using System.Collections;
using JangLib;
using UnityEngine;

public class RallyFlowManager : MonoBehaviour
{
    // --------- Ball & Players
    private IBall _ball;

    private IRallyInputHandler _inputHandler;
    private PlayerHandler _playerHandler;
    private EnemySpawner _enemySpawner;
    private RallyConfigSO _config;
    private bool _isTouched;

    private Coroutine _rallyCO;

    private int _comboCnt = 0;
    private float _dynamicSpeedMultiplier = 1.0f;
    private Vector2 _enemyReceivePos = new Vector2(-0.8f, 0.8f);

    #region <<  INIT & EVENT METHOD >>
    public void Init(RallyConfigSO rallyConfig)
    {
        _inputHandler = new RallyInputHandler(rallyConfig);
        _playerHandler = new PlayerHandler();
        _enemySpawner = new EnemySpawner(rallyConfig, _playerHandler.GetPos(PlayerType.Spiker));
        _config = rallyConfig;

        _ball = EntityFactory.CreateEntity<IBall>(EntityPath.Rally_Ball);

        EventManager.Instance.Register<Action>((int)EventType.GlobalTouch, OnGlobalTouchReceived);
    }

    private void OnDestroy()
    {
        EventManager.Instance.UnRegister<Action>((int)EventType.GlobalTouch, OnGlobalTouchReceived);
    }
    #endregion

    #region <<  RALLY FLOW LOGIC  >>

    public void StartRally()
    {
        if (_rallyCO != null) StopCoroutine(_rallyCO);

        _comboCnt = 0;
        _rallyCO = StartCoroutine(RallyMainCO());
    }

    public void StopRally() => StopCoroutine(_rallyCO);

    private IEnumerator RallyMainCO()
    {
        while (true)
        {
            yield return StartCoroutine(StepReadyCO());

            bool isServe = false;
            _playerHandler.ExecuteAction(PlayerType.Server, QualityType.Perfect);
            _ball.ThrowUpwards(() => isServe = true);

            yield return new WaitUntil(() => isServe);

            // 리시브
            QualityType receiveQuality = QualityType.Miss;
            yield return StartCoroutine(StepReceiveCO(q => receiveQuality = q));

            _playerHandler.ExecuteAction(PlayerType.Libero, receiveQuality);

            if (receiveQuality == QualityType.Miss)
            {
                yield return StartCoroutine(HandleFailCO());
                continue;
            }

            // 토스
            QualityType tossQuality = QualityType.Miss;
            yield return StartCoroutine(StepTossCO(receiveQuality, q => tossQuality = q));

            _playerHandler.ExecuteAction(PlayerType.Setter, tossQuality);

            if (tossQuality == QualityType.Miss)
            {
                yield return StartCoroutine(HandleFailCO());
                continue;
            }

            // 스파이크
            bool isHit = false;
            Vector2 targetPos = Vector2.zero;
            yield return StartCoroutine(StepSpikeCO(tossQuality, result =>
            {
                isHit = result.isHit;
                targetPos = result.targetPos;
            }));

            if (!isHit)
            {
                yield return StartCoroutine(HandleFailCO());
                continue;
            }

            yield return StartCoroutine(StepEndCO(targetPos));
        }
    }

    private IEnumerator StepReadyCO()
    {
        yield return WaitForSecondsCache.Wait(_config.RallyInterval);
        EventManager.Instance.Trigger((int)EventType.OnRallyReady);

        // 공 및 선수 초기화    
        ResetRally();

        _isTouched = false;
        yield return new WaitUntil(() => _isTouched);

        EventManager.Instance.Trigger((int)EventType.OnRallyStart);
        SoundManager.Instance.PlaySFX(SoundKey.SFX_WHISTLE_START);

        yield return WaitForSecondsCache.Wait(_config.RallyInterval);
    }

    private IEnumerator StepReceiveCO(Action<QualityType> onDone)
    {
        float finalSpeed = GetDynamicSpeedMultiplier(_config.ServeSpeed);
        yield return StartCoroutine(WaitForInput(_playerHandler.GetPos(PlayerType.Libero), finalSpeed, false, onDone));
    }

    private IEnumerator StepTossCO(QualityType receiveQuality, Action<QualityType> onDone)
    {
        float speedMultiplier = GetSpeedMultifiler(receiveQuality);
        float finalSpeed = GetDynamicSpeedMultiplier(_config.ReceiveSpeed * speedMultiplier);
        yield return StartCoroutine(WaitForInput(_playerHandler.GetPos(PlayerType.Setter), finalSpeed, false, onDone));
    }

    private IEnumerator StepSpikeCO(QualityType tossQuality, Action<(bool isHit, Vector3 targetPos)> onDone)
    {
        EventManager.Instance.Trigger((int)EventType.OnSpike, true);

        bool ballArrived = false;
        float finalSpeed = GetDynamicSpeedMultiplier(_config.TossSpeed);

        Vector2 spikerPos = _playerHandler.GetPos(PlayerType.Spiker);

        _ball.MoveToTarget(
            _playerHandler.GetPos(PlayerType.Spiker),
            finalSpeed, true,
            () => _playerHandler.ExecuteAction(PlayerType.Spiker, tossQuality),
            () => ballArrived = true
            );

        yield return new WaitUntil(() => ballArrived);

        bool spikeDone = false;

        Game.Play.Spike.StartSpike(tossQuality, (result) =>
        {
            _playerHandler.ExecuteAction(PlayerType.Spiker, tossQuality);
            spikeDone = true;

            onDone?.Invoke((result.isHit, result.targetPos));
        });

        yield return new WaitUntil(() => spikeDone);
        EventManager.Instance.Trigger((int)EventType.OnSpike, false);
    }

    private IEnumerator StepEndCO(Vector2 targetPos)
    {
        Vector2 startPos = _ball.GetPosition();
        Vector2 hitDir = (targetPos - startPos).normalized;
        float finalSpeed = GetDynamicSpeedMultiplier(_config.AttackSpeed);
        float distance = Vector2.Distance(startPos, targetPos);

        RaycastHit2D hit = Physics2D.CircleCast(startPos, 0.2f, hitDir, distance, LayerMask.GetMask("Enemy"));

        if (hit.collider != null)
        {
            yield return BlockSpikeCO(hit, hitDir, finalSpeed);
            yield break;
        }

        yield return SuccessSpikeCO(targetPos, hitDir, finalSpeed);
    }

    private IEnumerator BlockSpikeCO(RaycastHit2D hit, Vector2 hitDir, float speed)
    {
        EditorLog.Log($"[RallyFlowManager] 차단됨: {hit.collider.name}");

        bool ballBlocked = false;
        _ball.SpikeToTarget(hit.point, speed, () =>
        {
            _ball.PlayEffect();
            _ball.MoveToTarget(_enemyReceivePos, _config.ReceiveSpeed, false, null, () => ballBlocked = true);
        });

        yield return new WaitUntil(() => ballBlocked);
        JudgeScore(false);
    }

    private IEnumerator SuccessSpikeCO(Vector2 targetPos, Vector2 hitDir, float speed)
    {
        bool ballLanded = false;

        _ball.SpikeToTarget(targetPos, speed, () =>
        {
            _ball.PlayEffect();
            ballLanded = true;
        });

        yield return new WaitUntil(() => ballLanded);

        bool isIn = IsInCourt(targetPos);
        TextEffectHelper.ShowInOutAtWorldPos(_ball.transform, 1.0f, isIn);

        _ball.OutReflect(hitDir, speed);

        yield return WaitForSecondsCache.Wait(0.5f);
        JudgeScore(isIn);
    }

    private IEnumerator HandleFailCO()
    {
        bool ballOut = false;
        _ball.OutRandom(() => ballOut = true);
        yield return new WaitUntil(() => ballOut);

        JudgeScore(false);
    }
    #endregion

    #region <<  REFREE LOGIC  >>
    private void JudgeScore(bool isScored)
    {
        SoundManager.Instance.PlaySFX(SoundKey.SFX_WHISTLE_END);

        if (isScored)
        {
            EditorLog.Log("[RallyFlowManager] 득점 : Ball In");
            ApplyDifficultyIncrease();
            Game.Play.AddScore(_config.BaseScore + (_config.ScoreIncrement * _comboCnt));
        }
        else
        {
            EditorLog.Log("[RallyFlowManager] 미스 : Ball Out");
            ResetCombo();
            Game.Play.Miss();
        }
    }

    private bool IsInCourt(Vector2 pos)
    {
        return pos.x >= _config.CourtMinX && pos.x <= _config.CourtMaxX &&
           pos.y >= _config.CourtMinY && pos.y <= _config.CourtMaxY;
    }
    #endregion

    #region <<  INPUT LOGIC  >>
    private void OnGlobalTouchReceived()
    {
        _isTouched = true;
    }

    private IEnumerator WaitForInput(Vector2 playerPos, float ballSpeed, bool isToss, Action<QualityType> callback)
    {
        _isTouched = false;

        bool isInputed = false;

        QualityType quality = QualityType.Miss;

        // 공 이동 시작
        bool ballArrived = false;
        _ball.MoveToTarget(playerPos, ballSpeed, isToss, null, () => ballArrived = true);

        // 입력 대기 루프
        while (!ballArrived)
        {
            if (_isTouched && !isInputed)
            {
                _isTouched = false;
                isInputed = true;

                QualityType curQuality = _inputHandler.CheckQuality(playerPos, _ball.GetPosition());

                if (curQuality != QualityType.Miss)
                {
                    quality = curQuality;
                    break;
                }
                else
                {
                    EditorLog.Log("[RallyFlowManager] 너무 일찍 터치함: Miss 판정 유지 및 대기");
                }
            }

            if (ballArrived) break;

            yield return null;
        }

        callback?.Invoke(quality);
    }
    #endregion

    #region <<  RESET LOGIC  >>
    private void ResetRally()
    {
        _playerHandler.ResetPlayer();
        _enemySpawner.SetEnemy(_comboCnt / 3);
        _ball.ResetPos(_playerHandler.GetPos(PlayerType.Server));
    }

    private void ResetCombo()
    {
        _comboCnt = 0;
        _dynamicSpeedMultiplier = 1f;

        EventManager.Instance.Trigger((int)EventType.OnComboChanged, _comboCnt);
    }
    #endregion

    #region <<  UTIL  >>

    private void ApplyDifficultyIncrease()
    {
        _comboCnt++;
        _dynamicSpeedMultiplier += _config.SpeedIncrement;

        EventManager.Instance.Trigger((int)EventType.OnComboChanged, _comboCnt);
        EditorLog.Log($"[RallyFlowManager] 콤보 횟수: {_comboCnt}, 현재 속도 배율: {_dynamicSpeedMultiplier}");
    }

    private float GetDynamicSpeedMultiplier(float speed)
    {
        return speed * _dynamicSpeedMultiplier;
    }

    private float GetSpeedMultifiler(QualityType quality)
    {
        switch (quality)
        {
            case QualityType.Perfect:
                return _config.PerfectMultiplier;

            case QualityType.Good:
                return _config.GoodMultiplier;

            case QualityType.Bad:
                return _config.BadMultiplier;

            default:
                return 1f;
        }
    }
    #endregion
}
