using Cysharp.Threading.Tasks;
using Game.Managers;
using Game.Managers.Camera;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EntityController
{
	private PlayerInput playerInput;
	private delegate void Tool(bool? _isPrimaryAttack, bool _isPressed);
	private delegate void AbilityCast(int _index);
	private event Tool OnToolAttack;
	private event AbilityCast OnAbilityCast;
	private readonly Ability[] abilities = new Ability[4];

	protected virtual void setInputActions()
	{
		if (!IsLocalPlayer) { return; }
		playerInput.actions["ToolPrimary"].started += _ => OnToolAttack?.Invoke(true, true);
		playerInput.actions["ToolPrimary"].canceled += _ => OnToolAttack?.Invoke(true, false);
		playerInput.actions["ToolSecondary"].started += _ => OnToolAttack?.Invoke(false, true);
		playerInput.actions["ToolSecondary"].canceled += _ => OnToolAttack?.Invoke(false, false);
		playerInput.actions["ToolTertiary"].started += _ => OnToolAttack?.Invoke(null, true);
		playerInput.actions["ToolTertiary"].canceled += _ => OnToolAttack?.Invoke(null, false);
		OnToolAttack += toolAttack;
		playerInput.actions["Ability0"].performed += _ => OnAbilityCast?.Invoke(0);
		playerInput.actions["Ability1"].performed += _ => OnAbilityCast?.Invoke(1);
		playerInput.actions["Ability2"].performed += _ => OnAbilityCast?.Invoke(2);
		playerInput.actions["Ability3"].performed += _ => OnAbilityCast?.Invoke(3);
		OnAbilityCast += castAbility;
	}
	protected virtual void removeInputActions()
	{
		if (!IsLocalPlayer) { return; }
		playerInput.actions["ToolPrimary"].started -= _ => OnToolAttack?.Invoke(true, true);
		playerInput.actions["ToolPrimary"].canceled -= _ => OnToolAttack?.Invoke(true, false);
		playerInput.actions["ToolSecondary"].started -= _ => OnToolAttack?.Invoke(false, true);
		playerInput.actions["ToolSecondary"].canceled -= _ => OnToolAttack?.Invoke(false, false);
		OnToolAttack -= toolAttack;
		playerInput.actions["Ability0"].performed -= _ => OnAbilityCast?.Invoke(0);
		playerInput.actions["Ability1"].performed -= _ => OnAbilityCast?.Invoke(1);
		playerInput.actions["Ability2"].performed -= _ => OnAbilityCast?.Invoke(2);
		playerInput.actions["Ability3"].performed -= _ => OnAbilityCast?.Invoke(3);
		OnAbilityCast -= castAbility;
	}
#if UNITY_EDITOR
	private async void editorTestInit()
	{
		await UniTask.WaitUntil(() => MultiplayerGameManager.Instance != null);
		if (!UnityEngine.SceneManagement.SceneManager.
			GetActiveScene().name.Equals("SampleScene", "Map", "PracticeTool"))
		{
			return;
		}
		if (!NetworkObject.IsPlayerObject) { return; }
		tag = OwnerClientId % 2 == 0 ? "TeamA" : "TeamB";
		await UniTask.WaitUntil(() => StaticListManager.Instance != null);
		await UniTask.WaitForSeconds(.5f);
		if (IsServer)
		{
			UserData.Value = new()
			{
				ClientID = OwnerClientId,
				Team = tag
			};
		}
		//NetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnect;
	}

	private void onClientDisconnect(ulong _clientId)
	{
		UnityEditor.EditorApplication.isPlaying = false;
	}
#endif
	public override async void OnNetworkSpawn()
	{
#if UNITY_EDITOR
		editorTestInit();
#endif
		base.OnNetworkSpawn();
		if (IsLocalPlayer)
		{
			await UniTask.WaitUntil(() => CameraManager.Instance != null);
			CameraManager.Instance.SetTarget(transform.Find("CameraFollow").transform);
			playerInput = GetComponent<PlayerInput>();
			setInputActions();

			setUpLoadout();
			setHudHpBar();
			await UniTask.WaitUntil(() => MultiplayerGameManager.Instance != null);
			MultiplayerGameManager.Instance.SetLocalController(this);
		}
		if (IsServer)
		{
			await UniTask.WaitUntil(() => MultiplayerGameManager.Instance != null);
			MultiplayerGameManager.Instance.AddController(this);
		}
		MiniMap.Instance.AddToIconList(this);
	}
	private async void setHudHpBar()
	{
		try
		{
			await UniTask.WaitUntil(() => HUDManager.Instance != null);
			HUDManager.Instance.SetPlayerHpBar(this);
		}
		catch { Debug.LogWarning("HUDManager was always null!"); }
	}
	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if (IsLocalPlayer)
		{
			removeInputActions();
		}
	}
	protected async void setUpLoadout()
	{
		await UniTask.WaitUntil(() => StaticListManager.Instance != null);
		int _toolIndex = 0;
		Ability _ability = StaticListManager.Instance.GetAbility(0);
		abilities[0] = _ability.Create(0, this, _ability);
		StaticListManager.Instance.GetToolScript(_toolIndex).SetTool(_toolIndex, this);
	}
	private void toolAttack(bool? _isPrimaryAttack, bool _isPressed)
	{
		if (IsDead.Value == true) { return; }
		int _index = 0;
		if (StaticListManager.Instance == null) { return; }
		var _toolScript = StaticListManager.Instance.GetToolScript(_index);
		if (_toolScript == null) { return; }
		switch (_isPrimaryAttack)
		{
			case true:
				_toolScript.OnPrimaryAttack(_isPressed);
				break;
			case false:
				_toolScript.OnSecondaryAttack(_isPressed);
				break;
			default:
				_toolScript.OnTertiaryAttack(_isPressed);
				break;
		}
	}
	private void castAbility(int _index)
	{
		if (IsDead.Value == true) { return; }
		if (abilities[_index] == null) { return; }
		if (abilities[_index].IsOnCooldown)
		{
			Debug.Log("This ability is on cooldown!");
			//play cant cast sound
			return;
		}
		abilities[_index].OnCast();
	}
	/// <summary>
	/// Die called from Server
	/// </summary>
	/// <param name="_assasinData"></param>
	protected override void Die(UserData _assasinData)
	{
		base.Die(_assasinData);
		if (_assasinData.ChosenToolID < -500) { return; }
		var _assasinGo = NetworkManager.SpawnManager.GetPlayerNetworkObject(_assasinData.ClientID);
		if (_assasinGo == null) { return; }
		print($"{_assasinGo.name} has slain {name}!");
	}
}
