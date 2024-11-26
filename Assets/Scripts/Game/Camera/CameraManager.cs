using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Memo.Utilities.Game;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// @alex-memo 2023
/// </summary>
namespace Game.Managers.Camera
{
	[DefaultExecutionOrder(1000)]
	public class CameraManager : MonoBehaviour, IInputAxisOwner
	{
		[SerializeField] private Material deadMat;
		[SerializeField, Header("Camera Components")] private CinemachineBrain cam;
		[SerializeField] private CinemachineVirtualCameraBase FreeCam;
		[SerializeField] private CinemachineVirtualCameraBase AimCam;
		public Transform MainCameraTransform { get; private set; }
		public static CameraManager Instance;
		public bool HasAimAssist { get; private set; } = false;
		public RaycastHit[] RaycasterHits { get; private set; }
		[SerializeField] private InputActionReference playerLook;
		private float controllerSens = 200;
		[SerializeField] private float kbmSens = 0.1f;

		[field: SerializeField] public Vector2 VerticalClamp { get; private set; } = new(-10, 45);//in degrees
		[field: SerializeField] public Vector2 HorizontalClamp { get; private set; } = new(-60, 30);
		public InputAxis Horizontal = new() { Range = new Vector2(-180, 180), Wrap = true, Recentering = InputAxis.RecenteringSettings.Default };
		public InputAxis Vertical = new() { Range = new Vector2(-89, 90), Wrap = false, Recentering = InputAxis.RecenteringSettings.Default };
		[SerializeField] private Transform target;
		private Transform playerTransform;
		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
#if !UNITY_SERVER || UNITY_EDITOR
			MainCameraTransform = UnityEngine.Camera.main.transform;
#endif

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		void OnValidate()
		{
			Horizontal.Validate();
			Vertical.Range.x = Mathf.Clamp(Vertical.Range.x, -90, 90);
			Vertical.Range.y = Mathf.Clamp(Vertical.Range.y, -90, 90);
			Vertical.Validate();
		}
		private void OnDestroy()
		{
			Instance = null;
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
		}
		void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> _axes)
		{
			_axes.Add(new() { DrivenAxis = () => ref Horizontal, Name = "Horizontal Look", Hint = IInputAxisOwner.AxisDescriptor.Hints.X });
			_axes.Add(new() { DrivenAxis = () => ref Vertical, Name = "Vertical Input", Hint = IInputAxisOwner.AxisDescriptor.Hints.Y });
		}
		public void OnCamTurned(InputAction.CallbackContext _ctx)
		{
			//why c# why, please let me use a switch for comparing types
			if (_ctx.control.device is Gamepad)
			{
				HasAimAssist = true;
				return;
			}
			HasAimAssist = false;
			playerLook.action.ApplyParameterOverride("ScaleVector2:x", kbmSens);
			playerLook.action.ApplyParameterOverride("ScaleVector2:y", kbmSens);
		}
		public void SetTarget(Transform _t)
		{
			FreeCam.Follow = target;
			AimCam.Follow = target;
			playerTransform = _t.parent;
			if (!playerTransform.TryGetComponent<Entity>(out var _player)) { return; }
			initialCameraRotation(_player);
		}
		private async void initialCameraRotation(Entity _player)
		{
			await UniTask.WaitUntil(() => _player.UserData.Value.Team.Length != 0);
			if (_player.UserData.Value.Team.ToString().Contains("TeamA"))
			{
				Horizontal.Value = -180;
				return;
			}
			Horizontal.Value = 0;
		}
		public void DeathCamera()
		{
			//deadMat.profile.TryGet(out ColorAdjustments _colourSaturation);
			//_colourSaturation.saturation.value = -100;
		}
		public void RespawnCamera()
		{
			//deadMat.profile.TryGet(out ColorAdjustments _colourSaturation);
			//_colourSaturation.saturation.value = 0;
		}
		private void Update()
		{
			#if !UNITY_SERVER || UNITY_EDITOR
			cameraRaycaster();

			if (HasAimAssist)
			{
				aimAssist();
			}
			if (playerTransform == null) { return; }
			SetRotation();
			target.position = playerTransform.position + Vector3.up * 2;
			#endif
		}
		private void cameraRaycaster()
		{
#if !UNITY_SERVER || UNITY_EDITOR
			RaycasterHits = Physics.RaycastAll(MainCameraTransform.position, MainCameraTransform.forward, 100f, ~LayerMask.GetMask("Dead"));
			RaycasterHits.SortRaycasts();
#endif
		}
		public Vector3 GetCameraLookAt()
		{
			return getCameraLookAt();
		}
		/// <summary>
		/// We have to clamp the value to 
		/// </summary>
		/// <returns></returns>
		private Vector3 getCameraLookAt()
		{
#if !UNITY_SERVER || UNITY_EDITOR
			// Check if the MultiplayerGameManager instance and the LocalPlayer are not null
			if (MultiplayerGameManager.Instance == null || MultiplayerGameManager.Instance.LocalController == null)
			{
				// Return the camera's look-at point if the player is not available
				return MainCameraTransform.position + MainCameraTransform.forward * 10f;
			}

			// Get the player's transform
			Transform _playerTransform = MultiplayerGameManager.Instance.LocalController.transform;

			// Calculate the direction from the player to where the camera is looking
			Vector3 _cameraLookAtPoint = MainCameraTransform.position + MainCameraTransform.forward * 10f;
			Vector3 _lookAtDirection = _cameraLookAtPoint - _playerTransform.position;
			float _distanceToLookAtPoint = _lookAtDirection.magnitude;
			_lookAtDirection.Normalize();

			// Get the player's forward and right vectors
			Vector3 _playerForward = _playerTransform.forward;
			Vector3 _playerRight = _playerTransform.right;

			// Calculate the horizontal angle between the player's forward vector and the look-at direction
			float _horizontalAngle = Vector3.SignedAngle(_playerForward, new Vector3(_lookAtDirection.x, 0, _lookAtDirection.z), Vector3.up);

			// Calculate the vertical angle between the look-at direction and the horizontal plane
			float _verticalAngle = Mathf.Asin(_lookAtDirection.y) * Mathf.Rad2Deg;

			// Clamp the horizontal and vertical angles within the specified thresholds
			float _clampedHorizontalAngle = Mathf.Clamp(_horizontalAngle, HorizontalClamp.x, HorizontalClamp.y);
			float _clampedVerticalAngle = Mathf.Clamp(_verticalAngle, VerticalClamp.x, VerticalClamp.y);

			// Apply the clamped rotations to the player's forward vector
			Quaternion _horizontalRotation = Quaternion.AngleAxis(_clampedHorizontalAngle, Vector3.up);
			Quaternion _verticalRotation = Quaternion.AngleAxis(-_clampedVerticalAngle, _playerRight);

			Vector3 _finalDirection = _verticalRotation * _horizontalRotation * _playerForward;

			// Calculate the clamped look-at position
			Vector3 _lookAtPosition = _playerTransform.position + _finalDirection * _distanceToLookAtPoint;

			return _lookAtPosition;
#endif
#if UNITY_SERVER && !UNITY_EDITOR
			return Vector3.zero;
#endif
		}

