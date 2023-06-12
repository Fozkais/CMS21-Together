using System;
using System.Collections.Generic;
using System.IO;
using CMS21MP.ClientSide.Functionnality;
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
        
        public bool isMultiplayerGUIShowed = false;


        public string ipAdress = "192.168.1.95";
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


                GUI.Box(new Rect(panelOffsetX, panelOffsetY, panelSizeX, panelSizeY), "Multiplayer Mod " + MainMod.MOD_VERSION);
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
                        if (!MainMod.SteamMode)
                        {
                            if (!MainMod.isConnected && !MainMod.isHosting)
                            {
                                Server.Start(MainMod.maxPlayer,7777);

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
                            if (!MainMod.isConnected && !MainMod.isHosting)
                            {
                                if (!MainMod.isPrefabSet)
                                {
                                    Mod.playerInit();
                                }
                                
                            }
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
                }
                if (GUI.Button(new Rect(panelOffsetX + (panelSizeX / 2) + 10, panelOffsetY + 245, (panelSizeX / 2) - 20, 35), "Stop Server"))
                {
                    if (MainMod.isHosting)
                    {
                        Client.instance.Disconnect();
                        foreach (KeyValuePair<int, PlayerInfo> element in PlayerManager.players)
                        {
                            if (element.Value != MainMod.localPlayer.GetComponent<PlayerInfo>())
                            {
                                Destroy(element.Value.gameObject);
                            }
                            else
                            {
                                Destroy(MainMod.localPlayer.GetComponent<PlayerInfo>());
                                Destroy(MainMod.localPlayer.GetComponent<MPGameManager>());
                            }
                        }
                        MainMod.isPrefabSet = false;
                        PlayerManager.players.Clear();
                        Server.Stop();
                    }
                }

               // if (GUI.Button(new Rect(panelOffsetX + (panelSizeX / 2) + 10, panelOffsetY + 300, (panelSizeX / 2) - 20, 35), "SteamAPI"))
                //{
                  //  MainMod.SteamMode = MainMod.SteamMode ? false : true;
                //}

                //GUI.Label(new Rect(panelOffsetX + 10, panelOffsetY + 300, (panelSizeX / 2), 35), "CarPreHandle :" + CarPart_PreHandling.preHandleFinished);
            }
            PreferencesManager.SavePreferences();
        }

        public void GuiOnUpdate()
        {
            if (Input.GetKeyDown(MainMod.MOD_GUI_KEY) && !isMultiplayerGUIShowed)
            {
                isMultiplayerGUIShowed = true;
                //MelonLogger.Msg("Multiplayer GUI Loaded!");
            }
            else if (Input.GetKeyDown(MainMod.MOD_GUI_KEY) && isMultiplayerGUIShowed)
            {
                isMultiplayerGUIShowed = false;
                //MelonLogger.Msg("Multiplayer GUI Unloaded!");
            }
        }
    }
}
