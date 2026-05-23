using System.Collections.Generic;
using UnityEngine;
using JangLib;

[System.Serializable]
public class PopupData
{
	public string key;
	public string path;
}

public class PopupDataLoader
{
	public List<PopupData> ItemList { get; private set; }
	public Dictionary<string, PopupData> ItemDict { get; private set; }

	public PopupDataLoader(string path = "JsonData/popup_data")
	{
		TextAsset json = Resources.Load<TextAsset>(path);
		if (json == null)
		{
			EditorLog.LogWarning($"{path} 데이터가 존재하지 않습니다.");
			return;
		}

		ItemList = JsonUtility.FromJson<PopupDataWrapper>(json.text).Items;
		ItemDict = new Dictionary<string, PopupData>();
		foreach (var item in ItemList)
		{
			ItemDict[item.key] = item;
		}
	}

	public PopupData GetData(string key)
	{
		ItemDict.TryGetValue(key, out PopupData data);
		return data;
	}
}

/// <summary>
/// PopupData 리스트 Wrapper 클래스
/// </summary>
[System.Serializable]
public class PopupDataWrapper
{
	public List<PopupData> Items;
}
