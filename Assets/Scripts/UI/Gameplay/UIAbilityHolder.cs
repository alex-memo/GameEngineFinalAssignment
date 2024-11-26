using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UIAbilityHolder : MonoBehaviour
	{
		private ICastable castable;
		[SerializeField] private Image icon;
		[SerializeField] private Image grayedOutIcon;
		[SerializeField] private TMP_Text cooldownText;
		private void Awake()
		{
#if !UNITY_SERVER || UNITY_EDITOR
			createMaterialInstances();
#endif
		}
		private void createMaterialInstances()
		{
			icon.material = new(icon.material);
			grayedOutIcon.material = new(grayedOutIcon.material);
			//set outlines to white
			icon.material.SetAttribute("_Outlined", true);
			grayedOutIcon.material.SetAttribute("_Outlined", true);
			icon.material.SetAttribute("_OutlineColour", Color.white);
			grayedOutIcon.material.SetAttribute("_OutlineColour", Color.white);
			icon.material.SetAttribute("_GrayOut", 1f);
			grayedOutIcon.material.SetAttribute("_GrayOut", 0f);
		}
		public void Set(ICastable _castable)
		{
#if !UNITY_SERVER || UNITY_EDITOR
			if (_castable == null) { return; }
			castable = _castable;
			icon.material.SetAttribute("_MainTexture", ((IInventoryItem)castable).Icon);
			grayedOutIcon.material.SetAttribute("_MainTexture", ((IInventoryItem)castable).Icon);
			castable.ActiveCooldown.OnValueChanged += OnCooldownChanged;
#endif
		}
		private void OnCooldownChanged(float _currentCooldown)
		{
#if !UNITY_SERVER || UNITY_EDITOR
			float _cd = (1 - (_currentCooldown / castable.Cooldown)) * castable.Cooldown;
			bool _isCooldown = _cd > 0.05f;
			if (cooldownText == null || icon == null) { return; }
			cooldownText.text = _isCooldown ? _cd.ToString("0.0") : string.Empty;
			icon.fillAmount = _isCooldown ? _currentCooldown / castable.Cooldown : 1;
#endif
		}
	}
}
