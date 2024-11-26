using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialCycle : MonoBehaviour
{
	[SerializeField] List<Material> shaderMats = new();
	private int matNumber;

	private void Start()
	{
		gameObject.GetComponent<Renderer>().material = shaderMats[0];
		StartCoroutine(CycleMat());
	}

	private IEnumerator CycleMat()
	{
		yield return new WaitForSecondsRealtime(2);
		matNumber++;
		matNumber %= shaderMats.Count;
		gameObject.GetComponent<Renderer>().material = shaderMats[matNumber];
		StartCoroutine(CycleMat());
	}
}
