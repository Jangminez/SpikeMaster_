using DG.Tweening;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardSlot : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _slotImage;
    [SerializeField] private Outline _myOutline;
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Header("Rank Colors")]
    [SerializeField] private Color _firstColor = Color.yellow;
    [SerializeField] private Color _secondColor = Color.cyan;
    [SerializeField] private Color _thirdColor = Color.red;
    [SerializeField] private Color _defaultColor = Color.white;

    public void SetData(PlayerLeaderboardEntry entry)
    {
        int rank = entry.Position + 1;
        _rankText.text = rank.ToString();

        UpdateSlotColor(rank);

        string displayName = string.IsNullOrEmpty(entry.DisplayName)
            ? entry.PlayFabId
            : entry.DisplayName;

        _nameText.text = displayName;
        _scoreText.text = entry.StatValue.ToString();

        bool isMe = entry.PlayFabId == PlayFabSettings.staticPlayer.PlayFabId;
        if (_myOutline != null)
        {
            _myOutline.enabled = isMe;

            if (isMe)
            {
                _myOutline.effectColor = Color.black;
                _myOutline.DOFade(0.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    private void UpdateSlotColor(int rank)
    {
        if (_slotImage == null) return;

        switch (rank)
        {
            case 1:
                _slotImage.color = _firstColor;
                break;
            case 2:
                _slotImage.color = _secondColor;
                break;
            case 3:
                _slotImage.color = _thirdColor;
                break;
            default:
                _slotImage.color = _defaultColor;
                break;
        }
    }
}
