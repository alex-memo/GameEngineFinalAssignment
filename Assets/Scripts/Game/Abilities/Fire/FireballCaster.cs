using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Game/Abilities/00_Fireball")]
public class FireballCaster : ProjectileAbility
{
	private float burnDamageOverTime;
	public override void OnCast()
	{
		base.OnCast();
		//changeAnimation("FireballCast");
	}
}
