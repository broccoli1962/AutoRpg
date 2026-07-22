using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTable", menuName = "Tables/CharacterTable")]
public class CharacterTable : ScriptableObject, ITable
{
    public List<CharacterData> dataList = new List<CharacterData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<CharacterData>();
		foreach (var item in data)
		{
			CharacterData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
