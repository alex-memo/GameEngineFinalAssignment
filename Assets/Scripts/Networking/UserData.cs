using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
[Serializable]
public class UserData : INetworkSerializable, IEquatable<UserData>
{
	public string PlayerName;
	public FixedString128Bytes PlayerID;
	public int PlayerIconID;
	public int PlayerSkinID;
	public int ChosenToolID;
	public int[] ChosenAbilitiesID = new int[4];
	public Element[] ChosenElements = new Element[2];
	public bool IsKiwiAccount { get; private set; }
	public FixedString32Bytes Team;
	public ulong ClientID;
	#region Party

	public bool IsPartyLeader = false;
	public FixedString32Bytes[] PartyMembersID;

	#endregion
	public UserData()
	{
		PlayerName = "Loading...";
		PlayerID = "Loading...";
		PlayerIconID = 0;
		PlayerSkinID = 0;
		ChosenToolID = 0;
		for (int i = 0; i < ChosenAbilitiesID.Length; i++)
		{
			ChosenAbilitiesID[i] = -1;
		}
		for (int i = 0; i < ChosenElements.Length; i++)
		{
			ChosenElements[i] = Element.NULL;
		}

		Team = "";
		PartyMembersID = new FixedString32Bytes[] { "0" };
		ClientID = 0;
	}
	public UserData(UserData _userData)
	{
		PlayerName = _userData.PlayerName;
		PlayerID = _userData.PlayerID;
		PlayerIconID = _userData.PlayerIconID;
		PlayerSkinID = _userData.PlayerSkinID;
		ChosenToolID = _userData.ChosenToolID;
		ChosenAbilitiesID = _userData.ChosenAbilitiesID;
		ChosenElements = _userData.ChosenElements;
		IsKiwiAccount = _userData.IsKiwiAccount;
		Team = _userData.Team;
		PartyMembersID = _userData.PartyMembersID;
		ClientID = _userData.ClientID;
	}
	public UserData(int _playerIconID, int _playerSkinID, int _chosenToolID, int[] _chosenAbilitiesID, bool _isKiwiAccount = false)
	{
		PlayerName = "Loading...";
		PlayerID = "Loading...";
		PlayerIconID = _playerIconID;
		PlayerSkinID = _playerSkinID;
		ChosenToolID = _chosenToolID;
		SetChosenAbilities(_chosenAbilitiesID);
		int _chosenElementsIndex = 0;

		for (int i = 0; i < ChosenElements.Length; i++)
		{
			ChosenElements[i] = Element.NULL;
		}

		foreach (var _ability in _chosenAbilitiesID)
		{
			if (_ability == -1)
			{
				//Debug.Log("No ability on this index!");
				continue;
			}
			Element _abilityElement = StaticListManager.Instance.GetAbility(_ability).Element;
			//here we would check if physical is in the chosen elements, if it is we would skip it
			//this is so that physical can be a flex element
			if (ChosenElements.Contains(_abilityElement))
			{
				continue;
			}
			ChosenElements[_chosenElementsIndex] = _abilityElement;
			++_chosenElementsIndex;
			if (_chosenElementsIndex == 2)
			{
				break;
			}
		}
		IsKiwiAccount = _isKiwiAccount;
		Team = "";
		PartyMembersID = new FixedString32Bytes[] { "0" };
		ClientID = 0;
		//Debug.Log(this);
	}
	public void SetChosenElements(Element _element1, Element _element2)
	{
		ChosenElements[0] = _element1;
		ChosenElements[1] = _element2;
	}
	public void SetChosenAbilities(int _ability1, int _ability2, int _ability3, int _ability4)
	{
		ChosenAbilitiesID[0] = _ability1;
		ChosenAbilitiesID[1] = _ability2;
		ChosenAbilitiesID[2] = _ability3;
		ChosenAbilitiesID[3] = _ability4;
	}
	public void SetChosenAbilities(int[] _abilities)
	{// we do it this way to guarantee there are 4 abilities
		ChosenAbilitiesID[0] = _abilities[0];
		ChosenAbilitiesID[1] = _abilities[1];
		ChosenAbilitiesID[2] = _abilities[2];
		ChosenAbilitiesID[3] = _abilities[3];
	}
	public void SetChosenAbility(int _abilityIndex, int _abilityID)
	{
		ChosenAbilitiesID[_abilityIndex] = _abilityID;
	}
	public bool ValidatePlayerData()
	{
		if (string.IsNullOrEmpty(PlayerName))
		{
			return false;
		}
		if (ChosenToolID == -1)
		{
			return false;
		}
		foreach (var ability in ChosenAbilitiesID)
		{
			if (ability == -1)
			{
				return false;
			}
		}
		//validate if all of the abilities are unique
		for (int i = 0; i < ChosenAbilitiesID.Length; i++)
		{
			for (int j = i + 1; j < ChosenAbilitiesID.Length; j++)
			{
				if (ChosenAbilitiesID[i] == ChosenAbilitiesID[j])
				{
					return false;
				}
			}
		}
		if (ChosenElements[0] == Element.NULL || ChosenElements[1] == Element.NULL)
		{
			return false;
		}
		return true;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> _serializer) where T : IReaderWriter
	{
		if (_serializer.IsReader)
		{
			var _reader = _serializer.GetFastBufferReader();
			_reader.ReadValueSafe(out PlayerName);
			_reader.ReadValueSafe(out PlayerID);
			_reader.ReadValueSafe(out PlayerIconID);
			_reader.ReadValueSafe(out PlayerSkinID);
			_reader.ReadValueSafe(out ChosenToolID);
			_reader.ReadValueSafe(out ChosenAbilitiesID);
			_reader.ReadValueSafe(out ChosenElements);
			_reader.ReadValueSafe(out IsPartyLeader);
			_reader.ReadValueSafe(out PartyMembersID);
			_reader.ReadValueSafe(out Team);
			_reader.ReadValueSafe(out ClientID);
		}
		else
		{
			var _writer = _serializer.GetFastBufferWriter();
			_writer.WriteValueSafe(PlayerName);
			_writer.WriteValueSafe(PlayerID);
			_writer.WriteValueSafe(PlayerIconID);
			_writer.WriteValueSafe(PlayerSkinID);
			_writer.WriteValueSafe(ChosenToolID);
			_writer.WriteValueSafe(ChosenAbilitiesID);
			_writer.WriteValueSafe(ChosenElements);
			_writer.WriteValueSafe(IsPartyLeader);
			_writer.WriteValueSafe(PartyMembersID);
			_writer.WriteValueSafe(Team);
			_writer.WriteValueSafe(ClientID);
		}
	}
	public override string ToString()
	{
		return $"PlayerName: {PlayerName}, PlayerID: {PlayerID}, PlayerIconID: {PlayerIconID}, PlayerSkinID: {PlayerSkinID}, ChosenToolID: {ChosenToolID}, ChosenAbilitiesID: {string.Join(",", ChosenAbilitiesID)}, ChosenElements: {string.Join(",", ChosenElements)}, IsKiwiAccount: {IsKiwiAccount} IsPartyLeader: {IsPartyLeader} Team: {Team}";
	}

