using System;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using Newtonsoft.Json;

namespace CMS21Together.ClientSide.Data;

[Serializable]
public class UserData
{
    public string username;
    public string ip;
    public string lobbyID;
    
    [JsonIgnore]public int playerID;
    [JsonIgnore] public Vector3Serializable position;
    [JsonIgnore] public QuaternionSerializable rotation;

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
}