using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using JangLib;
using System;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

#if ENABLE_IAP
using UnityEngine.Purchasing;
#endif

public enum LoginMethod { Guest, Google }

public class PlayFabManager : SingletonWithMono<PlayFabManager>, IBaseManager
{
    private const string LastLoginKey = "LastLoginMethod";
    private const string BestScoreKey = "BestScore";

    public bool IsInitialized { private set; get; }

    private string _customID;
    public bool IsLogin { private set; get; }

    public string Nickname { private set; get; }
    public string PlayFabId { private set; get; }

    public void Init()
    {
        IsInitialized = true;
    }

    #region << LOGIN LOGIC >>

    public void TryAutoLogin()
    {
        if (IsLogin) return;

        string lastMethod = PlayerPrefs.GetString(LastLoginKey, "None");

        switch (lastMethod)
        {
            case "Guest":
                LoginWithGuest();
                break;
            case "Google":
                LoginWithGoogle();
                break;
            default:
                EditorLog.Log("최초 실행 또는 로그아웃 상태입니다.");
                break;
        }
    }

    /// <summary>
    /// [게스트 로그인 버튼용] 기존 기기 고유 ID 방식을 사용합니다.
    /// </summary>
    public void LoginWithGuest()
    {
        EventManager.Instance.Trigger((int)EventType.OnLogined);

        _customID = GetOrCreateCustomID();

        var request = new LoginWithCustomIDRequest
        {
            CustomId = _customID,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true }
        };

