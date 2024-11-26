using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerGameManager : NetworkBehaviour
{
	[SerializeField] private GameObject playerPrefab;
	public List<UserData> UserDataList { get; private set; } = new();
	public static MultiplayerGameManager Instance { get; private set; }
	public event UpdateHUD UpdateHUD;

	public NetworkVariable<int> GameTimer { get; private set; } = new(0);
	private const int roundDuration = 180;

	public List<PlayerController> PlayerControllers { get; private set; } = new();
	public List<EntityController> EntityControllers { get; private set; } = new();
	public Entity LocalController { get; private set; }

	public override void OnNetworkSpawn()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
		Init();
	}

	private async void Init()
	{
		NetworkManager.Singleton.OnClientStopped += OnClientStopped;
		await UniTask.WaitUntil(() => HUDManager.Instance != null);
		GameplayStart();
	}
	public void SetLocalController(Entity _controller)
	{
		LocalController = _controller;
	}
	private void GameplayStart()
	{
		if (IsServer)
		{
			sendUserDataListRpc(UserDataList.ToArray());
			StartCoroutine(gameTimer());
		}
		if (IsHost)
		{
			UpdateHUD?.Invoke();//we update immediately as we already have the userdatalist
		}
	}

	private IEnumerator gameTimer()
	{
		while (GameTimer.Value < roundDuration)
		{
			yield return new WaitForSecondsRealtime(1);
			++GameTimer.Value;
		}
		endGame("Draw");
	}
	private void endGame(string _winningTeam)
	{
		if (!IsServer) { return; }
		//determine winning team
		serverCleanUp();
	}
	[ClientRpc]
	private void endGameClientRpc(FixedString32Bytes _winningTeam)
	{
		//show end game screen
	}
	private void serverCleanUp()
	{
		var _clientsToDisconnect = new List<ulong>(NetworkManager.Singleton.ConnectedClients.Keys);
		ulong _hostId = NetworkManager.Singleton.LocalClientId;
		foreach (var _client in _clientsToDisconnect)
		{
			if (_client == _hostId) { continue; }
			NetworkManager.Singleton.DisconnectClient(_client);
		}
		//network shotdown is handled by serverQueryHandler if unity server
		//if not then we check if host and send to mainmenu
		if (!IsHost) { return; }
		NetworkManager.Singleton.Shutdown();
		if (SceneManager.GetActiveScene().name != "MainMenu")
		{
			SceneManager.LoadScene("MainMenu");
		}
	}

	[ClientRpc]
	private void setPlayerClientRpc(ulong _netObjID, int _skinID)
	{
		setPlayer(_netObjID, _skinID);
	}
	private async void setPlayer(ulong _netObjID, int _skinID)
	{
		await UniTask.WaitUntil(() => HUDManager.Instance != null);
		var _go = GetNetworkObject(_netObjID);
		HUDManager.Instance.SetUpPlayerTopBar(_go.gameObject);
	}
	public void SetUserDataList(List<UserData> _userDataList)
	{
		if (!IsServer) { return; }
		if (_userDataList == null) { return; }
		UserDataList = _userDataList;
		foreach (var _player in UserDataList)
		{
			Debug.Log(_player);
		}
	}
	[Rpc(SendTo.NotServer)]
	private void sendUserDataListRpc(UserData[] _userDataList)
	{
		Debug.LogWarning(UserDataList == null);
		UserDataList = _userDataList.ToList();
		UpdateHUD?.Invoke();
	}
	private void OnClientStopped(bool _)
	{
		UserDataList = new();
		Instance = null;
		Destroy(gameObject);
	}
	public override void OnDestroy()
	{
		base.OnDestroy();
		NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
	}
	public void AddEntity(EntityController _controller)
	{
		EntityControllers.Add(_controller);
	}
	public void AddController(PlayerController _controller)
	{
		PlayerControllers.Add(_controller);
	}
	public void RemoveEntity(EntityController _controller)
	{
		EntityControllers.Remove(_controller);
	}
}
public delegate void UpdateHUD();
public delegate void PlayerTakedown(UserData _victimUserData);
public delegate void PlayerRespawn(UserData _userData);


public class JsonPlayer
{
	[Newtonsoft.Json.JsonProperty("PlayerID")]
	public string PlayerID { get; set; }
	[Newtonsoft.Json.JsonProperty("Team")]
	public int Team { get; set; }
}