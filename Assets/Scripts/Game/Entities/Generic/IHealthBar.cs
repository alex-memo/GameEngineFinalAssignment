using UnityEngine;

public interface IHealthBar
{
	void SetMaxHealth(float _oldMaxHp, float _maxHealth);
	void SetHealth(float _oldCurrentHp, float _currentHp);
}
