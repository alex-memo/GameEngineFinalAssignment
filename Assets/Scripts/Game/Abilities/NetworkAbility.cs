using System.Collections;
using Game.Logic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// @alex-memo 2023
/// This class is responsible for 
/// </summary>
public abstract class NetworkAbility : MonoBehaviour, INetReplicableGO
{
	public ISpawnableProjectile Projectile { get; private set; }
	public Entity OwnerController { get; protected set; }

	protected bool hasCustomDamage;
	protected float customDamage;
	protected Element customDamageType;
	protected float maxImpactVfxLifetime = 4;
	protected int abilityId;
	/// <summary>
	/// This method sets the ownerID of the ability, and sets the stats and ability of the object.
	/// </summary>
	/// <param name="_ownerID">NetworkObjectId of the owner</param>
	/// <param name="_damageType">The damage type of the ability</param>
	/// <param name="_abilityNumber">The number of ability casted</param>
	public void Set(ulong _ownerID, ISpawnable _ability, int _id)
	{
		//set stats for the object
		if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(_ownerID, out var _owner))
		{
			OwnerController = _owner.GetComponent<Entity>();
			if (OwnerController.IsDead.Value)
			{
				print("Owner is dead");
				DestroyAbility();
			}
		}
		Projectile = (ISpawnableProjectile)_ability;
		abilityId = _id;
		abilitySpawned();
		if (Projectile.Duration == 0) { return; }
		StartCoroutine(destroyAbilityCoroutine());
	}
	protected virtual IEnumerator destroyAbilityCoroutine()
	{
		if (Projectile.Duration == 0) { yield break; }

		yield return new WaitForSeconds(Projectile.Duration);
		DestroyAbility();
	}
	protected void dealDamage(Collider _coll, float _damage, Element _damageType)
	{
		if (!NetworkManager.Singleton.IsServer) { return; }
		if (_coll == null) { return; }
		if (!_coll.TryGetComponent<IDamageable>(out var _controller)) { return; }
		if (!((NetworkBehaviour)_controller).IsSpawned) { return; }
		_controller.ServerTakeDamage(
			_damage,
			_damageType,
			OwnerController.UserData.Value,
			new DamageSource(_damage, _damageType, false, abilityId));

		dealtDamage?.Invoke(gameObject, _coll.transform, _damage, _damageType);
	}
	public virtual void DestroyAbility()
	{
		destroyChildVfx();
		StopAllCoroutines();
		Destroy(gameObject);
	}
	private void destroyChildVfx()
	{
		//has to be client sided
		if (transform.childCount == 0) { return; }
		foreach (Transform _child in transform)
		{
			_child.parent = null;
			var _vfx = _child.GetComponent<VisualEffect>();
			var _destroyTime = _vfx.GetFloat("ParticleLifetime") + .25f;
			_vfx.Stop();
			Destroy(_child.gameObject, _destroyTime);
		}
	}
	protected void spawnImpactVFX()
	{
		if (Projectile.AbilityHitVFX == null) { return; }
		var _hit = Instantiate(Projectile.AbilityHitVFX, transform.position, Quaternion.identity, null);
		Destroy(_hit, maxImpactVfxLifetime);
	}

	/// <summary>
	/// Serves as onnetworkspawn for abilities
	/// </summary>
	protected abstract void abilitySpawned();

	public DealtDamage dealtDamage;//i mean this only exists on server so should be fine
	public HitEnemy hitEnemy;
}
public delegate void DealtDamage(GameObject _projectile, Transform _target, float _damage, Element _damageType);
public delegate void HitEnemy(NetworkAbility _projectile, Transform _target);

