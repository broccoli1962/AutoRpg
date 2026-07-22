using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class MonsterData : IData
{
    public string id => _id;
    [SerializeField] private string _id;

    public string zoneId => _zoneId;
    [SerializeField] private string _zoneId;

    public string displayName => _displayName;
    [SerializeField] private string _displayName;

    public string rarity => _rarity;
    [SerializeField] private string _rarity;

    public int hp => _hp;
    [SerializeField] private int _hp;

    public int attack => _attack;
    [SerializeField] private int _attack;

    public int defense => _defense;
    [SerializeField] private int _defense;

    public int goldReward => _goldReward;
    [SerializeField] private int _goldReward;

	public void SetData(List<string> data)
	{
		_id = data.Count > 0 ? data[0] : string.Empty;
		_zoneId = data.Count > 1 ? data[1] : string.Empty;
		_displayName = data.Count > 2 ? data[2] : string.Empty;
		_rarity = data.Count > 3 ? data[3] : string.Empty;
		if (data.Count > 4 && !string.IsNullOrEmpty(data[4]))
		{
			_hp = int.Parse(data[4]);
		}
		if (data.Count > 5 && !string.IsNullOrEmpty(data[5]))
		{
			_attack = int.Parse(data[5]);
		}
		if (data.Count > 6 && !string.IsNullOrEmpty(data[6]))
		{
			_defense = int.Parse(data[6]);
		}
		if (data.Count > 7 && !string.IsNullOrEmpty(data[7]))
		{
			_goldReward = int.Parse(data[7]);
		}
	}
}
