using System.Collections.Generic;
using Game.Managers.Camera;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class VFXSpawner : NetworkBehaviour
{
	private EntityController myController;
	private void Awake()
	{
		myController = GetComponent<EntityController>();
	}
	/// <summary>
	/// Spawns the ability VFX at the given position or on the player
	/// Spawning on animation is preferred
	/// Useful mostly for abilities that need a final destination or the player has to move
	/// </summary>
	/// <param name="_abilityID"></param>
	/// <param name="_position"></param>
	[ServerRpc]
	protected void spawnVFXServerRpc(int _abilityID, Vector3 _position, Quaternion _rot, float _destroyTime = 2)
	{
		spawnVFXClientRpc(_abilityID, _position, _rot, _destroyTime);
	}
	/// <summary>
	/// Must only be called from spawnVFXServerRpc
	/// </summary>
	/// <param name="_abilityID"></param>
	/// <param name="_position"></param>
	[ClientRpc]
	private void spawnVFXClientRpc(int _abilityID, Vector3 _position, Quaternion _rot, float _destroyTime)
	{
		var _abilityVfx = new GameObject();//Instantiate(GetAbility(_abilityID).AbilityVFX, _position, _rot, null);
		Destroy(_abilityVfx, _destroyTime);
	}
	/// <summary>
	/// Preferred method, spawn them with the animation on the code
	/// </summary>
	/// <param name="_abilityID"></param>
	/// <param name="_position"></param>
	protected void spawnVfx(int _abilityID, Vector3 _position, float _destroyTime = 2)
	{
		var _abilityVfx = new GameObject();//Instantiate(GetAbility(_abilityID).AbilityVFX, _position, Quaternion.identity, null);
		Destroy(_abilityVfx, _destroyTime);
	}
	/// <summary>
	/// Used to spawn under the transform of the player
	/// </summary>
	/// <param name="_path"></param>
	/// <param name="_destroyTime"></param>
	[ServerRpc(RequireOwnership = false)]
	public void SpawnVFXServerRpc(FixedString64Bytes _path, float _destroyTime = 2)
	{
		spawnVFXClientRpc(_path, _destroyTime);
	}
	[ServerRpc]
	protected void SpawnVFXServerRpc(FixedString64Bytes _path, Vector3 _position, Quaternion _rotation, float _destroyTime = 2)
	{
		spawnVFXClientRpc(_path, _position, _rotation, _destroyTime);
	}

	private Dictionary<int, GameObject> storedTransformVFX = new();
	/// <summary>
	/// Spawns the vfx under this transform
	/// </summary>
	[ServerRpc]
	protected void spawnTransformVFXServerRpc(int _abilityNo, float _destroyTime = 2)
	{
		spawnTransformVFXClientRpc(_abilityNo, _destroyTime);
	}
	[ServerRpc]
	protected void destroyTransformVFXServerRpc(int _abilityNo)
	{
		destroyTransformVFXClientRpc(_abilityNo);
	}
	[ClientRpc]
	private void destroyTransformVFXClientRpc(int _abilityNo)
	{
		storedTransformVFX.TryGetValue(_abilityNo, out var _go);
		if (_go == null) { return; }
		Debug.Log("Destroying VFX: " + _abilityNo);
		Destroy(_go);
	}
	/// <summary>
	/// Spawns the vfx under this transform in the client
	/// </summary>
	/// <param name="_abilityNo"></param>
	/// <param name="_destroyTime"></param>
	[ClientRpc]
	private void spawnTransformVFXClientRpc(int _abilityNo, float _destroyTime)
	{
		var _abilityVfx = new GameObject();//Instantiate(GetAbility(_abilityNo).AbilityVFX, transform);
		if (_destroyTime < 0) //destroy is handled elsewhere
		{
			//check if it's already in the list
			if (storedTransformVFX.ContainsKey(_abilityNo)) { Debug.Log("There is already this element on the list!"); return; }
			storedTransformVFX.Add(_abilityNo, _abilityVfx);//in case we need to manage stacks later
			return;
		}
		Destroy(_abilityVfx, _destroyTime);
	}
	[ClientRpc]
	private void spawnVFXClientRpc(FixedString64Bytes _path, Vector3 _pos, Quaternion _rot, float _destroyTime)
	{
		var _abilityVfx = Instantiate(Resources.Load<GameObject>(_path.ToString()), _pos, _rot, null);
		if (_destroyTime < 0) { _destroyTime = 2; }
		Destroy(_abilityVfx, _destroyTime);
	}
	private Dictionary<FixedString64Bytes, GameObject> storedVFX = new();
	[ClientRpc]
	private void spawnVFXClientRpc(FixedString64Bytes _path, float _destroyTime)
	{
		var _abilityVfx = Instantiate(Resources.Load<GameObject>(_path.ToString()), transform);
		if (_destroyTime < 0) //destroy is handled elsewhere
		{
			//check if it's already in the list
			if (storedVFX.ContainsKey(_path)) { Debug.Log("There is already this element on the list!"); return; }
			storedVFX.Add(_path, _abilityVfx);//in case we need to manage stacks later
			return;
		}
		Destroy(_abilityVfx, _destroyTime);
	}
	[ServerRpc(RequireOwnership = false)]
	public void DestroyVFXServerRpc(FixedString64Bytes _path)
	{
		DestroyVFXClientRpc(_path);
	}
	[ClientRpc]
	private void DestroyVFXClientRpc(FixedString64Bytes _path)
	{
		print("Destroying VFX: " + _path.ToString());
		storedVFX.TryGetValue(_path, out var _go);
		storedVFX.Remove(_path);
		//var _go = transform.FindContains(_path.ToString()).gameObject;
		if (_go == null) { return; }
		Destroy(_go);
	}
	[ServerRpc(RequireOwnership = false)]
	public void ModifyVFXStacksServerRpc(FixedString64Bytes _path, int _stacks)
	{
		ModifyVFXStacksClientRpc(_path, _stacks);
	}
	[ClientRpc]
	private void ModifyVFXStacksClientRpc(FixedString64Bytes path, int stacks)
	{
		storedVFX.TryGetValue(path, out var _go);
		if (_go == null) { return; }
		_go.GetComponent<VisualEffect>().SetInt("Stacks", stacks);
	}
	#region Animation Events
	public void AnimEventLocalSpawn(AnimationEventDataSO _data)
	{
		_data.SpawnOffset = new Vector3(0, 0, 0);
		GameObject _temp = Instantiate(_data.Prefab, transform.position + _data.SpawnOffset, Quaternion.identity, null);
	}
	protected virtual void animProjectileServerSpawn(AnimationEventDataSO _data)
	{
		if (!IsLocalPlayer) { return; }

		CameraManager.Instance.GetAbilitySpawnTransform(out var _spawnPos, out var _rotation, out var _distance);
		myController.SpawnProjectileServerRpc(_data.IsAbility, _data.Id, _spawnPos, _rotation);
		//animSpawnProjectileServerRpc(_data.Id, _spawnPos, _rotation);

		if (IsServer) { return; }
		myController.SpawnProjectileLocal(_data.IsAbility, _data.Id, _spawnPos, _rotation);
	}
	#endregion
}
