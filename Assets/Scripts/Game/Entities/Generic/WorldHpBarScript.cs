using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// @alex-memo 2023
/// </summary>
public class WorldHpBarScript : HpBarFactory<IHealthBar>
{
	[ColorUsage(true, true), SerializeField] private Color enemyHpColor;
	[ColorUsage(true, true), SerializeField] private Color enemyHpBendayColour;
	[ColorUsage(true, true), SerializeField] private Color enemyShieldColour;
	[ColorUsage(true, true), SerializeField] private Color enemyShieldBendayColour;

	[ColorUsage(true, true), SerializeField] private Color allyHpColour;
	[ColorUsage(true, true), SerializeField] private Color allyHpBendayColour;
	[ColorUsage(true, true), SerializeField] private Color allyShieldColour;
	[ColorUsage(true, true), SerializeField] private Color allyShieldBendayColour;
	private bool hasSetColour;

	private TMP_Text healthText;

	public override void SetMaxHealth(float _oldMaxHp, float _maxHealth)
	{
		base.SetMaxHealth(_oldMaxHp, _maxHealth);
		//setHPText();
		if (!hasSetColour)
		{
			StartCoroutine(setHealthBarColor());
			hasSetColour = true;
		}
	}

	private void setHPText()
	{
		if (healthText == null) { return; }
		healthText.text = $"{currentHealth}/{maxHealth}";
	}
	public void SetHealthBarColour(UserData _oldData, UserData _newData)
	{
		//print($"{_team} has been asigned to {transform.parent.name} with tag {transform.parent.tag}");
		StartCoroutine(setHealthBarColor());
	}
	private IEnumerator setHealthBarColor()
	{
		//print(NetworkManager.Singleton.LocalClientId);
		yield return new WaitUntil(() => NetworkManager.Singleton.LocalClient.PlayerObject != null);//wait until has player
		var _localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<EntityController>();
		yield return new WaitUntil(() => _localPlayer.UserData.Value != null); //wait until player has team
		var _myStats = GetComponentInParent<EntityController>();
		//Debug.Log("Setting health bar color");
		if (_myStats.IsLocalPlayer) { yield break; }//player doesnt have world space hp bar

		var _allyTeam = _localPlayer.UserData.Value.Team.ToString();
		//print(_allyTeam);
		if(_myStats.CompareTag("Enemy"))//if explicitly enemy then dont even check
		{
			setEnemyColour();
			yield break;
		}
		if (_myStats.UserData.Value.Team.ToString().Equals(_allyTeam))
		{
			setMatColor("_HealthColour", allyHpColour);
			setMatColor("_ShieldColour", allyShieldColour);
			setMatColor("_HealthBendayColour", allyHpBendayColour);
			setMatColor("_ShieldBendayColour", allyShieldBendayColour);
			yield break;
		}//if not ally then we default to enemy
		setEnemyColour();
	}
	private void setEnemyColour()
	{
		setMatColor("_HealthColour", enemyHpColor);
		setMatColor("_ShieldColour", enemyShieldColour);
		setMatColor("_HealthBendayColour", enemyHpBendayColour);
		setMatColor("_ShieldBendayColour", enemyShieldBendayColour);
	}
}