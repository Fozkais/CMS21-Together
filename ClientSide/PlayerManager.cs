using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    [RegisterTypeInIl2Cpp]
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager instance;

        public static Dictionary<int, PlayerInfo> players = new Dictionary<int, PlayerInfo>();
        public MainMod _mainMod;

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
            if (_mainMod.playerPrefab != null)
            {
                GameObject _player;
                if (_id == Client.instance.myId)
                {
                    _player = MainMod.localPlayer;
                }
                else
                {
                    _player = Instantiate(_mainMod.playerPrefab, _position, _rotation);
                    _player.transform.name = _username;
                    _player.transform.position = new Vector3(0,-10,0);
                }

                _player.GetComponent<PlayerInfo>().id = _id;
                _player.GetComponent<PlayerInfo>().username = _username;
                if (!players.ContainsKey(_id))
                {
                    players.Add(_id, _player.GetComponent<PlayerInfo>());
                }
            }
            else
            {
                MelonLogger.Msg("playerPrefabs is not set! aborting...");
                _mainMod.playerInit();
            }
        }
    }
}