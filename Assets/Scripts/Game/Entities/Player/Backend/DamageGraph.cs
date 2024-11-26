using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// @alex-memo 2023
/// This class is responsible for 
/// </summary>
[System.Serializable]
public class DamageGraph
{
	private HashSet<DamageDealer> damageDealers = new();
	public UserData lastDamageDealer = null;
	public float lastDamageTakenTime = float.MinValue;
	[SerializeField] private List<DamageDealer> damageDealersList = new();
	public void AddDamage(float _damage, Element _element, UserData _dmgDealerData)
	{
		DamageDealer _newDealer = new()
		{
			UserData = _dmgDealerData,
		};
		if (damageDealers.Contains(_newDealer))
		{
			damageDealers.First(x => x.UserData.PlayerID == _dmgDealerData.PlayerID).AddDamage(_damage, _element);
		}
		else
		{
			_newDealer.AddDamage(_damage, _element);
			damageDealers.Add(_newDealer);
		}
		damageDealersList = damageDealers.ToList();
		if (_dmgDealerData.ChosenToolID < -500) { return; }
		lastDamageDealer = _dmgDealerData;
		lastDamageTakenTime = Time.time;
		damageDealersList = damageDealers.ToList();
	}
	public HashSet<DamageDealer> GetDamageDealers(bool _excludeZoneIonizationDamage = true)
	{
		if (_excludeZoneIonizationDamage)
		{
			var _tempList = new HashSet<DamageDealer>(damageDealers);
			_tempList.RemoveWhere(x => x.UserData.ChosenToolID < -500);
			return _tempList;
		}
		return damageDealers;
	}
	public override string ToString()
	{
		string _result = "Damage Graph Summary:\n";
		foreach (DamageDealer _damageDealer in damageDealers)
		{
			_result += _damageDealer.ToString();//$"DmgDealerData: {_damageDealer.UserData}, Water Damage: {_damageDealer.waterDamageDealt}, Earth Damage: {_damageDealer.earthDamageDealt}, Wind Damage: {_damageDealer.windDamageDealt}, Fire Damage: {_damageDealer.fireDamageDealt}, Plant Damage: {_damageDealer.plantDamageDealt}, Lightning Damage: {_damageDealer.lightningDamageDealt}, Ice Damage: {_damageDealer.iceDamageDealt}, Physical Damage: {_damageDealer.physicalDamageDealt}, Void Damage: {_damageDealer.voidDamageDealt}\n";
		}
		return _result;
	}
}
[System.Serializable]
public class DamageDealer : INetworkSerializable
{
	public DamageDealer() { }
	public DamageDealer(DamageDealer _dealer)
	{
		UserData = _dealer.UserData;
		waterDamageDealt = _dealer.waterDamageDealt;
		earthDamageDealt = _dealer.earthDamageDealt;
		windDamageDealt = _dealer.windDamageDealt;
		fireDamageDealt = _dealer.fireDamageDealt;
		plantDamageDealt = _dealer.plantDamageDealt;
		lightningDamageDealt = _dealer.lightningDamageDealt;
		iceDamageDealt = _dealer.iceDamageDealt;
		physicalDamageDealt = _dealer.physicalDamageDealt;
		voidDamageDealt = _dealer.voidDamageDealt;
	}
	public UserData UserData;

