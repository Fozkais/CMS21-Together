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
                           // Vector3 moveDirection = (_position.toVector3() - _gameObject.transform.position).normalized;

                            // Apply the movement direction to the GameObject
                          //  _gameObject.GetComponent<Rigidbody>().AddForce(moveDirection * 5f * Time.deltaTime, ForceMode.Force);
                            
                            
                            /*player.desiredPosition = new Vector3Serializable((_position.toVector3() - _gameObject.transform.position).normalized);

                            // Déplacer dans cette direction 
                            _gameObject.GetComponent<Rigidbody>().AddForce(  player.desiredPosition.toVector3() * 5f * Time.deltaTime, ForceMode.Force);

                            // Vérifier si on est arrivé à destination
                           
                           // MelonLogger.Msg("ReceivedValid pos!");
                            /*Vector3Serializable dif = Vector3Serializable.Subtract(player.desiredPosition, _position);
                            player.desiredPosition = Vector3Serializable.Add(player.desiredPosition, dif);
                            MelonLogger.Msg($"desiredPos:{player.desiredPosition.toVector3()}");#1#*/
                        }
                            
                            
                            /*Vector3 moveDirection = (_position.toVector3() - _gameObject.transform.position).normalized;

                            // Application de la direction de déplacement au CharacterController
                            _gameObject.GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);*/
                            
                           // _gameObject.transform.position = 
                              //  Vector3.Lerp(_gameObject.transform.position,_position.toVector3(), 15f * Time.deltaTime);
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