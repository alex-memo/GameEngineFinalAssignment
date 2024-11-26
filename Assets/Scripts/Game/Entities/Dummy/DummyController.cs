using System.Collections;
using Game.Logic;
using UnityEngine;

public class DummyController : PlayerController
{
	private const float timeForHPReplenish = 6f;
	private float timeSinceDamageTaken;
	private Coroutine timer;
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		CurrentHealth.OnValueChanged += onHealthChanged;
	}
	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		CurrentHealth.OnValueChanged -= onHealthChanged;
	}
	private void onHealthChanged(float _old, float _new)
	{
		if (!IsServer) { return; }
		if (_new == MaxHp) { return; }
		timeSinceDamageTaken = 0;
		timer ??= StartCoroutine(replenishHealth());
	}
	private IEnumerator replenishHealth()
	{
		if (IsDead.Value) { yield break; }
		while (timeSinceDamageTaken < timeForHPReplenish)
		{
			yield return new WaitForSeconds(1);
			++timeSinceDamageTaken;
		}
		if (CurrentHealth.Value <= 0)
		{
			//RespawnServerRpc();
		}
		HealToMaxHP();
		timer = null;
	}

}
