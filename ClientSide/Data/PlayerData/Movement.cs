using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    public static class Movement
    {
        private static float updateRate = 0.02f;
        private static Vector3 lastPosition;

        public static void SetInitialPosition(int id, Vector3Serializable _position)
        {
            if (ClientData.players.TryGetValue(id, out Player player))
            {
                if (player.scene == ModSceneManager.currentScene())
                {
                    if (ClientData.PlayersGameObjects.TryGetValue(id, out GameObject _gameObject))
                    {
                        if (_gameObject != null)
                        {
                            _gameObject.transform.position = _position.toVector3();
                            MelonLogger.Msg("Received InitialPos, updating..");
                        }
                    }
                }
            }
        }

        public static void UpdatePlayerPosition(int id, Vector3Serializable _position)
        {
            if (ClientData.players.TryGetValue(id, out Player player))
            {
                if (player.scene == ModSceneManager.currentScene())
                {
                    if (ClientData.PlayersGameObjects.TryGetValue(id, out GameObject _gameObject))
                    {
                        if (_gameObject != null)
                        {
                            player.desiredPosition = _position; // Assign the target position directly

                            MelonLogger.Msg($"Player {id} desired position: {player.desiredPosition.toVector3()}");
                        }
                        
                    }
                    else
                    {
                        ClientData.SpawnPlayer(player);
                    }
                    
                }
            }
        }

        public static void SendPosition()
        {
            if (GameData.localPlayer != null)
            {
                Vector3 _position = GameData.localPlayer.transform.position;
                _position.y -= 0.72f;
                if (Vector3.Distance(_position, lastPosition) > updateRate)
                {
                    lastPosition = _position;
                    ClientSend.SendPosition(new Vector3Serializable(_position));
                }
            }
            else
            {
                GameData.localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
            }
        }
    }
}