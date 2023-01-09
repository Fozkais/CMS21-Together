using System.Collections.Generic;
using CMS21MP.ClientSide;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager instance;

        public static Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();
    
        public GameObject localPlayerPrefab;
        public GameObject playerPrefab;

        public void Initialize()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
        }

        public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
        {
            GameObject _player;
            if (_id == Client.instance.myId)
            {
                _player = MainMod.localPlayer;
            }
            else
            {
                _player = Instantiate(playerPrefab, _position, _rotation);
                _player.transform.position = new Vector3(0,-10,0);
            }

            _player.GetComponent<PlayerInfo>().id = _id;
            _player.GetComponent<PlayerInfo>().username = _username;
            players.Add(_id, _player.GetComponent<PlayerInfo>());
        }
    }
}