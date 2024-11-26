using System.Collections.Generic;
using Game.Managers.Camera;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	private EntityMovement movement;
	private Vector3 velocity => movement.Velocity;
	private bool isGrounded => movement.IsGrounded;
	private Vector2 horizontalVelocity => new(velocity.x, velocity.z);

	private Animator animator;
	private bool isLocalPlayer;
	/// <summary>
	/// int layer of the animation playing, string the name of the animation 
	/// </summary>
	private readonly Dictionary<int, string> currentAnimations = new()
	{
		{0,"EMPTY"},
		{1,"EMPTY"}
	};
	private void Awake()
	{
		movement = GetComponent<EntityMovement>();
		animator = GetComponent<Animator>();
		if (movement == null) { return; }
		movement.OnJump += Jump;
		isLocalPlayer = movement.IsLocalPlayer;
	}
	private void OnAnimatorIK(int _layerIndex)
	{
		if (!isLocalPlayer) { return; }
		if (_layerIndex == 0)
		{
			if (CameraManager.Instance == null) { return; }
			animator.SetLookAtPosition(CameraManager.Instance.GetCameraLookAt());
			animator.SetLookAtWeight(1, .5f, .5f);
			//animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
			//animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
			//animator.SetIKPosition(AvatarIKGoal.LeftFoot, movement.LeftFootPosition);
			//animator.SetIKPosition(AvatarIKGoal.RightFoot, movement.RightFootPosition);
		}
	}
	private void LateUpdate()
	{
		if (movement == null) { return; }
		if (isGrounded)
		{
			animator.SetFloat("Velocity", horizontalVelocity.magnitude);
		}
		animator.SetBool("IsGrounded", isGrounded);
		// if (isGrounded && horizontalVelocity.magnitude > 0)
		// {
		// 	changeAnimation("Run");
		// }
		// else if (isGrounded)
		// {
		// 	changeAnimation("Idle");
		// }
		// endAnimation();
	}
	private void Jump()
	{
		bool _mirror = Random.value < 0.5f;
		changeAnimation("JumpStatic", 0, .1f, _mirror);
	}
	private void changeAnimation(string _animation, int _layer = 0, float _crossFade = .1f, bool _mirror = false)
	{
		//if (currentAnimations[_layer] == _animation) { return; }
		animator.CrossFade(_animation, _crossFade, _layer);
		animator.SetBool($"Mirror{_layer}", _mirror);
		currentAnimations[_layer] = _animation;
	}
	public void ChangeAnimation(AnimationData _data)
	{
		changeAnimation(_data.AnimationName, _data.Layer, _data.CrossFade);
	}
	private void endAnimation(int _layer = 1)
	{
		AnimatorStateInfo _state = animator.GetCurrentAnimatorStateInfo(_layer);
		if (_state.IsName("EMPTY")) { return; }
		if (_state.normalizedTime >= 1f && !animator.IsInTransition(_layer))
		{
			changeAnimation("EMPTY", _layer);
		}
	}
}
[System.Serializable]
public struct AnimationData
{
	public string AnimationName;
	public int Layer;
	public float CrossFade;
	public AnimationData(string _animationName, int _layer = 0, float _crossFade = .1f)
	{
		AnimationName = _animationName;
		Layer = _layer;
		CrossFade = _crossFade;
	}
}