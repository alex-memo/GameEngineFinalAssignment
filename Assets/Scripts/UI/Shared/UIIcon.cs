using System;
using UnityEngine;
using UnityEngine.UI;

public class UIIcon : MonoBehaviour
{
	protected Material materialInstance;
	protected int index;
	protected virtual void Awake()
	{
		createMaterialInstance();
	}
	/// <summary>
	/// creates an instance of the material so that the original material is not changed
	/// </summary>
	protected virtual void createMaterialInstance()
	{
		materialInstance = new Material(GetComponent<Image>().material);
		GetComponent<Image>().material = materialInstance;
	}
	public virtual void SetIcon(Texture _icon, int _index = 0)
	{
		materialInstance.SetTexture("_MainTexture", _icon);
		index = _index;
	}
	public void SetAttribute<T>(string _attribute, T _value)
	{
		if (_value is float floatValue)//why must types not work with switch
		{
			materialInstance.SetFloat(_attribute, floatValue);
		}
		else if (_value is Texture textureValue)
		{
			materialInstance.SetTexture(_attribute, textureValue);
		}
		else if (_value is Color colorValue)
		{
			materialInstance.SetColor(_attribute, colorValue);
		}
		else if (_value is bool boolValue)
		{
			materialInstance.SetFloat(_attribute, Convert.ToSingle(boolValue));
			if (_value.Equals(true))
			{
				//Debug.Log("True");
				materialInstance.GetFloat(_attribute);
			}
		}
		else
		{
			Debug.LogWarning("Unsupported type: " + _value.GetType());
		}

	}
}
