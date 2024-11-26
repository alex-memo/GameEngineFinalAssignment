using UnityEngine;

namespace Game.Logic
{
	public interface ISpawnable
	{
		[SerializeField] public GameObject SpawnablePrefab { get; }
	}
}
