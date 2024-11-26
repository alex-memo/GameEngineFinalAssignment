using UnityEngine;

namespace MainMenu.Loadout
{
	//[ExecuteInEditMode] // Allows the script to run in edit mode for testing
	public class CameraRenderControl : MonoBehaviour
	{
		[SerializeField] private Camera depthCamera;
		[SerializeField] private Material depthMaterial;

		private RenderTexture depthTexture;

		private void OnEnable()
		{
			// Ensure RenderTexture is created on enable
			createDepthTexture();
			assignDepthTexture();
		}

		private void Update()
		{
			if (depthTexture == null || depthTexture.width != Screen.width || depthTexture.height != Screen.height)
			{
				// Recreate RenderTexture if the screen size changes or if it's null
				createDepthTexture();
			}

			assignDepthTexture();
		}

		private void OnDisable()
		{
			// Clean up the RenderTexture when the script is disabled
			if (depthTexture != null)
			{
				depthTexture.Release();
				depthTexture = null;
			}
		}

		private void createDepthTexture()
		{
			// Release the old texture if it exists
			if (depthTexture != null)
			{
				depthTexture.Release();
			}

			// Create a new RenderTexture
			depthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
			depthCamera.targetTexture = depthTexture;
			depthCamera.depthTextureMode = DepthTextureMode.DepthNormals;
		}

		private void assignDepthTexture()
		{
			// Assign the depth texture to the material
			if (depthMaterial != null)
			{
				depthMaterial.SetTexture("_DepthTex", depthTexture);
			}
		}
	}
}