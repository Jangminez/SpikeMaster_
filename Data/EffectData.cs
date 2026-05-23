using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectData
{
	public string key;
	public string path;
}

public class EffectDataLoader
{
	public List<EffectData> ItemList { get; private set; }
	public Dictionary<string, EffectData> ItemDict { get; private set; }

	public EffectDataLoader(string path = "JsonData/effect_data")
	{
		TextAsset json = Resources.Load<TextAsset>(path);
		if (json == null)
		{
			Debug.LogWarning($"{path} 데이터가 존재하지 않습니다.");
			return;
		}

		ItemList = JsonUtility.FromJson<EffectDataWrapper>(json.text).Items;
		ItemDict = new Dictionary<string, EffectData>();
		foreach (var item in ItemList)
		{
			ItemDict[item.key] = item;
		}
	}

	public EffectData GetData(string key)
	{
		ItemDict.TryGetValue(key, out EffectData data);
		return data;
	}
}

/// <summary>
/// EffectData 리스트 Wrapper 클래스
/// </summary>
[System.Serializable]
public class EffectDataWrapper
{
	public List<EffectData> Items;
}
