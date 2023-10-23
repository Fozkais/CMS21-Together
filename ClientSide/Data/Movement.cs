using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    public static class Movement
    {
        private static float updateRate = 0.02f;
        private static Vector3 lastPosition = Vector3.zero;
        private static Quaternion lastRotation = Quaternion.identity;


        public static void UpdatePlayersRotation(int id, QuaternionSerializable _rotation)
        {
            if (ClientData.serverPlayerInstances.ContainsKey(id))
                ClientData.serverPlayerInstances[id].transform.rotation = 
                    Quaternion.Slerp( ClientData.serverPlayerInstances[id].transform.rotation, _rotation.toQuaternion(), 0.15f);
        }

        public static void UpdatePlayersPosition(int id, Vector3Serializable _position)
        {

            if (ClientData.serverPlayerInstances.ContainsKey(id))
            {
                Vector3 pos = new Vector3(_position.x, _position.y - 0.75f, _position.z);
                ClientData.serverPlayerInstances[id].transform.position = 
                    Vector3.Slerp(ClientData.serverPlayerInstances[id].transform.position, pos, 0.15f);
            }
        }

        public static void SendMovement()
        {
            Vector3 _position = GameData.localPlayer.transform.position;

            if (Vector3.Distance(_position, lastPosition) > updateRate)
            {
                lastPosition = _position;
                ClientSend.SendPosition(new Vector3Serializable(_position));
               // MelonLogger.Msg("Sending position to server...");
            }
        }

        public static void SendRotation()
        {
            Quaternion _rotation = GameData.localPlayer.transform.rotation;
            
            if(Quaternion.Angle(_rotation, lastRotation) > updateRate)
            {
                lastRotation = _rotation;
                ClientSend.SendRotation(new QuaternionSerializable(_rotation));
            }
        }
    }
}