	public float waterDamageDealt;
	public float earthDamageDealt;
	public float windDamageDealt;
	public float fireDamageDealt;
	public float plantDamageDealt;
	public float lightningDamageDealt;
	public float iceDamageDealt;
	public float physicalDamageDealt;
	public float voidDamageDealt;
	public void AddDamage(float _damage, Element _type)
	{
		switch (_type)
		{
			case Element.Water:
				waterDamageDealt += _damage;
				break;
			case Element.Earth:
				earthDamageDealt += _damage;
				break;
			case Element.Wind:
				windDamageDealt += _damage;
				break;
			case Element.Fire:
				fireDamageDealt += _damage;
				break;
			case Element.Plant:
				plantDamageDealt += _damage;
				break;
			case Element.Lightning:
				lightningDamageDealt += _damage;
				break;
			case Element.Ice:
				iceDamageDealt += _damage;
				break;
			case Element.Physical:
				physicalDamageDealt += _damage;
				break;
			case Element.Void:
				voidDamageDealt += _damage;
				break;
		}
	}
	public void AddDamage(float _waterDamage, float _earthDamage, float _windDamage, float _fireDamage, float _plantDamage, float _lightningDamage, float _iceDamage, float _physicalDamage, float _voidDamage)
	{
		System.Text.StringBuilder debugLog = new($"Added damage to {UserData.PlayerName}:");
		if (_waterDamage > 0)
		{
			waterDamageDealt += _waterDamage;
			debugLog.AppendLine($"Water: {_waterDamage}");
		}
		if (_earthDamage > 0)
		{
			earthDamageDealt += _earthDamage;
			debugLog.AppendLine($"Earth: {_earthDamage}");
		}
		if (_windDamage > 0)
		{
			windDamageDealt += _windDamage;
			debugLog.AppendLine($"Wind: {_windDamage}");
		}
		if (_fireDamage > 0)
		{
			fireDamageDealt += _fireDamage;
			debugLog.AppendLine($"Fire: {_fireDamage}");
		}
		if (_plantDamage > 0)
		{
			plantDamageDealt += _plantDamage;
			debugLog.AppendLine($"Plant: {_plantDamage}");
		}
		if (_lightningDamage > 0)
		{
			lightningDamageDealt += _lightningDamage;
			debugLog.AppendLine($"Lightning: {_lightningDamage}");
		}
		if (_iceDamage > 0)
		{
			iceDamageDealt += _iceDamage;
			debugLog.AppendLine($"Ice: {_iceDamage}");
		}
		if (_physicalDamage > 0)
		{
			physicalDamageDealt += _physicalDamage;
			debugLog.AppendLine($"Physical: {_physicalDamage}");
		}
		if (_voidDamage > 0)
		{
			voidDamageDealt += _voidDamage;
			debugLog.AppendLine($"Void: {_voidDamage}");
		}
		Debug.Log(debugLog.ToString());
	}
	public float GetTotalDamage()
	{
		float _totalDamage = waterDamageDealt + earthDamageDealt + windDamageDealt + fireDamageDealt + plantDamageDealt + lightningDamageDealt + iceDamageDealt + physicalDamageDealt + voidDamageDealt;
		return _totalDamage;
	}
	public override string ToString()
	{
		if (UserData == null) { return "UserData is null"; }
		return $"DD: {UserData.PlayerName} - CID{UserData.ClientID} - Total Damage: {GetTotalDamage()}";
		//return $"Damage Dealer Name: {UserData.PlayerName}\nWater Damage: {waterDamageDealt}\nEarth Damage: {earthDamageDealt}\nWind Damage: {windDamageDealt}\nFire Damage: {fireDamageDealt}\nPlant Damage: {plantDamageDealt}\nLightning Damage: {lightningDamageDealt}\nIce Damage: {iceDamageDealt}\nPhysical Damage: {physicalDamageDealt}\nVoid Damage: {voidDamageDealt}";
		//return $"Owner ID: {ownerObjectId}, Client ID: {damageDealerClientId}, Physical Damage: {physicalDamageDealt}, Magic Damage: {magicDamageDealt}, True Damage: {trueDamageDealt}";
	}
	public override int GetHashCode()
	{
		return UserData.GetHashCode();
	}
	public override bool Equals(object _obj)
	{
		if (_obj == null) { return false; }
		if (_obj.GetType() != GetType()) { return false; }
		return UserData.Equals(((DamageDealer)_obj).UserData);
	}
	public void NetworkSerialize<T>(BufferSerializer<T> _serializer) where T : IReaderWriter
	{
		if (_serializer.IsReader)
		{
			var reader = _serializer.GetFastBufferReader();
			reader.ReadValueSafe(out UserData);

			reader.ReadValueSafe(out waterDamageDealt);
			reader.ReadValueSafe(out earthDamageDealt);
			reader.ReadValueSafe(out windDamageDealt);
			reader.ReadValueSafe(out fireDamageDealt);
			reader.ReadValueSafe(out plantDamageDealt);
			reader.ReadValueSafe(out lightningDamageDealt);
			reader.ReadValueSafe(out iceDamageDealt);
			reader.ReadValueSafe(out physicalDamageDealt);
			reader.ReadValueSafe(out voidDamageDealt);
		}
		else
		{
			var writer = _serializer.GetFastBufferWriter();
			writer.WriteValueSafe(UserData);

			writer.WriteValueSafe(waterDamageDealt);
			writer.WriteValueSafe(earthDamageDealt);
			writer.WriteValueSafe(windDamageDealt);
			writer.WriteValueSafe(fireDamageDealt);
			writer.WriteValueSafe(plantDamageDealt);
			writer.WriteValueSafe(lightningDamageDealt);
			writer.WriteValueSafe(iceDamageDealt);
			writer.WriteValueSafe(physicalDamageDealt);
			writer.WriteValueSafe(voidDamageDealt);
		}
	}
}

public enum Element { Water, Earth, Wind, Fire, Plant, Lightning, Ice, Physical, Void, NULL }
public enum ReactionType { NULL, Vaporize, Vortex, Freeze, Flourish, Conduct, Dissolve, Burn, Melt, Overload, Overgrowth, Shatter }