        PlayFabClientAPI.LoginWithCustomID(request, result => OnLoginSuccess(result, LoginMethod.Guest), OnLoginFailure);
    }

    private string GetOrCreateCustomID()
    {
        string id = PlayerPrefs.GetString("PlayFabCustomID", string.Empty);
        if (string.IsNullOrEmpty(id))
        {
            id = SystemInfo.deviceUniqueIdentifier;

            if (string.IsNullOrEmpty(id) || id == "null" || id.Length < 5)
            {
                id = Guid.NewGuid().ToString();
            }

            PlayerPrefs.SetString("PlayFabCustomID", id);
            PlayerPrefs.Save();
        }
        return id;
    }

    public void LoginWithGoogle()
    {
        EventManager.Instance.Trigger((int)EventType.OnLogined);
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    private void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, ProcessGoogleLogin);
        }
    }

    private void ProcessGoogleLogin(string serverAuthCode)
    {
        var request = new LoginWithGooglePlayGamesServicesRequest
        {
            ServerAuthCode = serverAuthCode,
            CreateAccount = true,
            TitleId = PlayFabSettings.TitleId,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true }
        };

        PlayFabClientAPI.LoginWithGooglePlayGamesServices(request, result => OnLoginSuccess(result, LoginMethod.Google), OnLoginFailure);
    }

    public void LinkGoogleAccount(Action onLinked = null)
    {
        if (!IsLogin)
        {
            EditorLog.LogWarning("[PlayFab] 로그인 상태가 아닙니다. 먼저 게스트로 로그인하세요.");
            return;
        }

        string lastMethod = PlayerPrefs.GetString(LastLoginKey, "None");
        if (lastMethod == LoginMethod.Google.ToString())
        {
            EditorLog.Log("[PlayFab] 이미 구글 계정과 연동된 상태입니다.");
            return;
        }

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(false, serverAuthCode =>
                {
                    var request = new LinkGooglePlayGamesServicesAccountRequest
                    {
                        ServerAuthCode = serverAuthCode,
                        ForceLink = false,
                    };

                    PlayFabClientAPI.LinkGooglePlayGamesServicesAccount(request,
                        result =>
                        {
                            EditorLog.Log("[PlayFab] Google account linked successfully!");

                            PlayerPrefs.SetString(LastLoginKey, LoginMethod.Google.ToString());

                            onLinked?.Invoke();
                        },
                        error =>
                        {
                            if (error.Error == PlayFabErrorCode.AccountAlreadyLinked)
                            {
                                var popup = PopupManager.Instance.Show<ErrorPopup>("popup_error");
                                popup.SetMessage("This Google account is already linked to another player.");
                            }
                            else
                            {
                                OnLoginFailure(error);
                            }
                        });
                });
            }
            else
            {
                var popup = PopupManager.Instance.Show<ErrorPopup>("popup_error");
                popup.SetMessage("Google Authentication failed.");
            }
        });
    }

    private void OnLoginSuccess(LoginResult result, LoginMethod method)
    {
        CheckAppVersion(() =>
        {
            ProcessPostLogin(result, method);
        });
    }

    private void ProcessPostLogin(LoginResult result, LoginMethod method)
    {
        IsLogin = true;
        PlayerPrefs.SetString(LastLoginKey, method.ToString());
        PlayFabId = result.PlayFabId;

        string displayName = result.InfoResultPayload?.AccountInfo?.TitleInfo?.DisplayName;
        Nickname = displayName;

        if (string.IsNullOrEmpty(displayName))
        {
            StartCoroutine(ShowNicknamePopupCO());
        }

        EditorLog.Log($"[PlayFab] Login Success! Name: {displayName}");
        CheckUserNoAdsStatus();

        EventManager.Instance.Trigger((int)EventType.OnLoginSuccess, displayName, result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        EditorLog.LogError($"[PlayFab] Login Failed: {error.GenerateErrorReport()}");
        var popup = PopupManager.Instance.Show<ErrorPopup>("popup_error");
        popup.SetMessageWithAction(GetErrorMessage(error), () =>
        {
            PopupManager.Instance.CloseAll();
            ReLogin();
        });
    }

    public void Logout()
    {
        IsLogin = false;
        PlayerPrefs.DeleteKey(LastLoginKey);
        LoadSceneManager.LoadSceneAsync(SceneType.MainScene);
    }
    private void ReLogin()
    {
        IsLogin = false;
        LoadSceneManager.LoadSceneAsync(SceneType.MainScene);
    }

    public void DeleteAccount(Action<bool> onComplete = null)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "deletePlayerAccount",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                EditorLog.Log("[PlayFab] CloudScript 호출 성공. 계정 삭제 프로세스 시작.");

                if (result.FunctionResult != null)
                {
                    var json = result.FunctionResult.ToString();
                    EditorLog.Log($"[PlayFab] 서버 응답 상세: {json}");
                }

                // 로컬 데이터 정리
                PlayerPrefs.DeleteKey(LastLoginKey);
                PlayerPrefs.DeleteKey("PlayFabCustomID");
                IsLogin = false;

                onComplete?.Invoke(true);
                PopupManager.Instance.CloseAll();
                Logout();
            },
            error =>
            {
                EditorLog.LogError($"[PlayFab] 탈퇴 요청 실패: {error.GenerateErrorReport()}");
                onComplete?.Invoke(false);
            });
    }
    #endregion

    #region <<  NICKNAME LOGIC  >>
    private IEnumerator ShowNicknamePopupCO()
    {
        yield return new WaitUntil(() => GameManager.Instance.IsInitialized);

        PopupManager.Instance.Show<NicknamePopup>("popup_nickname");
    }

    /// <summary>
    /// 닉네임을 설정하거나 변경합니다.
    /// </summary>
    public void SetDisplayName(string name, Action<bool, string> onResult)
    {
        var validation = NicknameValidator.Instance.Validate(name);

        if (!validation.isValid)
        {
            EditorLog.LogWarning($"Nickname validation failed: {validation.message}");
            onResult?.Invoke(false, validation.message);
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Nickname = name;
                onResult?.Invoke(true, null);
            },
            error =>
            {
                string errorMessage = "An unknown error occurred.";

                switch (error.Error)
                {
                    case PlayFabErrorCode.NameNotAvailable:
                        errorMessage = "This nickname is already taken.";
                        break;
                    case PlayFabErrorCode.ProfaneDisplayName:
                        errorMessage = "This name contains prohibited words.";
                        break;
                    case PlayFabErrorCode.InvalidParams:
                        errorMessage = "Invalid nickname format.";
                        break;
                }

                EditorLog.LogError($"[PlayFab Error] {error.GenerateErrorReport()}");
                onResult?.Invoke(false, errorMessage);
            }
        );
    }
    #endregion

    #region << High Score (Statistics) >>
    /// <summary>
    /// 최고점수를 저장합니다.
    /// </summary>
    /// <param name="score"></param>
    public void SaveBestScore(int score)
    {
        if (score == 0) return;

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = BestScoreKey, Value = score }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => EditorLog.Log("[PlayFab] 최고 점수 업로드 완료"),
            error =>
            {
                EditorLog.LogError(error.GenerateErrorReport());
                var popup = PopupManager.Instance.Show<ErrorPopup>("popup_error");
                popup.SetMessageWithAction(GetErrorMessage(error), () => SaveBestScore(score));
            }
        );
    }

    /// <summary>
    /// 본인의 최고점수를 가져옵니다.
    /// </summary>
    public void GetBestScore(Action<int> onResult)
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { BestScoreKey }
        };

        PlayFabClientAPI.GetPlayerStatistics(request,
            result =>
            {
                int score = 0;
                foreach (var stat in result.Statistics)
                {
                    if (stat.StatisticName == BestScoreKey)
                    {
                        score = stat.Value;
                        break;
                    }
                }
                EditorLog.Log($"[PlayFab] 내 최고 점수 로드 성공: {score}");
                onResult?.Invoke(score);
            },
            error =>
            {
                EditorLog.LogError($"[PlayFab] 점수 로드 실패: {error.GenerateErrorReport()}");
                onResult?.Invoke(0); // 에러 시 0점 반환
            }
        );
    }

    /// <summary>
    /// 전 세계 리더보드 상위 순위를 가져옵니다.
    /// </summary>
    /// <param name="maxCount">가져올 유저 수 (기본 10명)</param>
    public void GetLeaderboard(int maxCount, Action<List<PlayerLeaderboardEntry>> onResult)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = BestScoreKey,
            StartPosition = 0,
            MaxResultsCount = maxCount
        };

        PlayFabClientAPI.GetLeaderboard(request,
            result =>
            {
                Debug.Log($"[PlayFab] 리더보드 {result.Leaderboard.Count}명 로드 완료");
                onResult?.Invoke(result.Leaderboard);
            },
            error =>
            {
                Debug.LogError($"[PlayFab] 리더보드 로드 실패: {error.GenerateErrorReport()}");
                onResult?.Invoke(null);
            }
        );
    }
    #endregion

    #region << Version Check >>
    /// <summary>
    /// 서버의 최신 버전과 현재 앱 버전을 비교합니다.
    /// </summary>
    private void CheckAppVersion(Action onSuccess)
    {
        var request = new GetTitleDataRequest();

        PlayFabClientAPI.GetTitleData(request,
            result =>
            {
                if (result.Data.ContainsKey("LatestVersion"))
                {
                    string serverVersion = result.Data["LatestVersion"];
                    string localVersion = Application.version;

                    if (serverVersion != localVersion)
                    {
                        EditorLog.Log($"[Version] Update Required! Server: {serverVersion} / Local: {localVersion}");
                        ShowUpdatePopup();
                    }
                    else
                    {
                        EditorLog.Log("[Version] App is up to date.");
                        onSuccess?.Invoke();
                    }
                }
            },
            error => EditorLog.LogError($"[Version] Check Failed: {error.GenerateErrorReport()}")
        );
    }

    private void ShowUpdatePopup()
    {
        var popup = PopupManager.Instance.Show<ErrorPopup>("popup_error");

        string msg = "A new version is available.\nPlease update for a better experience!";

        popup.SetMessageWithAction(msg, () =>
        {
            Application.OpenURL("market://details?id=com.JangM.SpikeMaster");
            ReLogin();
        });
    }
    #endregion

    #region <<  IAP LOGIC  >>
    private void CheckUserNoAdsStatus()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result =>
    {
        bool hasNoAds = result.Inventory.Exists(item => item.ItemId == IAP_Products.PRODUCT_NO_ADS_ID);

        if (hasNoAds)
        {
            EditorLog.Log("[IAP] 광고 제거 유저임을 확인했습니다.");
            EventManager.Instance.Trigger((int)EventType.OnSetNoAds);
        }
        else
        {
            EditorLog.Log("[IAP] 광고 제거 아이템이 없습니다.");
        }
    }, error =>
    {
        EditorLog.LogError("[IAP] 인벤토리 확인 실패");
    });
    }

