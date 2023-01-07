using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP
{
    public class ModGUI : MonoBehaviour
    {
        public static ModGUI instance;
        public MainMod Mod;

        public KeyCode MultiplayerGUIToggleKey = KeyCode.RightShift;
        public bool isMultiplayerGUIShowed = false;


        public string ipAdress = "127.0.0.1";
        public string usernameField = "player";

        public Vector3 player2Pos;
        public string player2Position;


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

        public void OnGUI()
        {
            if (isMultiplayerGUIShowed)
            {
                int panelSizeX = 300;
                int panelSizeY = 300;
                float panelOffsetX = Screen.width - (panelSizeX + 25);
                float panelOffsetY = 25;


                GUI.Box(new Rect(panelOffsetX, panelOffsetY, panelSizeX, panelSizeY), "Multiplayer GUI");
                if (GUI.Button(new Rect(panelOffsetX + 10, panelOffsetY + 50, panelSizeX - 20, 35), "Connect"))
                {
                    if (SceneManager.GetActiveScene().name == "garage")
                    {
                        Mod.client.ip = ipAdress;
                        Mod.client.ConnectToServer();
                    }
                    else
                    {  
                        MelonLogger.Msg("Please launch a game before connecting to Server!");
                    }
                }
                if (GUI.Button(new Rect(panelOffsetX + 10, panelOffsetY + 90, panelSizeX - 20, 35), "Host"))
                {

                }
                
                ipAdress = GUI.TextField(new Rect(panelOffsetX + 10, panelOffsetY + 130, panelSizeX - 20, 25), ipAdress);
                usernameField = GUI.TextField(new Rect(panelOffsetX + 10, panelOffsetY + 160, panelSizeX - 20, 25), usernameField);
                GUI.Label(new Rect(panelOffsetX + 10, panelOffsetY + 185, panelSizeX - 20, 25), "player2Pos : "+player2Pos.ToString());
                GUI.Label(new Rect(panelOffsetX + 10, panelOffsetY + 215, panelSizeX - 20, 25),"Slots : "+ MainMod.playerConnected.ToString()+"/"+MainMod.maxPlayer.ToString());
                
                if (GUI.Button(new Rect(panelOffsetX + 10, panelOffsetY + 245, panelSizeX - 20, 35), "Disconnect"))
                {
                    Client.instance.Disconnect();
                }
            }
        }

        public void GuiOnUpdate()
        {
            if (Input.GetKeyDown(MultiplayerGUIToggleKey) && !isMultiplayerGUIShowed)
            {
                isMultiplayerGUIShowed = true;
                MelonLogger.Msg("Multiplayer GUI Loaded!");
            }
            else if (Input.GetKeyDown(MultiplayerGUIToggleKey) && isMultiplayerGUIShowed)
            {
                isMultiplayerGUIShowed = false;
                MelonLogger.Msg("Multiplayer GUI Unloaded!");
            }
        }
    }
}
