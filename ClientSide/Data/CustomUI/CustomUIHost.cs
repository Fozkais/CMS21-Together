using System;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using Il2CppCMS.MainMenu;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.MainMenu.Sections;
using Il2CppCMS.MainMenu.Windows;
using Il2CppCMS.UI.Logic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUIHost
    {
        private static int displayedPage = 1;
        
        public static void InitializeHostMenu()
        {
            GameObject parent = new GameObject("MP_HostButtons");
            parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
            parent.transform.localPosition = new Vector3(-380, 0, 0);
            parent.transform.localScale = new Vector3(.65f, .65f, .65f);

            CustomUIManager.MP_Host_Parent = parent.transform;
            
            Vector2 b1_pos = new Vector2(20, 58);
            Vector2 b1_size = new Vector2(336, 65);
            Action b1_action = delegate { OpenSavesMenu(); };
            ButtonInfo b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Load/Create a game");
            CustomUIBuilder.CreateNewButton(UISection.MP_Host, b1_info, false);
            
            /*Vector2 b2_pos = new Vector2(20, -17);
            Vector2 b2_size = new Vector2(336, 65);
            Action b2_action = delegate {  }; 
            ButtonInfo b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Delete a game"); // Moved into save info
            CustomUIBuilder.CreateNewButton(UISection.MP_Host, b2_info, true);*/
            
            Vector2 b3_pos = new Vector2(20, -317);
            Vector2 b3_size = new Vector2(336, 65);
            Action b3_action = delegate { OpenMultiplayerMenu(); };
            ButtonInfo b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, "Back to menu");
            CustomUIBuilder.CreateNewButton(UISection.MP_Host, b3_info, false);
            
            CustomUIManager.DisableUI(UISection.MP_Host);
        }

        private static void OpenMultiplayerMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Host);
            CustomUIManager.EnableUI(UISection.MP_Main);
        }
        
        private static void OpenSavesMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Host);
            
            CustomUISaves.saveIndex = 0;
            CustomUIManager.EnableUI(UISection.MP_Saves);
        }
        
    }

}