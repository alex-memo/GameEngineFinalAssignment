using Unity.Netcode;
using UnityEngine;
namespace Game.Logic
{
	public interface IDamageable
	{
		public NetworkVariable<float> CurrentHealth { get; }
		static float MaxHp { get; }
		public NetworkVariable<float> Shield { get; }

		[ServerRpc]
		public void TakeDamageServerRpc(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource);
		public void ServerTakeDamage(float _damage, Element _element, UserData _dmgDealerData, DamageSource _damageSource);
	}
}