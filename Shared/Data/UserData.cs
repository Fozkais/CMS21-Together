using System;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ClientSide.Data.Player;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS21Together.Shared.Data;

[Serializable]
public class UserData
{
	public string username;
	public string ip;
	public string lobbyID;
	public string playerGUID;

	public NetworkType selectedNetworkType = NetworkType.DirectIP;

	[JsonIgnore] public int playerID;
	[JsonIgnore] public bool isReady;

	[JsonIgnore] public GameScene scene;
	[JsonIgnore] public Vector3Serializable position = new(Vector3.zero);
	[JsonIgnore] public QuaternionSerializable rotation = new(Quaternion.identity);
	[JsonIgnore] public int playerLevel;
	[JsonIgnore] public int playerExp;
	[JsonIgnore] public int playerSkillPoints;
	[JsonIgnore] [NonSerialized] public Vector3Serializable lastPosition;
	[JsonIgnore] [NonSerialized] public Animator userAnimator;

	[JsonIgnore] public bool isInCar = false;
	[JsonIgnore] public int carLoaderID = -1; // -1 means not in any car

	[JsonIgnore] [NonSerialized] public GameObject userObject;
	[JsonIgnore] [NonSerialized] public float lastUpdateTime;

	public UserData()
	{
		username = "player";
		ip = "127.0.0.1";
		lobbyID = "";
		playerID = 1;
		playerGUID = username;
		selectedNetworkType = NetworkType.DirectIP;
	}

	public UserData(string _username, int _playerID, string playerGuid)
	{
		username = _username;
		playerID = _playerID;
		playerGUID = playerGuid;
	}

	public void UpdateScene(string sceneName)
	{
		scene = SceneManager.UpdateScene(sceneName);
		ClientSend.SceneChangePacket(scene);
	}

	public void SpawnPlayer()
	{
		// Security Rule: Prevent duplicate player spawning
		if (userObject != null)
		{
			MelonLogger.Warning($"[UserData->SpawnPlayer] Player {username} (ID: {playerID}) already spawned. Skipping duplicate spawn.");
			return;
		}

		if (ClientData.Instance.playerPrefab == null)
		{
			MelonLogger.Error("[CMS21-Together] Cannot spawn player: playerPrefab is null.");
			return;
		}
		if (playerID == ClientData.UserData.playerID)
		{
			if (GameData.Instance.localPlayer == null)
			{
				MelonLogger.Error("[CMS21-Together] Cannot spawn local player: localPlayer is null.");
				return;
			}
			userObject = GameData.Instance.localPlayer;
		}
		else
		{
			// Security Rule: Validate GameData.Instance and localPlayer before accessing
			if (GameData.Instance == null || GameData.Instance.localPlayer == null)
			{
				MelonLogger.Warning($"[UserData->SpawnPlayer] GameData.Instance or localPlayer is null. Cannot spawn player {username}. Will retry later.");
				return;
			}

			// Security Rule: Validate localPlayer has Collider component
			var localPlayerCollider = GameData.Instance.localPlayer.GetComponent<Collider>();
			if (localPlayerCollider == null)
			{
				MelonLogger.Warning($"[UserData->SpawnPlayer] Local player has no Collider component. Cannot set up collision ignore for {username}.");
			}

			// Business Logic: Instantiate player prefab at correct position and rotation
			userObject = Object.Instantiate(ClientData.Instance.playerPrefab, position.toVector3(), rotation.toQuaternion());
			userObject.AddComponent<InfoBillboard>();
			userAnimator = userObject.GetComponent<Animator>();
			userObject.name = username;
			
			// Security Rule: Only ignore collision if both colliders exist
			var userObjectCollider = userObject.GetComponent<Collider>();
			if (localPlayerCollider != null && userObjectCollider != null)
			{
				Physics.IgnoreCollision(localPlayerCollider, userObjectCollider);
			}
			else
			{
				MelonLogger.Warning($"[UserData->SpawnPlayer] One or both players missing Collider. Collision ignore not set for {username}.");
			}
			
			MelonLogger.Msg($"[UserData->SpawnPlayer] Spawned player {username} (ID: {playerID}) at position {position.toVector3()}");
		}

	}

	public void DestroyPlayer()
	{
		if (userObject == null) return;
		Object.Destroy(userObject);
		userObject = null;
	}
}