using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RallyConfigSO", menuName = "GameConfig/RallyConfig")]
public class RallyConfigSO : ScriptableObject
{
    [Header("Court Settings")]
    [SerializeField] private float _rallyInterval = 1f;
    [SerializeField] private float _courtMinX = -2.9f;
    [SerializeField] private float _courtMaxX = 2.9f;
    [SerializeField] private float _courtMinY = 0f;
    [SerializeField] private float _courtMaxY = 5.28f;

    [Header("Rally Ball Settings")]
    [SerializeField] private float _serveSpeed = 5f;
    [SerializeField] private float _receiveSpeed = 3f;
    [SerializeField] private float _tossSpeed = 3f;
    [SerializeField] private float _attackSpeed = 10f;

    [Header("Rally Input Settings")]
    [SerializeField] private float _perfectRange = 0.1f;
    [SerializeField] private float _goodRange = 0.3f;
    [SerializeField] private float _badRange = 0.7f;

    [Header("Rally Settings")]
    [SerializeField] private float _perfectMultiplier = 0.7f;
    [SerializeField] private float _goodMultiplier = 1f;
    [SerializeField] private float _badMultiplier = 1.3f;
    [SerializeField] private float _speedIncrement = 0.05f;
    [SerializeField] private int _baseScore = 30;
    [SerializeField] private int _scoreIncrement = 6;

    public float RallyInterval => _rallyInterval;
    public float CourtMinX => _courtMinX;
    public float CourtMaxX => _courtMaxX;
    public float CourtMinY => _courtMinY;
    public float CourtMaxY => _courtMaxY;

    public float ServeSpeed => _serveSpeed;
    public float ReceiveSpeed => _receiveSpeed;
    public float TossSpeed => _tossSpeed;
    public float AttackSpeed => _attackSpeed;

    public float PerfectRange => _perfectRange;
    public float GoodRange => _goodRange;
    public float BadRange => _badRange;

    public float PerfectMultiplier => _perfectMultiplier;
    public float GoodMultiplier => _goodMultiplier;
    public float BadMultiplier => _badMultiplier;
    public float SpeedIncrement => _speedIncrement;
    public int BaseScore => _baseScore;
    public int ScoreIncrement => _scoreIncrement;
}
