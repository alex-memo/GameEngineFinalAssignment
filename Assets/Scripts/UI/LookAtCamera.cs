using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private Transform mainCameraTransform;
	private void Awake()
	{
		mainCameraTransform = Camera.main.transform;
	}
	private void LateUpdate()
	{

		if (mainCameraTransform == null) { return; }

		//looks at camera plane
		Vector3 _cameraForward = mainCameraTransform.forward;
		Vector3 _cameraUp = mainCameraTransform.up;
		Vector3 _targetPosition = transform.position + _cameraForward;

		transform.LookAt(_targetPosition, _cameraUp);
	}
}
