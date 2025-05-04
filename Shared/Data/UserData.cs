using System;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ClientSide.Data.Player;
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

	public NetworkType selectedNetworkType = NetworkType.TCP;

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


	[JsonIgnore] [NonSerialized] public GameObject userObject;
	[JsonIgnore] [NonSerialized] public float lastUpdateTime;

	public UserData()
	{
		username = "player";
		ip = "127.0.0.1";
		lobbyID = "";
		playerID = 1;
		playerGUID = Guid.NewGuid().ToString();
		selectedNetworkType = NetworkType.TCP;
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
		if (ClientData.Instance.playerPrefab == null)
		{
			Debug.LogError("[CMS21-Together] Cannot spawn player: playerPrefab is null.");
			return;
		}
		if (playerID == ClientData.UserData.playerID) 
			userObject = GameData.Instance.localPlayer;
		else
		{
			userObject = Object.Instantiate(ClientData.Instance.playerPrefab, position.toVector3(), rotation.toQuaternion());
			userObject.AddComponent<InfoBillboard>();
			userAnimator = userObject.GetComponent<Animator>();
			userObject.name = username;
			Physics.IgnoreCollision(GameData.Instance.localPlayer.GetComponent<Collider>(), userObject.GetComponent<Collider>());
		}

	}

	public void DestroyPlayer()
	{
		if (userObject == null) return;
		Object.Destroy(userObject);
		userObject = null;
	}
}