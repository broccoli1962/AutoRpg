using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class DiscoveryData : IData
{
    public string itemId => _itemId;
    [SerializeField] private string _itemId;

    public string zoneId => _zoneId;
    [SerializeField] private string _zoneId;

    public string displayName => _displayName;
    [SerializeField] private string _displayName;

    public int quantity => _quantity;
    [SerializeField] private int _quantity;

    public int goldValue => _goldValue;
    [SerializeField] private int _goldValue;

	public void SetData(List<string> data)
	{
		_itemId = data.Count > 0 ? data[0] : string.Empty;
		_zoneId = data.Count > 1 ? data[1] : string.Empty;
		_displayName = data.Count > 2 ? data[2] : string.Empty;
		if (data.Count > 3 && !string.IsNullOrEmpty(data[3]))
		{
			_quantity = int.Parse(data[3]);
		}
		if (data.Count > 4 && !string.IsNullOrEmpty(data[4]))
		{
			_goldValue = int.Parse(data[4]);
		}
	}
}
