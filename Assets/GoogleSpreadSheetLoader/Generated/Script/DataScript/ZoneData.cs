using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class ZoneData : IData
{
    public string id => _id;
    [SerializeField] private string _id;

    public string displayName => _displayName;
    [SerializeField] private string _displayName;

    public int minFloor => _minFloor;
    [SerializeField] private int _minFloor;

    public int maxFloor => _maxFloor;
    [SerializeField] private int _maxFloor;

    public float rewardMultiplier => _rewardMultiplier;
    [SerializeField] private float _rewardMultiplier;

    public float riskMultiplier => _riskMultiplier;
    [SerializeField] private float _riskMultiplier;

	public void SetData(List<string> data)
	{
		_id = data.Count > 0 ? data[0] : string.Empty;
		_displayName = data.Count > 1 ? data[1] : string.Empty;
		if (data.Count > 2 && !string.IsNullOrEmpty(data[2]))
		{
			_minFloor = int.Parse(data[2]);
		}
		if (data.Count > 3 && !string.IsNullOrEmpty(data[3]))
		{
			_maxFloor = int.Parse(data[3]);
		}
		if (data.Count > 4 && !string.IsNullOrEmpty(data[4]))
		{
			_rewardMultiplier = float.Parse(data[4]);
		}
		if (data.Count > 5 && !string.IsNullOrEmpty(data[5]))
		{
			_riskMultiplier = float.Parse(data[5]);
		}
	}
}
