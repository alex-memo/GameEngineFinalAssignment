using UnityEngine;

namespace UI
{
	public class UIAmmoCount : MonoBehaviour
	{
		private int maxAmmo;
		[SerializeField] private TMPro.TMP_Text ammoCountText;
		public void SetMaxAmmo(int _maxAmmo)
		{
			maxAmmo = _maxAmmo;
			ammoCountText.text = $"{maxAmmo}/{maxAmmo}";
		}
		public void SetAmmo(int _currentAmmo)
		{
			ammoCountText.text = $"{_currentAmmo}/{maxAmmo}";
		}
	}
}
