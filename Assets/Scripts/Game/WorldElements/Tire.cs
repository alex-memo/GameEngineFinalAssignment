using UnityEngine;

public class Tire : MonoBehaviour
{
	[SerializeField] private float knockUpForce = 15;
	private void OnCollisionEnter(Collision _coll)
	{
		if (_coll.gameObject.TryGetComponent<EntityMovement>(out var _entity))
		{
			_entity.SendMessage("KnockUp", knockUpForce);
		}
	}
}
