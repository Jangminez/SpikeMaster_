using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JangLib;

public class PopupManager : SingletonWithMono<PopupManager>, IBaseManager
{

    public bool IsInitialized { private set; get; }

    [Header("UI Root")]
    [SerializeField] private Canvas _popupCanvas;
    [SerializeField] private GraphicRaycaster _raycaster;
    [SerializeField] private Transform _safeArea;
    [SerializeField] private Image _blurImg;

    private PopupDataLoader _dataLoader;
    private Dictionary<string, IPopup> _popups = new Dictionary<string, IPopup>();
    private Stack<IPopup> _popupStack = new Stack<IPopup>(); // 현재 열린 순서 관리

    public bool IsPopupOpen => _popupStack.Count > 0;

    public void Init()
    {
        _popupCanvas = GetComponentInChildren<Canvas>();
        _raycaster = GetComponentInChildren<GraphicRaycaster>();
        _safeArea = _popupCanvas.transform.Find("SafeArea");

        SetupCanvas(_popupCanvas);

        _dataLoader = new PopupDataLoader();

        RefreshPopupState();

        IsInitialized = true;
    }

    private void SetupCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }

        canvas.planeDistance = 5f;

        if (canvas.TryGetComponent(out CanvasScaler scaler))
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // 기준 해상도
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
        }

        _popupCanvas.sortingOrder = 999;

    }

    private void RefreshPopupState()
    {
        if (_raycaster != null)
        {
            _raycaster.enabled = IsPopupOpen;
            _blurImg.enabled = IsPopupOpen;
        }

        Time.timeScale = IsPopupOpen ? 0f : 1f;
    }


    /// <summary>
    /// 키값을 통해 팝업을 띄웁니다.
    /// </summary>
    public T Show<T>(string key) where T : class, IPopup
    {
        if (!_popups.TryGetValue(key, out var popup))
        {
            // 데이터 시트에서 프리팹 찾기
            string path = _dataLoader.GetData(key).path;
            GameObject prefab = Resources.Load<GameObject>(path);

            // 팝업 생성 및 캔버스 자식으로 설정
            GameObject obj = Instantiate(prefab, _safeArea.transform);
            popup = obj.GetComponent<IPopup>();
            _popups.Add(key, popup);
        }

        popup.SetAsLastSibling();
        popup.Open();
        _popupStack.Push(popup);

        RefreshPopupState();

        return popup as T;
    }

    /// <summary>
    /// 가장 최근에 열린 팝업을 닫습니다.
    /// </summary>
    public void HideLatest()
    {
        if (IsPopupOpen)
        {
            IPopup popup = _popupStack.Pop();
            popup.Close();
        }

        RefreshPopupState();
    }

    public void CloseAll()
    {
        if (IsPopupOpen)
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Close();
            }
        }

        RefreshPopupState();
    }
}