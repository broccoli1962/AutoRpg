using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class CharacterData : IData
{
    public string id => _id;
    [SerializeField] private string _id;

    public string displayName => _displayName;
    [SerializeField] private string _displayName;

    public string role => _role;
    [SerializeField] private string _role;

    public string personality => _personality;
    [SerializeField] private string _personality;

    public int str => _str;
    [SerializeField] private int _str;

    public int agi => _agi;
    [SerializeField] private int _agi;

    public int intel => _intel;
    [SerializeField] private int _intel;

    public int vit => _vit;
    [SerializeField] private int _vit;

    public int luk => _luk;
    [SerializeField] private int _luk;

	public void SetData(List<string> data)
	{
		_id = data.Count > 0 ? data[0] : string.Empty;
		_displayName = data.Count > 1 ? data[1] : string.Empty;
		_role = data.Count > 2 ? data[2] : string.Empty;
		_personality = data.Count > 3 ? data[3] : string.Empty;
		if (data.Count > 4 && !string.IsNullOrEmpty(data[4]))
		{
			_str = int.Parse(data[4]);
		}
		if (data.Count > 5 && !string.IsNullOrEmpty(data[5]))
		{
			_agi = int.Parse(data[5]);
		}
		if (data.Count > 6 && !string.IsNullOrEmpty(data[6]))
		{
			_intel = int.Parse(data[6]);
		}
		if (data.Count > 7 && !string.IsNullOrEmpty(data[7]))
		{
			_vit = int.Parse(data[7]);
		}
		if (data.Count > 8 && !string.IsNullOrEmpty(data[8]))
		{
			_luk = int.Parse(data[8]);
		}
	}
}
