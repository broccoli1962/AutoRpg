using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "DiscoveryTable", menuName = "Tables/DiscoveryTable")]
public class DiscoveryTable : ScriptableObject, ITable
{
    public List<DiscoveryData> dataList = new List<DiscoveryData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<DiscoveryData>();
		foreach (var item in data)
		{
			DiscoveryData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
