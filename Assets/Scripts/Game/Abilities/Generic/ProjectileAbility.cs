using Game.Logic;
using Game.Managers.Camera;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class ProjectileAbility : Ability, ISpawnableProjectile
{
	[field: SerializeField] public GameObject SpawnablePrefab { get; private set; }
	[field: SerializeField] public float Duration { get; private set; }//time to destroy ability
	[field: SerializeField, Tooltip("Units per second")] public float Speed { get; private set; } = 8;
	[field: SerializeField] public bool DestroyOnHit { get; private set; } = true;
	[field: SerializeField] public float MaxImpactVfxLifetime { get; private set; } = 4;
	public override void OnCast()
	{
		base.OnCast();
		CameraManager.Instance.GetAbilitySpawnTransform(out var _spawnPos, out var _rotation, out var _distance);
		controller.SpawnProjectileServerRpc(true, index, _spawnPos, _rotation);
		if (NetworkManager.Singleton.IsServer) { return; }
		controller.SpawnProjectileLocal(true, index, _spawnPos, _rotation);
		//changeAnimation("ProjectileCast");
	}
	public float GetProjectileRange()
	{
		return Speed * Duration;
	}
#if UNITY_EDITOR
	protected virtual void OnValidate()
	{
		if (SpawnablePrefab != null)
		{
			var _vfxs = SpawnablePrefab.GetComponentsInChildren<VisualEffect>();
			foreach (var _vfx in _vfxs)
			{
				switch (_vfx.HasFloat("Duration"))
				{
					case true:
						_vfx.SetFloat("Duration", Duration);
						break;
					case false:
						Debug.LogWarning($"VFX {_vfx.name} does not have a Duration parameter");
						break;
				}
			}
		}
		//get impactvfx
		if (AbilityHitVFX != null)
		{
			if (AbilityHitVFX.TryGetComponent<VisualEffect>(out var _vfx))
			{
				switch (_vfx.HasFloat("MaxLifetime"))
				{
					case true:
						MaxImpactVfxLifetime = _vfx.GetFloat("MaxLifetime") + .25f;
						break;
					case false:
						MaxImpactVfxLifetime = 4;
						break;
				}
			}
		}
		if (Speed < 0)
		{
			Speed = 1;
		}
	}
#endif
}