		/// <summary>
		/// Returns the unintersected hit with bloom applied
		/// </summary>
		/// <param name="_accuracy"></param>
		/// <param name="_distance"></param>
		/// <returns></returns>
		public RaycastHit GetBloomHit(float _accuracy, out float _distance)
		{
#if !UNITY_SERVER || UNITY_EDITOR
			var _playerPos = MultiplayerGameManager.Instance.LocalController.transform.position;
			_playerPos.y += 1.5f;
			const float _deviation = 0.12f;

			float _bloomRangeX = (1 - _accuracy) * Random.Range(-_deviation, _deviation); //Deviation (_deviation% of the screen range based on accuracy)
			float _bloomRangeY = (1 - _accuracy) * Random.Range(-_deviation, _deviation);

			Vector3 _bloomOffset = (MainCameraTransform.right * _bloomRangeX) + (MainCameraTransform.up * _bloomRangeY);
			Vector3 _shootingDirection = (MainCameraTransform.forward + _bloomOffset).normalized;

			RaycastHit[] _hits = new RaycastHit[10];
			Physics.RaycastNonAlloc(MainCameraTransform.position + _bloomOffset, _shootingDirection, _hits, 100f, ~LayerMask.GetMask("Dead"));
			Debug.DrawRay(MainCameraTransform.position + _bloomOffset, _shootingDirection * 100, Color.green, 10f);

			RaycastHit _cameraHit = getClosestNonTeammateHit(_hits, out _distance);
			if (_cameraHit.collider == null) { return new(); }

			hitSameTarget(ref _cameraHit, ref _distance, ref _playerPos);
			return _cameraHit;
#endif
#if UNITY_SERVER && !UNITY_EDITOR
			_distance = 0;
			return new();
#endif
		}
		private void hitSameTarget(ref RaycastHit _cameraHit, ref float _distance, ref Vector3 _playerPos)
		{
			RaycastHit[] _hits = new RaycastHit[10];
			Physics.RaycastNonAlloc(_playerPos, _cameraHit.point - _playerPos, _hits, 100f, ~LayerMask.GetMask("Dead"));
			RaycastHit _playerHit = getClosestNonTeammateHit(_hits, out _distance);
			if (_playerHit.collider == null) { return; }
			_distance = _playerHit.distance;

			if (_playerHit.transform == _cameraHit.transform) { return; }
			_cameraHit = _playerHit;
		}
		/// <summary>
		/// Returns the closest hit that is not a teammate
		/// </summary>
		/// <param name="_distance"></param>
		/// <param name="_point"></param>
		/// <returns></returns>
		private RaycastHit getClosestNonTeammateHit(RaycastHit[] _hits, out float _distance)
		{
			_distance = float.MaxValue;
			_hits = _hits.RemoveArrayNulls();
			_hits.SortRaycasts();
			if (_hits.Length == 0) { return new(); }
			for (int _i = 0; _i < _hits.Length; ++_i)
			{
				if (_hits[_i].transform.CompareTag(MultiplayerGameManager.Instance.LocalController.tag)) { continue; }//if they have same tag then keep looking
				{
					_distance = _hits[_i].distance;
					return _hits[_i];
				}
			}
			return new();
		}
		private void aimAssist()
		{
			RaycastHit? _hit = getClosestNonTeammateHit(RaycasterHits, out var _distance);
			Debug.Log(_hit);
			if (_hit == null) { return; }

			if (!_hit.Value.collider.IsEnemy(MultiplayerGameManager.Instance.LocalController))
			{
				playerLook.action.ApplyParameterOverride("ScaleVector2:x", -controllerSens);
				playerLook.action.ApplyParameterOverride("ScaleVector2:y", controllerSens);
				return;
			}
			float _aimAssistStrength = Mathf.Lerp(.4f, .01f, Mathf.InverseLerp(0, 60, _distance));
			Debug.Log($"Aim Assist Strength: {_aimAssistStrength}");
			playerLook.action.ApplyParameterOverride("ScaleVector2:x", -controllerSens * .4f);
			playerLook.action.ApplyParameterOverride("ScaleVector2:y", controllerSens * .4f);
		}
		private void SetRotation()
		{
			if (target == null) { return; }
			target.rotation = Quaternion.Euler(Vertical.Value, Horizontal.Value, 0);
		}
		public void OnChangeAdsState(bool _isAds)
		{
			FreeCam.Priority = _isAds ? 0 : 1;
			AimCam.Priority = _isAds ? 1 : 0;
		}
		public RaycastHit GetAbilitySpawnTransform(out Vector3 _spawnPosition, out Quaternion _rotation, out float _distance, float _accuracy = 1)
		{
			_spawnPosition = playerTransform.position + playerTransform.forward / 10;
			_spawnPosition.y += 1.5f;

			var _camForward = UnityEngine.Camera.main.transform.forward;
			var _camPos = UnityEngine.Camera.main.transform.position;

			Vector3 _direction;
			RaycastHit _hit = GetBloomHit(_accuracy, out _distance);//Instance.GetClosestHit(out var _);
			if (_hit.collider != null)
			{
				_direction = _hit.point - _spawnPosition;
			}
			else
			{
				_direction = (_camPos + _camForward * 100f) - _spawnPosition;
			}
			_rotation = Quaternion.LookRotation(_direction);
			return _hit;
		}
#if !UNITY_SERVER || UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (MainCameraTransform == null) { return; }
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(MainCameraTransform.position, MainCameraTransform.position + MainCameraTransform.forward * 100);
			if (MultiplayerGameManager.Instance == null) { return; }
			if (MultiplayerGameManager.Instance.LocalController == null) { return; }
			Gizmos.color = Color.red;
			var _playerPos = MultiplayerGameManager.Instance.LocalController.transform.position;
			_playerPos.y += 1.5f;
			if (RaycasterHits.Length > 0)
			{
				Gizmos.DrawLine(_playerPos, RaycasterHits[0].point);
			}

			Gizmos.color = Color.cyan;
			var _lookAt = getCameraLookAt();
			Gizmos.DrawLine(_playerPos, _lookAt);
		}
#endif
	}
}