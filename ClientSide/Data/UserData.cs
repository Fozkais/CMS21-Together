using System;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data;

[Serializable]
public class UserData
{
    public string username;
    public string ip;
    public string lobbyID;
    
    [JsonIgnore] public int playerID;
    [JsonIgnore] public GameScene scene;
    [JsonIgnore] public Vector3Serializable position;
    [JsonIgnore] public QuaternionSerializable rotation;
    [JsonIgnore][NonSerialized] public GameObject userObject;

    public UserData()
    {
        username = "player";
        ip = "127.0.0.1";
        lobbyID = "";
        playerID = 1;
    }
    
    public UserData(string _username, int _playerID)
    {
        username = _username;
        playerID = _playerID;
    }

    public void SpawnPlayer()
    {
        if (ClientData.Instance.playerPrefab == null) return;

        if (playerID == ClientData.UserData.playerID)
            userObject = GameData.Instance.localPlayer;
        else
        {
            userObject = Object.Instantiate(ClientData.Instance.playerPrefab, position.toVector3(), rotation.toQuaternion());
            userObject.name = username;
        }
            
    }
}