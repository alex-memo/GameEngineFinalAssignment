using Cysharp.Threading.Tasks;
using Game.Managers.Camera;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// @alex-memo 2024
/// </summary>
public class EntityMovement : NetworkBehaviour
{

	#region Movement Variables
	private Vector3 velocity;
	public Vector3 Velocity { get => velocity; }
	private const float runAcceleration = 40f;
	private float deceleration => IsGrounded ? 6f : 2;
	private const float maxRunVelocity = 5;
	private float rotationSpeed;
	private const float jumpForce = 8;
	private const float gravity = 17f;
	/// <summary>
	/// Movement direction is the input direction from the player normalized
	/// </summary>
	private Vector3 movementDirection;
	#endregion

	private Transform cam;
	private Rigidbody rb;
	private readonly Vector3 boxSize = new(.5f, .1f, .5f);
	private LayerMask ignoreLayers;
	public bool IsGrounded => Time.time > lastJumpTime + .5f && Physics.CheckBox(transform.position, boxSize / 2, transform.rotation, ignoreLayers);
	private float lastJumpTime;

	private const float turnSmoothTime = .2f;
	private float turnSmoothVelocity;

	public OnJump OnJump;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, boxSize);
	}
	public override async void OnNetworkSpawn()
	{
		ignoreLayers = ~LayerMask.GetMask("Entity","Ignore Raycast", "Dead");
		transform.position = new Vector3(0, 1, 0);
		rb = GetComponent<Rigidbody>();

		if (!IsLocalPlayer) { return; }
		playerInput = GetComponent<PlayerInput>();
		if (IsOwner)
		{
			playerInput.enabled = true;
		}
		await UniTask.WaitUntil(() => CameraManager.Instance != null);
		cam = Camera.main.transform;
		Debug.Log(cam);
		SetInputActions();
	}
	#region Input
	private PlayerInput playerInput;
	private delegate void MoveInputEvent(Vector2 direction);
	private delegate void JumpInputEvent();
	private event MoveInputEvent OnMoveInput;
	private event JumpInputEvent OnJumpInput;
	private event OnCamTurned OnCamTurned;

	protected virtual void SetInputActions()
	{
		playerInput.actions["Move"].performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
		OnMoveInput += movement;
		playerInput.actions["Jump"].performed += ctx => OnJumpInput?.Invoke();
		OnJumpInput += Jump;
		playerInput.actions["Look"].performed += ctx => OnCamTurned?.Invoke(ctx);
		OnCamTurned += CameraManager.Instance.OnCamTurned;
	}
	protected virtual void RemoveInputActions()
	{
		if (!IsLocalPlayer) { return; }
		playerInput.actions["Move"].performed -= ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
		OnMoveInput -= movement;
		playerInput.actions["Jump"].performed -= ctx => OnJumpInput?.Invoke();
		OnJumpInput -= Jump;
		playerInput.actions["Look"].performed -= ctx => OnCamTurned?.Invoke(ctx);
		if (CameraManager.Instance != null)
		{
			OnCamTurned -= CameraManager.Instance.OnCamTurned;
		}

	}
	#endregion
	public override void OnDestroy()
	{
		base.OnDestroy();
		RemoveInputActions();
	}

	private void movement(Vector2 _movementDirection)
	{
		//print("Movement Direction: " + _movementDirection);
		movementDirection = new Vector3(_movementDirection.x, 0, _movementDirection.y);
		//moveServerRpc(movementDirection);
	}
	[ServerRpc]
	private void moveServerRpc(Vector3 _movementDirection, ServerRpcParams _params = default)
	{
		movementDirection = _movementDirection;
		//print("Server Movement Direction: " + movementDirection);
		//_params.Receive.SenderClientId
	}
	private void Jump()
	{
		if (!IsGrounded) { return; }
		if (!IsServer)
		{
			//jumpServerRpc();
		}
		OnJump?.Invoke();
		velocity.y = jumpForce;
		lastJumpTime = Time.time;
	}
	protected void KnockUp(float _force)
	{
		velocity.y = _force;
		lastJumpTime = Time.time;
		//velocity += transform.forward * 15;
	}
	[ServerRpc]
	private void jumpServerRpc(ServerRpcParams _params = default)
	{
		Jump();
	}

	private void FixedUpdate()
	{
		MoveEntity();
		if (!IsGrounded)
		{
			velocity.y -= gravity * Time.fixedDeltaTime;
			velocity.y = Mathf.Clamp(velocity.y, -20, 20);
		}
		else
		{
			velocity.y = 0;
		}
	}
	private void MoveEntity()
	{
		Vector3 _horizontalVelocity = new(velocity.x, 0, velocity.z);
		//Vector3 _moveDir = Vector3.zero;
		if (movementDirection.magnitude > 0)
		{
			float _targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
			float _angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref turnSmoothVelocity, turnSmoothTime);
			transform.rotation = Quaternion.Euler(0f, _angle, 0f);
			Vector3 _moveDir = Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward;

			Vector3 _frameAccel = runAcceleration * Time.fixedDeltaTime * _moveDir;

			if ((_horizontalVelocity + _frameAccel).magnitude < maxRunVelocity)
			{
				_horizontalVelocity += _frameAccel;
			}
			else
			{
				_horizontalVelocity = Vector3.Lerp(_horizontalVelocity, _moveDir * maxRunVelocity, deceleration * Time.fixedDeltaTime);
			}
		}
		else
		{
			if (_horizontalVelocity.magnitude > 0.1f)
			{
				_horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
			}
			else
			{
				_horizontalVelocity = Vector3.zero;
			}
		}
		velocity = new Vector3(_horizontalVelocity.x, velocity.y, _horizontalVelocity.z);
		rb.linearVelocity = velocity;
		//transform.position += velocity * Time.fixedDeltaTime;
	}
	private void OnCollisionStay(Collision _coll)
	{
		//if (!_coll.gameObject.CompareTag("Wall")) { return; }
		var _collPoint = _coll.GetContact(0).point;
		var _transformPos = transform.position;
		_collPoint.y = 0;
		_transformPos.y = 0;
		var _direction = _collPoint - _transformPos;
		//print(_direction);
		//print("The direction normalized: " + _direction.normalized + " The velocity normalized: " + velocity.normalized);
		//print(Vector3.Dot(_direction.normalized, velocity.normalized));
		if (Vector3.Dot(_direction.normalized, velocity.normalized) > 0.5f)
		{
			//print("walking to wall");
			velocity.x = 0;
			velocity.z = 0;
		}
	}
}
public delegate void OnJump();
public delegate void OnCast(int _abilityIndex);
public delegate void OnCamTurned(InputAction.CallbackContext _ctx);