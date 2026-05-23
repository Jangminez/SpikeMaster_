using System.Collections.Generic;
using PlayFab.ClientModels;
using UnityEngine;

public class PopupLeaderboard : BasePopup
{
    [Header("UI References")]
    [SerializeField] private Transform _slotParent;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private GameObject _loadingObject;

    private List<GameObject> _spawnedSlots = new List<GameObject>();

    public override void Open()
    {
        base.Open();

        Refresh();
    }

    public override void Close()
    {
        base.Close();
    }

    public void Refresh()
    {
        ClearList();
        _loadingObject.SetActive(true);

        PlayFabManager.Instance.GetLeaderboard(10, (entries) =>
        {
            _loadingObject.SetActive(false);
            if (entries != null)
            {
                UpdateUI(entries);
            }
        });
    }

    private void UpdateUI(List<PlayerLeaderboardEntry> entries)
    {
        foreach (var entry in entries)
        {
            var obj = Instantiate(_slotPrefab, _slotParent);

            if (obj.TryGetComponent(out LeaderboardSlot slot))
            {
                slot.SetData(entry);
                _spawnedSlots.Add(obj);
            }
        }
    }

    private void ClearList()
    {
        foreach (var item in _spawnedSlots)
        {
            Destroy(item);
        }
    }

    protected override void SetEvents(bool isOpen)
    {

    }
}
