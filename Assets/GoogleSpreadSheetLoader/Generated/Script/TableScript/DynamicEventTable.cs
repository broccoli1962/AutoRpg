using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "DynamicEventTable", menuName = "Tables/DynamicEventTable")]
public class DynamicEventTable : ScriptableObject, ITable
{
    public List<DynamicEventData> dataList = new List<DynamicEventData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<DynamicEventData>();
		foreach (var item in data)
		{
			DynamicEventData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
