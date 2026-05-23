using UnityEngine;
using UnityEngine.UI;

public class PlayUI : MonoBehaviour
{
    [SerializeField] private RallyUI _rallyUI;

    [Header("Play UI")]
    [SerializeField] private Button _settingBtn;

    private void Start()
    {
        _rallyUI.Init();
    }
}
