using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "ZoneTable", menuName = "Tables/ZoneTable")]
public class ZoneTable : ScriptableObject, ITable
{
    public List<ZoneData> dataList = new List<ZoneData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<ZoneData>();
		foreach (var item in data)
		{
			ZoneData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
