using System;
using Game.Logic;
using Game.Managers.Camera;
using UnityEngine;
namespace RangedTools
{
	[CreateAssetMenu(fileName = "StapleGun", menuName = "Game/Tools/01_StapleGun")]

	public class StapleGun : HitscanTool
	{
		[Header("Alt Fire")]
		[SerializeField, Min(1)] private float adsDamage = 200f;
		[SerializeField] protected DamageFallOff adsFallOff;
		[SerializeField, Range(0, 1)] protected float adsAccuracy = 1f;
		private bool isAdsing = false;
		public override void SetTool(int _index, Entity _controller)
		{
			base.SetTool(_index, _controller);
			_controller.IsDead.OnValueChanged += (_previousValue, _newValue) =>
			{
				if (_newValue)
				{
					CameraManager.Instance.OnChangeAdsState(false);
				}
			};
		}
		public override void OnPrimaryAttack(bool _isPressed)
		{
			if (!_isPressed) { return; }
			if (!canShoot("Alt Fire")) { return; }
			switch (isAdsing)
			{
				case true:
					shoot(adsDamage, adsFallOff, adsAccuracy);
					break;
				case false:
					shoot(Damage, damageFallOff, accuracy);
					break;
			}
		}
		public override void OnSecondaryAttack(bool _isPressed)
		{
			isAdsing = _isPressed;
			CameraManager.Instance.OnChangeAdsState(_isPressed);
		}
	}
}