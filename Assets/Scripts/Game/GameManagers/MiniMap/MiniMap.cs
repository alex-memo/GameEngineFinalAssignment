using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Memo.Utilities.Game;
using UnityEngine;
using UnityEngine.UI;
namespace Game.Managers
{
	public class MiniMap : MonoBehaviour
	{
		private Transform localPlayer;
		private readonly List<Transform> teammateTransforms = new();
		private readonly List<Transform> teammateMapIcons = new();
		private readonly List<Transform> teammateVisioneCones = new();
		[SerializeField] private Vector2 mapCentre = new(0, 0);
		[SerializeField] private Vector2 mapDimensions = new(100, 100);
		[SerializeField] private RectTransform miniMapUI;
		[SerializeField] private RectTransform localPlayerIconParent;
		private Vector2 mapBoundsMax;
		private float mapWidth;
		private float mapHeight;
		public static MiniMap Instance;
		[SerializeField] private Transform mapIconPrefab;
		[SerializeField] private RectTransform miniMapZoneCutOut;
		[SerializeField] private RectTransform miniMapLine;
		private CapsuleCollider zoneCollider;
		private Transform localPlayerVisonCone;
		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
#if !UNITY_SERVER || UNITY_EDITOR
			mapBoundsMax = new Vector2(mapCentre.x + mapDimensions.x, mapCentre.y + mapDimensions.y);
			mapWidth = mapBoundsMax.x - mapCentre.x;
			mapHeight = mapBoundsMax.y - mapCentre.y;
			setUp();
#endif
		}
		private void FixedUpdate()
		{
#if !UNITY_SERVER || UNITY_EDITOR
			moveMap();
			moveZoneCutOut();
			drawLineToZone();
#endif
		}
		private void moveMap()
		{
			if (localPlayer == null) { return; }
			miniMapUI.anchoredPosition = -minimapPos(localPlayer);
			localPlayerVisonCone.rotation = Quaternion.Euler(0, 0, 180 - UnityEngine.Camera.main.transform.eulerAngles.y);
			if (teammateMapIcons == null || teammateTransforms == null) { return; }
			for (int i = 0; i < teammateMapIcons.Count; i++)
			{
				if (teammateTransforms[i] == null) { teammateTransforms.Remove(teammateTransforms[i]); teammateMapIcons[i].gameObject.SetActive(false); teammateMapIcons.Remove(teammateMapIcons[i]); continue; }
				teammateMapIcons[i].localPosition = minimapPos(teammateTransforms[i]);
				teammateVisioneCones[i].rotation = Quaternion.Euler(0, 0, 180 - teammateTransforms[i].eulerAngles.y);
			}
		}
		private void moveZoneCutOut()
		{
			if (zoneCollider == null) { return; }
			miniMapZoneCutOut.localPosition = miniMapUI.anchoredPosition + new Vector2((zoneCollider.center.x - mapCentre.x) / mapWidth * miniMapUI.rect.width,
																						(zoneCollider.center.z - mapCentre.y) / mapHeight * miniMapUI.rect.height);
			miniMapZoneCutOut.sizeDelta = new Vector2(zoneCollider.radius * 2 / mapWidth * miniMapUI.rect.width, zoneCollider.radius * 2 / mapHeight * miniMapUI.rect.height);
			if (zoneCollider.radius <= 1.0f)
			{
				miniMapLine.gameObject.SetActive(false);
			}
		}

		private async void setUp()
		{
			await UniTask.WaitUntil(() => MultiplayerGameManager.Instance != null);
			zoneCollider = MultiplayerGameManager.Instance.GetComponent<CapsuleCollider>();
		}
		private Vector2 minimapPos(Transform _player)
		{
			Vector2 _miniMapPos = new((_player.position.x - mapCentre.x) / mapWidth * miniMapUI.rect.width,
									(_player.position.z - mapCentre.y) / mapHeight * miniMapUI.rect.height);
			return _miniMapPos;
		}

		private void drawLineToZone()
		{
			Vector2 _startpoint = new Vector2(0, 0);
			Vector2 _endPoint = new Vector2(miniMapZoneCutOut.localPosition.x, miniMapZoneCutOut.localPosition.y);
			Vector2 _midPoint = (_startpoint + _endPoint) / 2;
			miniMapLine.localPosition = _midPoint;
			Vector2 _dir = _startpoint - _endPoint;
			miniMapLine.localRotation = Quaternion.Euler(0f, 0f, (float)(Math.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg));
			miniMapLine.localScale = new Vector3(_dir.magnitude, 5f, 5f);
		}

		public async void AddToIconList(Entity _controller)
		{
			await UniTask.WaitUntil(() => MultiplayerGameManager.Instance.LocalController != null);
			if (MultiplayerGameManager.Instance.LocalController.UserData.Value.IsEnemy(_controller.UserData.Value)) { return; }
			Texture _playerIcon = StaticListManager.Instance.GetIcon(_controller.UserData.Value.PlayerIconID);
			Transform _miniMapPrefab = Instantiate(mapIconPrefab, transform);
			var _mapiconSetup = _miniMapPrefab.GetComponent<MapiconSetup>();

			switch (_controller.IsLocalPlayer)
			{
				case true:
					localPlayer = _controller.transform;
					_miniMapPrefab.SetParent(localPlayerIconParent);
					localPlayerVisonCone = _mapiconSetup.GetVisionConeHolder();
					_mapiconSetup.SetupIcon(Color.green, _playerIcon);
					break;
				case false:
					teammateTransforms.Add(_controller.transform);
					_miniMapPrefab.SetParent(miniMapUI);
					teammateMapIcons.Add(_miniMapPrefab);
					_miniMapPrefab.transform.position = Vector3.zero;
					teammateVisioneCones.Add(_mapiconSetup.GetVisionConeHolder());
					_mapiconSetup.SetupIcon(Color.blue, _playerIcon);
					break;
			}
		}
#if !UNITY_SERVER || UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(new(mapCentre.x, 0, mapCentre.y), new(mapDimensions.x, 2, mapDimensions.y));
		}
#endif
	}
}