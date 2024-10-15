using System;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Handle;
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

    public NetworkType selectedNetworkType = NetworkType.TCP;
    
    [JsonIgnore] public int playerID;
    [JsonIgnore] public bool isReady;
    
    [JsonIgnore] public GameScene scene;
    [JsonIgnore] public Vector3Serializable position = new Vector3Serializable(Vector3.zero);
    [JsonIgnore] public QuaternionSerializable rotation = new QuaternionSerializable(Quaternion.identity);
    
    
    [JsonIgnore][NonSerialized] public GameObject userObject;
    [JsonIgnore][NonSerialized] public Animator userAnimator;
    [JsonIgnore][NonSerialized] public Vector3Serializable lastPosition;
    
    public UserData()
    {
        username = "player";
        ip = "127.0.0.1";
        lobbyID = "";
        playerID = 1;
        selectedNetworkType = NetworkType.TCP;
    }
    
    public UserData(string _username, int _playerID)
    {
        username = _username;
        playerID = _playerID;
    }

    public void UpdateScene(string sceneName)
    {
        scene = SceneManager.UpdateScene(sceneName);
        ClientSend.SceneChangePacket(scene);
    }

    public void SpawnPlayer()
    {
        if (ClientData.Instance.playerPrefab == null) return;

        if (playerID == ClientData.UserData.playerID)
            userObject = GameData.Instance.localPlayer;
        else
        {
            userObject = Object.Instantiate(ClientData.Instance.playerPrefab, position.toVector3(), rotation.toQuaternion());
            userAnimator = userObject.GetComponent<Animator>();
            userObject.name = username;
        }
            
    }

    public void DestroyPlayer()
    {
        if(userObject == null) return;
        Object.Destroy(userObject);
        userObject = null;
    }
}