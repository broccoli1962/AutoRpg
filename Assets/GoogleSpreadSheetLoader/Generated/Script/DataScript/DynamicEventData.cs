using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class DynamicEventData : IData
{
    public string id => _id;
    [SerializeField] private string _id;

    public string category => _category;
    [SerializeField] private string _category;

    public string triggerType => _triggerType;
    [SerializeField] private string _triggerType;

    public string zoneMin => _zoneMin;
    [SerializeField] private string _zoneMin;

    public string zoneMax => _zoneMax;
    [SerializeField] private string _zoneMax;

    public float probability => _probability;
    [SerializeField] private float _probability;

    public string intensity => _intensity;
    [SerializeField] private string _intensity;

	public void SetData(List<string> data)
	{
		_id = data.Count > 0 ? data[0] : string.Empty;
		_category = data.Count > 1 ? data[1] : string.Empty;
		_triggerType = data.Count > 2 ? data[2] : string.Empty;
		_zoneMin = data.Count > 3 ? data[3] : string.Empty;
		_zoneMax = data.Count > 4 ? data[4] : string.Empty;
		if (data.Count > 5 && !string.IsNullOrEmpty(data[5]))
		{
			_probability = float.Parse(data[5]);
		}
		_intensity = data.Count > 6 ? data[6] : string.Empty;
	}
}
