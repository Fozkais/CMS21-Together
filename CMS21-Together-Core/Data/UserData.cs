using System;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace CMS21_Together_Core.Data;

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
		playerGUID = Guid.NewGuid().ToString();
		selectedNetworkType = NetworkType.TCP;
	}

	public UserData(string _username, int _playerID, string playerGuid)
	{
		username = _username;
		playerID = _playerID;
		playerGUID = playerGuid;
	}
}