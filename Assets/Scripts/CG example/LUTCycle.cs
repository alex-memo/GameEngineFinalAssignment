using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//im not proud of putting the playercontroller on an image

public class LUTCycle : MonoBehaviour
{
	public PlayerInput playerInput;
	public ColorGrading ColorGrading;
	[SerializeField] List<Material> LUTS;
	private int LUTIndex;
	void Start()
	{
		GetComponent<Image>().material = LUTS[0];
		ColorGrading.lutMaterial = LUTS[0];

		playerInput.actions["SwapLUT"].performed += _ => SwapLut();
	}

	void SwapLut()
	{
		LUTIndex++;
		LUTIndex %= LUTS.Count;
		if (LUTIndex < 3)
		{
			LUTS[3].SetFloat("_Contribution", 0.0f);
			GetComponent<Image>().material = LUTS[LUTIndex];
			ColorGrading.lutMaterial = LUTS[LUTIndex];
		}
		else//when at 4
		{
			GetComponent<Image>().material = LUTS[0];
			ColorGrading.lutMaterial = LUTS[0];
			LUTS[3].SetFloat("_Contribution", 0.6f);
		}
	}
}
