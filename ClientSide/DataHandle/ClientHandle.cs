using System.Collections.Generic;
using CMS21MP.ClientSide.Data;
using CMS21MP.ServerSide;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide.DataHandle
{
    public class ClientHandle : MonoBehaviour
    {

        #region Lobby and connection
        public static void Welcome(Packet _packet)
            {
                string _msg = _packet.ReadString();
                int _myId = _packet.ReadInt();

                MelonLogger.Msg($"Message from Server:{_msg}");
                Client.Instance.Id = _myId;

                ClientSend.WelcomeReceived();
            }

            public static void Disconnect(Packet _packet)
            {
                string _msg = _packet.ReadString();
                
                MelonLogger.Msg($"Message from Server:{_msg}");
                var name = SceneManager.GetActiveScene().name;
                if(name == "garage" || name == "Junkyard" || name == "Auto_salon")
                    NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                else
                    Client.Instance.Disconnect();
                
            }

            public static void ReadyState(Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                ClientData.serverPlayers[_id].isReady = _ready;
            }
            
            public static void PlayersInfo(Packet _packet)
            {
                Player info = _packet.Read<Player>();
                ClientData.serverPlayers[info.id] = info;
                
                MelonLogger.Msg($"Received {info.username} info from server.");
            }
            
            public static void StartGame(Packet _packet)
            {
                SaveSystem.LoadSave(0, "client", true);
            }
        #endregion
    }
}