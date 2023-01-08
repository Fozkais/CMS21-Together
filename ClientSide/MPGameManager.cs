using System.Collections.Generic;
using CMS21MP.ClientSide;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public class MPGameManager : MonoBehaviour
    {
        public static MPGameManager instance;

        public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    
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
                _player = GameObject.Find("First Person Controller");
            }
            else
            {
                _player = Instantiate(playerPrefab, _position, _rotation);
                _player.transform.position = GameObject.Find("First Person Controller").transform.position;
            }

            _player.GetComponent<PlayerManager>().id = _id;
            _player.GetComponent<PlayerManager>().username = _username;
            players.Add(_id, _player.GetComponent<PlayerManager>());
        }
    }
}