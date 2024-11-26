using UnityEngine;
using UnityEngine.UI;

namespace Game.Managers
{
	public class MapiconSetup : MonoBehaviour
	{
		[SerializeField] private Transform visionConeHolder;
		[SerializeField] private RawImage coneImage;
		[SerializeField] private RawImage playerIcon;

		public void SetupIcon(Color _color, Texture _playerIcon)
		{
			coneImage.color = _color;
			playerIcon.texture = _playerIcon;
		}
		public Transform GetVisionConeHolder()
		{
			return visionConeHolder;
		}
	}
}
