using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Game.Logic;
using Game.Core;

namespace Game.Managers
{
	public class HUDPlayerIcon : UIIcon
	{
		[SerializeField] private Transform Icon;
		[SerializeField] private Transform HpBar;
		private Material hpMaterialInstance;
		public async void SetPlayer(Entity _playerController)
		{
			UserData _userData = _playerController.UserData.Value;
			await UniTask.WaitUntil(() => MultiplayerGameManager.Instance.LocalController != null);
			//check if ally
			SetAttribute("_MainTexture", StaticListManager.Instance.GetIcon(_userData.PlayerIconID));
			if (MultiplayerGameManager.Instance.LocalController.GetEnemyTeam() == _userData.Team)
			{
				return;
			}
			var _hpBar = GetComponent<HpBarFactory<IHealthBar>>();
			_playerController.CurrentHealth.OnValueChanged += _hpBar.SetHealth;
			_playerController.Shield.OnValueChanged += _hpBar.SetShield;
			_hpBar.SetMaxHealth(0, Entity.MaxHp);
			_hpBar.SetHealth(0, _playerController.CurrentHealth.Value);
			_hpBar.SetShield(0, _playerController.Shield.Value);
		}
		protected override void createMaterialInstance()
		{
			materialInstance = new Material(Icon.GetComponent<Image>().material);
			Icon.GetComponent<Image>().material = materialInstance;
			hpMaterialInstance = new Material(HpBar.GetComponent<Image>().material);
			HpBar.GetComponent<Image>().material = hpMaterialInstance;
		}
	}
}