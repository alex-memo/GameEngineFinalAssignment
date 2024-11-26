using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Managers
{
	public class OrthoCamCastToTexture : MonoBehaviour
	{
#if !UNITY_SERVER || UNITY_EDITOR
		private async void Start()
		{
			var _cam = GetComponent<UnityEngine.Camera>();
			_cam.enabled = false;
			await UniTask.Delay(100);
			_cam.Render();
		}
#endif
	}
}
