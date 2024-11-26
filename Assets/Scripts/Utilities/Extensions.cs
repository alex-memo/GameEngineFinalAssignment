using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
/// <summary>
/// @alex-memo 2023
/// </summary>
public static class Extensions
{
	public static void SetScale(this Transform _transform, float _scale)
	{
		_transform.localScale = new Vector3(_scale, _scale, _scale);
	}
	public static void SetHexColour(this ref Color _colour, string _hexColour)
	{
		if (ColorUtility.TryParseHtmlString(_hexColour, out Color _tempCol))
		{
			_colour = _tempCol;
		}
	}

	/// <summary>
	/// Clamps the vector to the min and max values
	/// </summary>
	/// <param name="_vector"></param>
	/// <param name="_min">The minimum value</param>
	/// <param name="_max">The maximum value</param>
	/// <returns></returns>
	public static Vector3 Clamp(this Vector3 _vector, float _min, float _max)
	{
		_vector.x = Mathf.Clamp(_vector.x, _min, _max);
		_vector.y = Mathf.Clamp(_vector.y, _min, _max);
		_vector.z = Mathf.Clamp(_vector.z, _min, _max);
		return _vector;
	}
	/// <summary>
	/// This method returns the first child of the transform that contains the given name
	/// </summary>
	/// <param name="_transform"></param>
	/// <param name="_name"></param>
	/// <returns></returns>
	public static Transform FindContains(this Transform _transform, string _name)
	{
		if (_transform == null) { return null; }
		return _transform.Find(_name);
	}
	/// <summary>
	/// Returns true if any of the tags are attached to the gameobject
	/// </summary>
	/// <param name="_gameobject"></param>
	/// <param name="_tags">Tags to compare object tag to</param>
	/// <returns></returns>
	public static bool CompareTag(this Collider _gameobject, params string[] _tags)
	{
		if (_gameobject == null) { return false; }
		foreach (string _tag in _tags)
		{
			if (string.IsNullOrEmpty(_tag)) { continue; }
			if (_gameobject.CompareTag(_tag))
			{
				return true;
			}
		}
		return false;
	}
	/// <summary>
	/// Returns true if the object layer is the same as the sent one
	/// </summary>
	/// <param name="_gameobject"></param>
	/// <param name="_layer">Layer to compare object layer to</param>
	/// <returns></returns>
	public static bool CompareLayer(this GameObject _gameobject, string _layer)
	{
		if (string.IsNullOrEmpty(_layer)) { return false; }
		if (_gameobject.layer == LayerMask.NameToLayer(_layer)) { return true; }
		return false;
	}

	public static bool Contains(this string _string, params string[] _contains)
	{
		foreach (string _contain in _contains)
		{
			if (_string.Contains(_contain))
			{
				return true;
			}
		}
		return false;
	}
	public static bool Equals<T>(this T _object, params T[] _comparisons)
	{
		foreach (T _otherObj in _comparisons)
		{
			if (_object.Equals(_otherObj))
			{
				return true;
			}
		}
		return false;
	}
	public static string ToString<T>(this IEnumerable<T> _list, bool _braces = true, bool _objSpacing = true, char _spacingChar = '\n')
	{
		System.Text.StringBuilder _sb = new();
		if (_braces) { _sb.Append("{\n"); }
		int _count = _list.Count();
		for (int i = 0; i < _count; ++i)
		{
			T _item = _list.ElementAt(i);
			_sb.Append(_item.ToString());
			if (_objSpacing && i < _count - 1) { _sb.Append(_spacingChar); }
		}
		if (_braces) { _sb.Append("}"); }
		return _sb.ToString();
	}
	public static bool IsNullOrEmpty(this string _string)
	{
		return string.IsNullOrEmpty(_string);
	}
	/// <summary>
	/// Returns a value weighted towards the peak value
	/// Ex. min = 0, max = 100, peak = 100, will return 67 on average
	/// </summary>
	/// <param name="_value"></param>
	/// <param name="_min"></param>
	/// <param name="_peak"></param>
	/// <param name="_max"></param>
	/// <returns></returns>
	public static void TriangularDistribution(this ref float _value, float _min = 0f, float _peak = 0.5f, float _max = 1f)
	{
		float _rand = Random.value;
		if (_rand < (_peak - _min) / (_max - _min))
		{
			_value = _min + Mathf.Sqrt(_rand * (_max - _min) * (_peak - _min));
		}
		else
		{
			_value = _max - Mathf.Sqrt((1f - _rand) * (_max - _min) * (_max - _peak));
		}
	}
	/// <summary>
	/// Sorts the Raycast array by distance in an ascending order
	/// Will throw an exception if the array contains nulls
	/// </summary>
	/// <param name="_raycasts">The raycast array to sort</param>
	/// <param name="_inverse">Reverse the array to get a descending sort</param>
	public static void SortRaycasts(this RaycastHit[] _raycasts, bool _inverse = false)
	{
		System.Array.Sort(_raycasts, (_prev, _next) => _prev.distance.CompareTo(_next.distance));
		if (_inverse)
		{
			System.Array.Reverse(_raycasts);
		}
	}
	/// <summary>
	/// Returns the array without any null values
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="_arr"></param>
	/// <returns></returns>
	public static T[] RemoveArrayNulls<T>(this T[] _arr)
	{
		return _arr.Where(x => x != null).ToArray();
	}
	public static RaycastHit[] RemoveArrayNulls(this RaycastHit[] _arr)
	{
		return _arr.Where(x => x.collider != null).ToArray();
	}
	public static void SetAttribute<T>(this Material _materialInstance, string _attribute, T _value)
	{
		if (_value is float _floatValue)//why must types not work with switch
		{
			_materialInstance.SetFloat(_attribute, _floatValue);
		}
		else if (_value is Texture _textureValue)
		{
			_materialInstance.SetTexture(_attribute, _textureValue);
		}
		else if (_value is Color _colorValue)
		{
			_materialInstance.SetColor(_attribute, _colorValue);
		}
		else if (_value is bool _boolValue)
		{
			_materialInstance.SetFloat(_attribute, System.Convert.ToSingle(_boolValue));
		}
		else
		{
			if(_value == null)
			{
				Debug.LogWarning("Value is null");
				return;
			}
			Debug.LogWarning("Unsupported type: " + _value.GetType());
		}

	}
	#region UIExtensions
	/// <summary>
	/// @memo 2024
	/// Fades a canvas group to the desired alpha in the desired time
	/// </summary>
	/// <param name="_canvas"></param>
	/// <returns></returns>
	public static async UniTask Fade(this CanvasGroup _canvas, float _desiredAlpha, float _lerpTime = 1, CancellationToken _cts = default)
	{
		if (_canvas == null) { return; }
		_desiredAlpha = Mathf.Clamp(_desiredAlpha, 0, 1);

		_canvas.gameObject.SetActive(true);
		float _startAlpha = _canvas.alpha;
		if (_startAlpha == _desiredAlpha) { return; }
		float _time = 0;
		while (_time < _lerpTime)
		{
			if (_canvas == null) { return; }
			_canvas.alpha = Mathf.Lerp(_startAlpha, _desiredAlpha, _time / _lerpTime);
			_time += Time.deltaTime;
			await Awaitable.EndOfFrameAsync(_cts);
		}
		_canvas.alpha = _desiredAlpha;
		_canvas.gameObject.SetActive(_desiredAlpha > 0);
	}

	#endregion
}
