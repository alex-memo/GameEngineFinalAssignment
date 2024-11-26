using UnityEngine;
namespace Game.Barrels
{
	[CreateAssetMenu(fileName = "SyphonBarrel", menuName = "Game/Barrels/SyphonBarrel")]
	public class SyphonBarrel : BarrelCommand
	{
		[SerializeField] protected float syphonAmount;
		[SerializeField] protected float explosionRadius = 5;
		public override void Execute()
		{
			Collider[] _colliders = new Collider[10];
			int _numColliders = Physics.OverlapSphereNonAlloc
			(barrel.transform.position, explosionRadius, _colliders,
			1 << LayerMask.NameToLayer("Entity")); 
			for (int i = 0; i < _numColliders; ++i)
			{
				if (_colliders[i] == null) { continue; }
				if (!_colliders[i].TryGetComponent(out EntityController _controller)) { continue; }
				GrantEffect(_controller);
			}
		}
		protected virtual void GrantEffect(EntityController _controller)
		{
			_controller.HealOrShield(syphonAmount);
		}
	}
}