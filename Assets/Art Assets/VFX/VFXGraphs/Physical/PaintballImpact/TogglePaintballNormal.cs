using UnityEngine;
using UnityEngine.VFX;

public class TogglePaintballNormal : MonoBehaviour
{
	[SerializeField] private VisualEffect paintBallImpact;
	private bool hasNormal;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			hasNormal = !hasNormal;
			paintBallImpact.SetBool("HasNormal", hasNormal);
		}
	}
}
