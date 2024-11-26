using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Game.Logic
{
	public abstract class Entity : NetworkBehaviour
	{
		public NetworkVariable<UserData> UserData = new();
		public NetworkVariable<float> CurrentHealth { get; protected set; } = new(2000);
		public static float MaxHp => 2000;
		public NetworkVariable<float> Shield { get; protected set; } = new(0);
		public NetworkVariable<bool> IsDead { get; protected set; } = new(false);
		public NetworkVariable<float> TotalDamageDealt { get; set; } = new(0);
		public string GetEnemyTeam()
		{
			if (UserData.Value == null) { return "Enemy"; }
			var _team = UserData.Value.Team.ToString();//  TeamNumber.Value.ToString();

			return _team switch
			{
				"TeamA" => "TeamB",
				"TeamB" => "TeamA",
				_ => "Enemy",
			};
		}
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			IsDead.OnValueChanged = null;
		}
		public abstract void ServerRespawn();
		public abstract void ServerTakeDamage(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource);
		public abstract List<DamageGraph> ExtractDamageGraphs();
		[Rpc(SendTo.Server)]
		public void SpawnProjectileServerRpc(bool _isAbility, int _index, Vector3 _spawnPos, Quaternion _rotation, RpcParams _params = default)
		{
			ServerSpawnProjectile(_isAbility, _index, _spawnPos, _rotation, _params);
		}
		public abstract void ServerSpawnProjectile(bool _isAbility, int _index, Vector3 _spawnPos, Quaternion _rotation, RpcParams _params = default);
		public abstract void SpawnProjectileLocal(bool _isAbility, int _index, Vector3 _spawnPos, Quaternion _rotation);
	}
}