	public bool Equals(UserData _other)
	{
		if(_other == null) { return false; }
		if (PlayerName != _other.PlayerName) { return false; }
		if (PlayerID != _other.PlayerID) { return false; }
		if (PlayerIconID != _other.PlayerIconID) { return false; }
		if (PlayerSkinID != _other.PlayerSkinID) { return false; }
		if (ChosenToolID != _other.ChosenToolID) { return false; }
		if (!ChosenAbilitiesID.SequenceEqual(_other.ChosenAbilitiesID)) { return false; }
		if (!ChosenElements.SequenceEqual(_other.ChosenElements)) { return false; }
		//if (IsKiwiAccount != _other.IsKiwiAccount){return false;}
		if (IsPartyLeader != _other.IsPartyLeader) { return false; }
		if (!PartyMembersID.SequenceEqual(_other.PartyMembersID)) { return false; }
		if (Team != _other.Team) { return false; }
		if (ClientID != _other.ClientID) { return false; }
		return true;
	}
	public override bool Equals(object obj)
	{
		if (obj is UserData _other)
		{
			return Equals(_other);
		}
		return false;
	}
	public override int GetHashCode()
	{
		int _hash = 17;

		// Handle PlayerName and other nullable types carefully
		_hash = _hash * 23 + (PlayerName?.GetHashCode() ?? 0);
		_hash = _hash * 23 + PlayerID.GetHashCode(); // Assuming PlayerID is a FixedString128Bytes

		_hash = _hash * 23 + PlayerIconID.GetHashCode();
		_hash = _hash * 23 + PlayerSkinID.GetHashCode();
		_hash = _hash * 23 + ChosenToolID.GetHashCode();

		// Compute hash for arrays manually (e.g., ChosenAbilitiesID)
		_hash = _hash * 23 + ChosenAbilitiesID.Aggregate(0, (acc, val) => acc * 23 + val.GetHashCode());
		_hash = _hash * 23 + ChosenElements.Aggregate(0, (acc, val) => acc * 23 + val.GetHashCode());

		// Handle booleans and nullable fields like Team
		_hash = _hash * 23 + IsPartyLeader.GetHashCode();
		_hash = _hash * 23 + PartyMembersID.Aggregate(0, (acc, val) => acc * 23 + val.GetHashCode());
		_hash = _hash * 23 + Team.GetHashCode();
		_hash = _hash * 23 + ClientID.GetHashCode();

		return _hash;
	}
}

public enum CloudSaveKeys

{
	PLAYER_ICON_ID,
	CHOSEN_SKIN_ID,
	CHOSEN_TOOL_ID,
	CHOSEN_ABILITIES_ID,
	IsKiwiAccount
}