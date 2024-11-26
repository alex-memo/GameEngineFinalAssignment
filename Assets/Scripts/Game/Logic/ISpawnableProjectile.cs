using UnityEngine;

namespace Game.Logic
{
	public interface ISpawnableProjectile : ISpawnable
	{
		public float Damage { get; }
		public Element Element { get; }
		public float Duration { get; }
		public float Speed { get; }
		public bool DestroyOnHit { get; }
		public GameObject AbilityHitVFX { get; }
	}
}
