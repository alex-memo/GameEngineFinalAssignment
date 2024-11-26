using Game.Logic;
using Memo.Utilities.Game;
using Unity.Netcode;
using UnityEngine;

public class NetworkProjectile : NetworkAbility
{
	[Header("REFERENCES")]
	protected Rigidbody rb;

	protected override void abilitySpawned()
	{
		//throw new System.NotImplementedException();
		rb = GetComponent<Rigidbody>();
		rb.linearVelocity = transform.forward * Projectile.Speed;
	}
	protected virtual void OnTriggerEnter(Collider _coll)
	{
		if (_coll.gameObject.CompareTag("Ability"))
		{
			spawnImpactVFX();
			//DestroyAbility();
			return;
		}
		if (_coll.gameObject.CompareTag("Untagged"))
		{
			if (_coll.TryGetComponent(out IDamageable _damageable))
			{
				hitEntity(ref _coll);
				return;
			}
			spawnImpactVFX();
			if (Projectile.DestroyOnHit)
			{
				DestroyAbility();
			}
		}
		if (_coll.gameObject.CompareTag("Wall") || _coll.gameObject.CompareLayer("Ground"))
		{
			spawnImpactVFX();
			DestroyAbility();
			return;
		}
		//Debug.Log($"I hit {_coll.name} my owner is {OwnerController} the collider is enemy: {_coll.IsEnemy(OwnerController)}");
		if (didIHitEnemy(ref _coll)) { return; }
	}
	protected virtual bool didIHitEnemy(ref Collider _coll)
	{
		if (OwnerController == null) { return false; }
		if (!_coll.IsEnemy(OwnerController)) { return false; }
		//if enemy team or explicitly enemy
		hitEnemy?.Invoke(this, _coll.transform);
		hitEntity(ref _coll);
		return true;
	}
	private void hitEntity(ref Collider _coll)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			//Debug.Log($"Server hit {_coll.name}");
			specialInteractionsOnHit(_coll);
			if (hasCustomDamage)
			{
				dealDamage(_coll, customDamage, customDamageType);
			}
			else
			{
				dealDamage(_coll, Projectile.Damage, Projectile.Element);
			}
		}

		spawnImpactVFX();
		if (Projectile.DestroyOnHit)
		{
			DestroyAbility();
		}
	}
	protected virtual void specialInteractionsOnHit(Collider _coll)
	{

	}
}
