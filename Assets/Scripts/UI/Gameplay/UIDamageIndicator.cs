using Cysharp.Threading.Tasks;
using UnityEngine;
namespace UI
{
	public class UIDamageIndicator : MonoBehaviour
	{
		[SerializeField] private RectTransform[] arrows;
		[SerializeField] private AnimationCurve animScaleCurve;
		[SerializeField] private float animDuration = .15f;
		private const float maxAnimScale = 1.5f;
		[SerializeField] private float yOffset = -15;
		private CanvasGroup canvasGroup;
		private bool isRunning;
		private Vector2[] originalPositions;
		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			originalPositions = new Vector2[arrows.Length];
			for (int i = 0; i < arrows.Length; i++)
			{
				originalPositions[i] = arrows[i].anchoredPosition;
			}
			canvasGroup.alpha = 0;
		}

		public void DealDamage()
		{
			//move back on transform local y
			if (isRunning) { return; }
			animateArrows().Forget();
			canvasGroup.alpha = 1;
			canvasGroup.Fade(0, animDuration).Forget();
		}
		/// <summary>
		/// Scales arrows and moves them back
		/// </summary>
		/// <returns></returns>
		private async UniTask animateArrows()
		{
			isRunning = true;
			float _timer = 0;
			while (_timer < animDuration)
			{
				float _curvePos = animScaleCurve.Evaluate(_timer / animDuration);
				float _scale = Mathf.Lerp(1, maxAnimScale, _curvePos);
				float _yOffset = Mathf.Lerp(0, yOffset, _curvePos);
				for(int i = 0; i < arrows.Length; ++i)
				{
					arrows[i].SetScale(_scale);
					Vector2 _dir = arrows[i].up;
					Vector2 _newArrowPos = originalPositions[i] + _dir * _yOffset;
					arrows[i].anchoredPosition =_newArrowPos;
				}
				_timer += Time.deltaTime;
				await UniTask.Yield();
			}
			resetPositions();
			isRunning = false;
		}
		private void resetPositions()
		{
			for (int i = 0; i < arrows.Length; i++)
			{
				arrows[i].anchoredPosition = originalPositions[i];
			}
		}
	}
}
