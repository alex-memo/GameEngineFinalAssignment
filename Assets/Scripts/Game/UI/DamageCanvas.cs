using System.Collections;
using UnityEngine;
/// <summary>
/// @alex-memo 2023
/// </summary>
public class DamageCanvas : MonoBehaviour
{
	private float damage;
	private TMPro.TMP_Text text;
	//private string adColour = "#FF4700";
	//private string apColour = "#00BCFF";
	//private string trueColour = "#FFFFFF";
	[SerializeField] private Color waterColour;
	[SerializeField] private Color earthColour;
	[SerializeField] private Color windColour;
	[SerializeField] private Color fireColour;
	[SerializeField] private Color plantColour;
	[SerializeField] private Color lightningColour;
	[SerializeField] private Color iceColour;
	[SerializeField] private Color physicalColour;
	[SerializeField] private Color voidColour;

	[SerializeField, Range(.1f, 3)] private float timeActive = 3;
	private float timer = 0.1f;

	[SerializeField] private Vector3 offset = new(1.5f, 1.5f, 0);
	private Transform parentTransform;

	private Transform mainCameraTransform;
	private float desiredScale = 1;
	private Coroutine damageAnimation;
	[SerializeField] private AnimationCurve animScaleCurve = new();
	private void Awake()
	{
		text = GetComponentInChildren<TMPro.TMP_Text>();
		//adjustPositionToAvoidOverlap();
		StartCoroutine(decolour());
		StartCoroutine(scaleToScreen());
		StartCoroutine(maintainOffset());
		StartCoroutine(counter());
		mainCameraTransform = Camera.main.transform;
		parentTransform = transform.parent;
	}
	public void SetDamage(float _damage, Element _element)
	{
		damage = _damage;
		text.text = ((int)damage).ToString();

		setDesiredScale(damage);
		setElementColour(_element);
		damageAnimation ??= StartCoroutine(animateDamage());
	}
	public void AddDamage(float _damage, Element _element)
	{
		damage += _damage;
		timer = 0;
		text.text = ((int)damage).ToString();
		setDesiredScale(damage);
		setElementColour(_element);
		damageAnimation ??= StartCoroutine(animateDamage());
	}
	private void setElementColour(Element _element)
	{
		switch (_element)
		{
			case Element.Water:
				text.color = waterColour;
				break;
			case Element.Earth:
				text.color = earthColour;
				break;
			case Element.Wind:
				text.color = windColour;
				break;
			case Element.Fire:
				text.color = fireColour;
				break;
			case Element.Plant:
				text.color = plantColour;
				break;
			case Element.Lightning:
				text.color = lightningColour;
				break;
			case Element.Ice:
				text.color = iceColour;
				break;
			case Element.Physical:
				text.color = physicalColour;
				break;
			case Element.Void:
				text.color = voidColour;
				break;
		}
	}

	private void setDesiredScale(float _damage)
	{
		if (damageAnimation != null) { return; }
		float _clampedDamage = Mathf.Clamp(_damage, 0, 1000);
		desiredScale = Mathf.Lerp(.1f, .2f, Mathf.InverseLerp(0, 1000, _clampedDamage));
	}
	private IEnumerator scaleToScreen()
	{
		while (timer < timeActive)
		{
			if (mainCameraTransform == null) { yield return new WaitForEndOfFrame(); continue; }
			float _cameraDistance = Vector3.Distance(transform.position, mainCameraTransform.position);
			float _screenScale = desiredScale * _cameraDistance;
			transform.SetScale(_screenScale);
			yield return new WaitForEndOfFrame();
		}
	}
	private IEnumerator maintainOffset()
	{
		while (timer < timeActive)
		{
			if (parentTransform == null || Camera.main == null)
			{
				yield return new WaitForEndOfFrame();
				continue;
			}

			// Get the vector pointing to the right of the camera's view
			Vector3 cameraRight = Camera.main.transform.right * offset.x;
			Vector3 cameraUp = Camera.main.transform.up * offset.y;

			// Calculate the new position relative to the camera's right and up vectors
			Vector3 newPosition = parentTransform.position + cameraRight + cameraUp;

			// Update the position of the child object
			transform.position = newPosition;

			yield return new WaitForEndOfFrame();
		}
	}
	private IEnumerator animateDamage()
	{
		var _initScale = desiredScale;
		float _timer = 0;
		const float _animDuration = .15f;
		float _maxAnimScale = _initScale * 1.5f;
		while (_timer < _animDuration)
		{
			//move through curve
			float _scale = animScaleCurve.Evaluate(_timer / _animDuration);
			//scale from curve (0-1) to 0-maxanimScale
			_scale = Mathf.Lerp(_initScale, _maxAnimScale, _scale);
			desiredScale = _scale;
			_timer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		desiredScale = _initScale;
		damageAnimation = null;


	}
	private IEnumerator decolour()
	{
		while (timer < timeActive)
		{
			var _currColour = text.color;
			if (timer / timeActive < .8)
			{
				_currColour.a = 1;
			}
			else
			{
				_currColour.a = Mathf.Lerp(1f, 0f, (timer / timeActive - 0.8f) / .2f);
			}
			text.color = _currColour;
			yield return new WaitForEndOfFrame();
		}
	}
	private IEnumerator counter()
	{
		while (timer < timeActive)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
	}
}