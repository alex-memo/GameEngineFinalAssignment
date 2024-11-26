using UnityEngine;
using UnityEngine.UI;

public class HpBarFactory<T> : MonoBehaviour where T : IHealthBar
{
	protected Material healthBarMaterial;
	protected float maxHealth;
	protected float currentHealth;
	protected virtual void Awake()
	{
		Image _image = GetComponentInChildren<Image>();

		var _tempMat = _image.material;
		healthBarMaterial = new Material(_tempMat);
		_image.material = healthBarMaterial;
		setMatFloat("_ShieldPercentage", 0);
		setMatFloat("_HealthPercentage", 1);
	}
	public virtual void SetMaxHealth(float _oldMaxHp, float _maxHealth)
	{
		maxHealth = _maxHealth;
	}
	public void SetHealth(float _oldCurrentHp, float _currentHp)
	{
		if (_currentHp > maxHealth)
		{//we check that max health is not bigger in server so should be fine
			SetMaxHealth(0, _currentHp);//this is spaghetti code but should not backfire
		}//pls dont backfire
		currentHealth = _currentHp;
		setMatFloat("_HealthPercentage", _currentHp / maxHealth);
		//setHPText();
	}
	public void SetShield(float _oldShield, float _shield)
	{
		//Debug.Log($"maxHealth: {maxHealth}, _shield: {_shield}");
		setMatFloat("_ShieldPercentage", _shield / maxHealth);
	}
	protected void setMatFloat(string _property, float _percentage)
	{
		//Debug.Log($"Setting {_property} to {_percentage}");
		healthBarMaterial.SetFloat(_property, _percentage);
	}
	protected void setMatColor(string _property, Color _color)
	{
		healthBarMaterial.SetColor(_property, _color);
	}
}
