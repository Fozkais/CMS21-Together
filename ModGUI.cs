using System;
using MelonLoader;
using UnityEngine;

namespace CMS21MP
{
    public class ModGUI
    {
        public static ModGUI instance;
        public MainMod Mod;

        public KeyCode MultiplayerGUIToggleKey = KeyCode.RightShift;
        public bool isMultiplayerGUIShowed = false;


        public string ipAdress = "127.0.0.1";
        public string usernameField = "player";

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
                    Mod.client.ip = ipAdress;
                    Mod.client.ConnectToServer();
                }
                if (GUI.Button(new Rect(panelOffsetX + 10, panelOffsetY + 90, panelSizeX - 20, 35), "Host"))
                {

                }
                
                ipAdress = GUI.TextField(new Rect(panelOffsetX + 10, panelOffsetY + 130, panelSizeX - 20, 25), ipAdress);
                usernameField = GUI.TextField(new Rect(panelOffsetX + 10, panelOffsetY + 160, panelSizeX - 20, 25), usernameField);
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
