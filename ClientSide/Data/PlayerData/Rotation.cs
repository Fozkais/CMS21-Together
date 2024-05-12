using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared;
using Il2Cpp;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    public static class Rotation
    {
        private static float updateRate = 0.02f;
        private static Quaternion lastRotation;

        public static void UpdatePlayerRotation(int id, QuaternionSerializable _rotation)
        {
            if (ClientData.players.TryGetValue(id, out Player player))
            {
                if (player.scene == ModSceneManager.currentScene())
                {
                    if (ClientData.PlayersGameObjects.TryGetValue(id, out GameObject _gameObject))
                    {
                        if (_gameObject != null)
                        {
                            
                            _gameObject.GetComponent<ModCharacterController>().RotateToRotation(_rotation.toQuaternion());
                            /*_gameObject.transform.rotation = 
                                Quaternion.Lerp(_gameObject.transform.rotation,_rotation.toQuaternion(), 10f * Time.deltaTime);*/
                        }
                    }
                    else
                    {
                        ClientData.SpawnPlayer(player);
                    }
                    
                }
            }
        }
        public static void SendRotation()
        {
            if (GameData.Instance.localPlayer != null)
            {
                Quaternion _rotation = GameData.Instance.localPlayer.transform.rotation;
                if (Quaternion.Angle(_rotation, lastRotation) > updateRate)
                {
                    lastRotation = _rotation;
                    ClientSend.SendRotation(new QuaternionSerializable(_rotation));
                }
            }
            else
            {
                GameData.Instance.localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
            }
        }
    }
}