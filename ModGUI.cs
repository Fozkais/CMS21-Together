using System;
using System.Collections.Generic;
using CMS21MP.ServerSide;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide
{
    public class ModGUI : MonoBehaviour
    {
        public static ModGUI instance;
        public MainMod Mod;

        public KeyCode MultiplayerGUIToggleKey = KeyCode.RightShift;
        public bool isMultiplayerGUIShowed = false;


        public string ipAdress = "192.168.1.94";
        public string usernameField = "player2";

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
                        if ( !MainMod.isConnected)
                        {
                            if (!MainMod.isPrefabSet)
                            {
                                Mod.playerInit();
                            }
                            Mod.client.ip = ipAdress;
                            Mod.client.ConnectToServer();
                        }
                    }
                    else
                    {  
                        MelonLogger.Msg("Please launch a game before connecting to Server!");
                    }
                }
                if (GUI.Button(new Rect(panelOffsetX + 10, panelOffsetY + 90, panelSizeX - 20, 35), "Host"))
                {
                    if (SceneManager.GetActiveScene().name == "garage")
                    {
                        if (!MainMod.isConnected && !MainMod.isHosting)
                        {
                            Server.Start(4,7777);
                            MainMod.isHosting = true;
                            
                            if (!MainMod.isPrefabSet)
                            {
                                Mod.playerInit();
                            }
                            Mod.client.ip = "127.0.0.1";
                            usernameField = "player";
                            Mod.client.ConnectToServer();
                        }
                    }
                    else
                    {  
                        MelonLogger.Msg("Please launch a game before hosting a Server!");
                    }
                }
                
                ipAdress = GUI.TextField(new Rect(panelOffsetX + 10, panelOffsetY + 130, panelSizeX - 20, 25), ipAdress);
                usernameField = GUI.TextField(new Rect(panelOffsetX + 10, panelOffsetY + 160, panelSizeX - 20, 25), usernameField);
                GUI.Label(new Rect(panelOffsetX + 10, panelOffsetY + 185, panelSizeX - 20, 25),"Slots : "+ MainMod.playerConnected.ToString()+"/"+MainMod.maxPlayer.ToString());
                
                if (GUI.Button(new Rect(panelOffsetX + 10, panelOffsetY + 245, (panelSizeX / 2), 35), "Disconnect"))
                {
                    Client.instance.Disconnect();
                    foreach (KeyValuePair<int, PlayerInfo> element in PlayerManager.players)
                    {
                        Destroy(element.Value.gameObject);
                    }
                    PlayerManager.players.Clear();
                    if (MainMod.isHosting)
                    {
                        Server.Stop();
                        MainMod.isHosting = false;
                    }
                }
                if (GUI.Button(new Rect(panelOffsetX + (panelSizeX / 2) + 10, panelOffsetY + 245, (panelSizeX / 2) - 20, 35), "Stop Server"))
                {
                    if (MainMod.isHosting)
                    {
                        Client.instance.Disconnect();
                        Server.Stop();

                        MainMod.isHosting = false;
                    }
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
