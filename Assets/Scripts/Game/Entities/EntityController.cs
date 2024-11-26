using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Game.Logic;
using Game.Managers;
using Game.Core;
/// <summary>
/// @alex-memo 2024
/// </summary>
public class EntityController : Entity, IDamageable
{
	protected DamageGraph damageGraph = new();
	protected List<DamageGraph> damageGraphs = new();
	public WorldHpBarScript HealthBar { get; private set; }
	public override async void OnNetworkSpawn()
	{
		if (!IsLocalPlayer)
		{
			instantiateHealthBar();
		}
		//StartCoroutine(setInitialStats());
		UserData.OnValueChanged += OnUserDataChanged;
		if (IsServer)
		{
			await UniTask.WaitUntil(() => MultiplayerGameManager.Instance != null);
			MultiplayerGameManager.Instance.AddEntity(this);
			//SetCurrentHealthServerRpc(EntityStats.GetMaxHealth);
		}
	}
	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		UserData.OnValueChanged -= OnUserDataChanged;
		if (IsServer)
		{
			MultiplayerGameManager.Instance.RemoveEntity(this);
		}
	}
	protected virtual void instantiateHealthBar()
	{
#if !UNITY_SERVER || UNITY_EDITOR
		//if(!IsLocalPlayer){return;}
		HealthBar = Instantiate(Resources.Load<GameObject>("HealthBar"), transform).GetComponent<WorldHpBarScript>();

		CurrentHealth.OnValueChanged += HealthBar.SetHealth;
		Shield.OnValueChanged += HealthBar.SetShield;
		UserData.OnValueChanged += HealthBar.SetHealthBarColour;

		HealthBar.SetMaxHealth(0, MaxHp);
		HealthBar.SetHealth(0, CurrentHealth.Value);
		//print($"HealthBar instantiated for {name}");
#endif
	}
	/// <summary>
	/// should be triggered in server and just replicate in clients, so abilities and autos that
	/// hit player in server just replicate in clients
	/// melee users just trigger this method so those are not in server require ownership false
	/// </summary>
	/// <param name="_damage"></param>
	/// <param name="_element"></param>
	[ServerRpc(RequireOwnership = false)]
	public void TakeDamageServerRpc(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource)
	{
		takeDamage(_damage, _element, _dmgDealerData, _damageSource);
	}
	public override void ServerTakeDamage(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource)
	{
		takeDamage(_damage, _element, _dmgDealerData, _damageSource);
	}
	protected void takeDamage(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource)
	{
		if (IsDead.Value) { return; }
		if (_damage < 0) { _damage = 0; }
		float _dmgToShield = 0;
		float _dmgToHp = 0;
		if (Shield.Value > 0)
		{
			if (Shield.Value >= _damage)
			{//shield can absorb all incoming damage
				_dmgToShield = _damage;
				Shield.Value -= _damage;
				_damage = 0f; // No damage left to apply to health
			}
			else
			{//cracked shield
				_dmgToShield = Shield.Value;
				_damage -= Shield.Value;
				Shield.Value = 0f;
			}
		}
		if (_damage > 0f)
		{
			if (CurrentHealth.Value >= _damage)
			{//entity can take the damage
				_dmgToHp = _damage;
				CurrentHealth.Value -= _damage;
			}
			else
			{//entity cant take the damage, will die
				_dmgToHp = CurrentHealth.Value;
				CurrentHealth.Value = 0f;
			}
		}
		float _actualDmgTaken = _dmgToShield + _dmgToHp;
		damageGraph.AddDamage(_actualDmgTaken, _element, _dmgDealerData);
		if (_dmgDealerData.ChosenToolID >= -500)
		{
			_damageSource.Damage = _actualDmgTaken;
			//MultiplayerGameManager.Instance.AnalyticsHolder.AddDamageSource(_dmgDealerData.ClientID, _damageSource);
		}

		if (CurrentHealth.Value <= 0)
		{
			CurrentHealth.Value = 0;
			secureKill(ref _dmgDealerData);
			Die(_dmgDealerData);
		}

		InstantiateDamageCanvasClientRpc(_actualDmgTaken, _element);
	}
	/// <summary>
	/// if the entity dies from external source from player,
	/// if they took damage in the last 15 seconds
	/// then we replace the damage dealer for the last damage taken
	/// </summary>
	private void secureKill(ref UserData _dmgDealerData)
	{
		if (_dmgDealerData.ChosenToolID > -500) { return; }
		if (Time.time - damageGraph.lastDamageTakenTime > 15) { return; }
		if (damageGraph.lastDamageDealer == null) { return; }
		_dmgDealerData = damageGraph.lastDamageDealer;
	}
	/// <summary>
	/// Die Called from server
	/// </summary>
	/// <param name="_assasinData"></param>
	protected virtual void Die(UserData _assasinData)
	{
		if (!IsServer) { return; }//here we should still be in server all the time but still just in case
		print(damageGraph.ToString());
		damageGraphs.Add(damageGraph);
		damageGraph = new();
		IsDead.Value = true;
		if (!IsHost)
		{
			deadColliders();
		}

		DieClientRpc();
		//if (IsOwnedByServer && !IsLocalPlayer) { NetworkObject.Despawn(); }
		//enable the line above if we want to despawn the entity when killed
	}
	/// <summary>
	/// Runs on the killed client
	/// </summary>
	/// <param name="_params"></param>

	[ClientRpc]
	protected void DieClientRpc()
	{
		ClientDie();
	}
	private void deadColliders()
	{
		if (TryGetComponent<Collider>(out var _collider))
		{
			_collider.enabled = false;
		}
		gameObject.layer = LayerMask.NameToLayer("Dead");
	}
	protected virtual void ClientDie()
	{
		//runs on every client so visuals are synched
		//we should play death animation and disable the entity
		Debug.Log("Die ClientRpc");
		if (IsLocalPlayer)
		{//things to do when player dies if its the local player
		 //if anim is client based then here death anim
		}
		deadColliders();
		foreach (Transform _child in transform)
		{
			if (_child.name.Contains("DamageCanvas")) { continue; }
			_child.gameObject.SetActive(false);
		}
		//play death sound here
	}
	public override List<DamageGraph> ExtractDamageGraphs()
	{
		if (!IsServer) { return null; }
		var _tempGraphs = new List<DamageGraph>(damageGraphs)
		{
			damageGraph
		};
		return _tempGraphs;
	}
	public override void ServerRespawn()
	{
		IsDead.Value = false;
		//HealToMaxHP();
		string _json = @"{""Health"":""350"",""Shield"":""750""}";
		var _jObj = Newtonsoft.Json.Linq.JObject.Parse(_json);
		float _health = (float)_jObj["Health"];
		float _shield = (float)_jObj["Shield"];
		Heal(_health);
		addShieldServer(_shield).Forget();
		if (!IsHost)
		{
			respawnColliders();
		}
		RespawnVisualsClientRpc();
	}
	[ServerRpc]
	public void RespawnServerRpc()
	{
		ServerRespawn();
	}
	[ClientRpc]
	protected virtual void RespawnVisualsClientRpc()
	{
		//runs on every client so visuals are synched
		respawnColliders();
		foreach (Transform _child in transform)
		{
			_child.gameObject.SetActive(true);
		}
	}
	private void respawnColliders()
	{
		if (TryGetComponent<Collider>(out var _collider))
		{
			_collider.enabled = true;
		}
		gameObject.layer = LayerMask.NameToLayer("Entity");
	}

	[ClientRpc]
	private void InstantiateDamageCanvasClientRpc(float _damage, Element _damageType)
	{
		if (IsLocalPlayer) { return; }
		DamageCanvas _damageCanvas = GetComponentInChildren<DamageCanvas>();
		if (_damageCanvas != null)
		{
			_damageCanvas.AddDamage(_damage, _damageType);
			return;
		}
		GameObject _canvas = Instantiate(Resources.Load<GameObject>("DamageCanvas"), transform);

		_canvas.GetComponent<DamageCanvas>().SetDamage(_damage, _damageType);
	}

	#region Health and Shield Utilities Region
	public void HealToMaxHP()
	{
		SetCurrentHealth(MaxHp);
	}
	public void Heal(float _hp)
	{
		float _newHealth = CurrentHealth.Value + _hp;
		SetCurrentHealth(_newHealth);
	}
	public void HealOrShield(float _hp)
	{
		if (CurrentHealth.Value < MaxHp)
		{
			var _healthOverflow = CurrentHealth.Value + _hp - MaxHp;
			if (_healthOverflow > 0)
			{
				Heal(MaxHp - CurrentHealth.Value);
				addShieldServer(_healthOverflow).Forget();
				return;
			}
			Heal(_hp);
			return;
		}
		addShieldServer(_hp).Forget();
	}
	public void Syphon()
	{
		if (!IsServer) { return; }
		if (CurrentHealth.Value <= 0) { return; }
		float _value = 500;
		HealOrShield(_value);
	}

	protected void SetCurrentHealth(float _health)
	{
		var _newHealth = Mathf.Clamp(_health, 0, MaxHp);
		CurrentHealth.Value = _newHealth;
	}
	[ServerRpc]
	private void AddShieldServerRpc(float _shield)
	{
		addShieldServer(_shield).Forget();
	}
	private async UniTaskVoid addShieldServer(float _shield)
	{
		if (!IsServer) { return; }
		Shield.Value += _shield;
		//all shields decay over 3 seconds
		const float _decayTime = 3;
		const float _tickInterval = .15f;
		float _tickDecayValue = _shield * _tickInterval / _decayTime;
		float _timer = 0;
		while (_timer < _decayTime)
		{
			await UniTask.WaitForSeconds(_tickInterval);
			if (Shield.Value <= 0) { break; }
			Shield.Value -= _tickDecayValue;

			_timer += _tickInterval;
		}
		if (Shield.Value < 0)
		{
			Shield.Value = 0;
		}
	}

	#endregion
	private void OnUserDataChanged(UserData _previousValue, UserData _newValue)
	{
		//print(name + "team number changed from " + previousValue + " to " + newValue);
		tag = _newValue.Team.ToString();
	}
	public override void ServerSpawnProjectile(bool _isAbility, int _id, Vector3 _spawnPos, Quaternion _rotation, RpcParams _params = default)
	{
		SpawnProjectileLocal(_isAbility, _id, _spawnPos, _rotation);

		spawnProjectileClientRpc(_isAbility, _id, _spawnPos, _rotation, RpcTarget.Not(
			new ulong[]
			{
				_params.Receive.SenderClientId, //we dont want to send to the client that sent the rpc
				NetworkManager.Singleton.LocalClientId //we dont want to send to the server cuz alr happened here(server)
			},
			RpcTargetUse.Temp));
	}
	[Rpc(SendTo.SpecifiedInParams)]
	private void spawnProjectileClientRpc(bool _isAbility, int _id, Vector3 _spawnPos, Quaternion _rotation, RpcParams _params = default)
	{
		SpawnProjectileLocal(_isAbility, _id, _spawnPos, _rotation);
	}
	public override void SpawnProjectileLocal(bool _isAbility, int _id, Vector3 _spawnPos, Quaternion _rotation)
	{
		//Debug.Log("SpawnLocalProjectile");
		ISpawnable _projectile = _isAbility ?
		StaticListManager.Instance.GetAbility(_id) as ISpawnable :
		StaticListManager.Instance.GetToolScript(_id) as ISpawnable;
		GameObject _obj = _projectile.SpawnablePrefab;
		if (_obj == null) { return; }
		GameObject _go = Instantiate(_obj, _spawnPos, _rotation, null);
		_go.GetComponent<INetReplicableGO>().Set(NetworkObjectId, _projectile, _id);
	}
}