using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JangLib;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class NicknameValidator : SingletonWithMono<NicknameValidator>, IBaseManager
{
    private HashSet<string> badWords = new HashSet<string>();
    public bool IsInitialized { private set; get; }

    public void Init()
    {
        if (IsInitialized) return;

        StartCoroutine(LoadBannedWordsCO());
        IsInitialized = true;
    }

    private IEnumerator LoadBannedWordsCO()
    {
        yield return new WaitUntil(() => PlayFabManager.Instance.IsLogin);

        var request = new GetTitleDataRequest { Keys = new List<string> { "BannedWord" } };

        PlayFabClientAPI.GetTitleData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("BannedWord"))
            {
                string rawData = result.Data["BannedWord"];
                string[] words = rawData.Split(new char[] { ' ', ',', '\n', '\r', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

                badWords.Clear();
                foreach (string word in words)
                {
                    badWords.Add(word.Trim());
                }

                EditorLog.Log("[Validator] 서버 금지어 리스트 로드 완료");
            }
        },
        error =>
        {
            EditorLog.LogError("[Validator] 금지어 로드 실패: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// 닉네임 유효성 검사
    /// </summary>
    public (bool isValid, string message) Validate(string nickname)
    {
        if (!IsInitialized)
            return (false, "System is initializing. Please try again in a moment.");

        if (string.IsNullOrWhiteSpace(nickname))
            return (false, "Please enter a nickname.");

        if (nickname.Length < 2 || nickname.Length > 10)
            return (false, "Nickname must be between 2 and 10 characters.");

        if (!Regex.IsMatch(nickname, @"^[a-zA-Z가-힣]+$"))
            return (false, "Only English and Korean characters are allowed.");

        if (ContainsBadWord(nickname))
            return (false, "This nickname contains inappropriate words.");

        return (true, "This nickname is available.");
    }

    private bool ContainsBadWord(string input)
    {
        string cleanInput = input.Replace(" ", "").ToLower();

        foreach (string badWord in badWords)
        {
            if (string.IsNullOrWhiteSpace(badWord)) continue;

            string lowerBadWord = badWord.ToLower().Trim();

            if (IsEnglish(lowerBadWord))
            {
                if (lowerBadWord.Length <= 3)
                {
                    if (cleanInput == lowerBadWord) return true;
                }
                else
                {
                    if (cleanInput.Contains(lowerBadWord)) return true;
                }
            }
            else
            {
                if (lowerBadWord.Length >= 2)
                {
                    if (cleanInput.Contains(lowerBadWord)) return true;
                }
                else
                {
                    if (cleanInput == lowerBadWord) return true;
                }
            }
        }

        return false;
    }

    private bool IsEnglish(string word)
    {
        return Regex.IsMatch(word, @"^[a-zA-Z0-9$!@#%^&*()_+=-]+$");
    }
}