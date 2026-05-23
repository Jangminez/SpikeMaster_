using System;
using JangLib;
using TMPro;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private CanvasGroup _mainGroup;
    [SerializeField] private CanvasGroup _loginGroup;

    [Header("Login Buttons")]
    [SerializeField] private BaseBtn _googleLoginBtn;
    [SerializeField] private BaseBtn _guestLoginBtn;


    [Header("Play Button")]
    [SerializeField] private BaseBtn _playBtn;

    [Header("Login UI")]
    [SerializeField] private TextMeshProUGUI _userInfoTMP;
    [SerializeField] private GameObject _loginEffect;

    private void Start()
    {
        if (PlayFabManager.Instance.IsLogin)
        {
            SetUserInfoUI(PlayFabManager.Instance.Nickname, PlayFabManager.Instance.PlayFabId);
        }
        else
        {
            SetLoginGroup();
        }

        if (_playBtn != null)
            _playBtn.OnClick += PlayGame;

        if (_guestLoginBtn != null)
            _guestLoginBtn.OnClick += OnClickGuestLogin;

        if (_googleLoginBtn != null)
            _googleLoginBtn.OnClick += OnClickGoogleLogin;

        EventManager.Instance.Register<Action>((int)EventType.OnLogined, SetLoginEffect);
        EventManager.Instance.Register<Action<string, string>>((int)EventType.OnLoginSuccess, SetUserInfoUI);
    }

    private void OnDestroy()
    {
        EventManager.Instance.UnRegister<Action>((int)EventType.OnLogined, SetLoginEffect);
        EventManager.Instance.UnRegister<Action<string, string>>((int)EventType.OnLoginSuccess, SetUserInfoUI);
    }

    private void OnClickGuestLogin()
    {
        PlayFabManager.Instance.LoginWithGuest();
    }

    private void OnClickGoogleLogin()
    {
        PlayFabManager.Instance.LoginWithGoogle();
    }

    private void SetLoginEffect()
    {
        _googleLoginBtn.SetActive(false);
        _guestLoginBtn.SetActive(false);
        _loginEffect.SetActive(true);
    }

    private void PlayGame()
    {
        if (PlayFabManager.Instance.IsLogin)
        {
            Game.Main.Play();
        }
        else
        {
            EditorLog.LogWarning("로그인이 필요합니다!");
        }
    }

    private void SetMainGroup()
    {
        _mainGroup.gameObject.SetActive(true);
        _mainGroup.interactable = true;
        _mainGroup.alpha = 1f;
        _mainGroup.blocksRaycasts = true;

        _loginGroup.gameObject.SetActive(false);
        _loginGroup.interactable = false;
        _loginGroup.alpha = 0f;
        _loginGroup.blocksRaycasts = false;
    }

    private void SetLoginGroup()
    {
        _loginEffect.SetActive(false);

        _loginGroup.gameObject.SetActive(true);
        _loginGroup.interactable = true;
        _loginGroup.alpha = 1f;
        _loginGroup.blocksRaycasts = true;

        _mainGroup.gameObject.SetActive(false);
        _mainGroup.interactable = false;
        _mainGroup.alpha = 0f;
        _mainGroup.blocksRaycasts = false;
    }

    private void SetUserInfoUI(string nickname, string id)
    {
        SetMainGroup();
        _userInfoTMP.text = $"Logged in: {nickname} ({id})";
    }
}