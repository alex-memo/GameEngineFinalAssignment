using Game.Logic;
using Game.Managers;
using Game.Managers.Camera;
using UnityEngine;
namespace RangedTools
{
	public abstract class HitscanTool : RangeTool
	{
		protected override void altFire()
		{

		}

		protected override void shoot(float _damage, DamageFallOff _falloff, float _accuracy, bool _noBulletTime = false)
		{
			--currentAmmo.Value;
			Transform _hit = CameraManager.Instance.GetBloomHit(_accuracy, out var _distance).transform;

			if (_hit == null) { return; }
			//spawn hit vfx
			if (!_noBulletTime) { lastBulletTime = Time.time; }
			if (!_hit.TryGetComponent(out IDamageable _damageable)) { return; }
			var _falloffDamage = _falloff.GetFallOffDamage(_distance, _damage);
			Debug.Log($"Dealing {_falloffDamage} damage to {_damageable}");
			HUDManager.Instance.DealDamage();
			_damageable.TakeDamageServerRpc(_falloffDamage, Element, controller.UserData.Value, new DamageSource(_damage, Element, true, index));
		}
	}
}