#if ENABLE_IAP
    /// <summary>
    /// 구글 플레이 결제 영수증 검증
    /// </summary>
    /// <param name="item">구매한 Product 객체</param>
    /// <param name="onResult">성공 여부 콜백</param>
    public void ValidateGooglePurchase(Product product, PayloadData payload, Action<bool> onResult)
    {
        EditorLog.Log($"[IAP] 검증 시도 - JSON: {payload.json}");
        EditorLog.Log($"[IAP] 검증 시도 - Signature: {payload.signature}");

        var request = new ValidateGooglePlayPurchaseRequest
        {
            CurrencyCode = product.metadata.isoCurrencyCode,
            PurchasePrice = (uint)(product.metadata.localizedPrice * 100),
            ReceiptJson = payload.json,
            Signature = payload.signature
        };

        PlayFabClientAPI.ValidateGooglePlayPurchase(request, result =>
        {
            EditorLog.Log("[IAP] PlayFab 영수증 검증 및 아이템 지급 성공!");

            // 구매 성공 팝업
            var popup = PopupManager.Instance.Show<AlertPopup>("popup_alert");
            popup.SetAlert("Product purchase completed.");

            EventManager.Instance.Trigger((int)EventType.OnPurchasedNoAds);
            onResult?.Invoke(true);
        },
        error =>
        {
            EditorLog.LogError($"[IAP] 검증 실패: {error.GenerateErrorReport()}");
            onResult?.Invoke(false);
        });
    }
#endif
    #endregion

    #region <<  UTIL  >>
    private string GetErrorMessage(PlayFabError error)
    {
        switch (error.Error)
        {
            // 네트워크 및 서버 관련
            case PlayFabErrorCode.ConnectionError:
            case PlayFabErrorCode.ServiceUnavailable:
                return "Network connection is unstable.\nPlease check your internet settings.";

            // 로그인 및 인증 관련
            case PlayFabErrorCode.InvalidParams:
            case PlayFabErrorCode.InvalidSessionTicket:
                return "Session expired or invalid request.\nPlease try logging in again.";

            case PlayFabErrorCode.AccountNotFound:
                return "Account not found.\nPlease check your login method.";

            case PlayFabErrorCode.NameNotAvailable:
                return "This nickname is already in use.\nPlease try another one.";

            case PlayFabErrorCode.ProfaneDisplayName:
                return "This name contains inappropriate words.\nPlease choose a different name.";

            default:
                return "An unexpected error occurred.\nPlease try again later.";
        }
    }
    #endregion
}