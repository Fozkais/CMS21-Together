using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CMS21Together.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.MainMenu;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.MainMenu.Sections;
using Il2CppCMS.MainMenu.Windows;
using Il2CppCMS.UI.Description;
using Il2CppCMS.UI.Logic;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    [HarmonyPatch]
    public static class CustomUIMain
    {
        public static MainSection section;
        private static GameObject multiplayerObject;
        public static Texture2D mpIcon;
        private static bool isSet;


        public static void InitializeMainMenu()
        {
            CustomUIManager.V_Main_Parent = GameObject.Find("MainButtons").GetComponent<MainSection>().transform;

            var V_Main_Buttons =  CustomUIManager.V_Main_Parent.GetComponentsInChildren<MainMenuButton>();
            foreach (MainMenuButton button in V_Main_Buttons)
            {
                ButtonState buttonState = new ButtonState()
                {
                    button = button,
                    Disabled = false
                };
                
                CustomUIBuilder.AddButtonToSection(buttonState, UISection.V_Main);
            }
            
            RectTransform playTransform =  CustomUIManager.templateButton.GetComponent<RectTransform>();
            playTransform.anchoredPosition = new Vector2(-26, 58);
            playTransform.sizeDelta = new Vector2(180, 44);

            Texture2D b1_texture = DataHelper.LoadCustomTexture("CMS21Together.Assets.peoples.png");
            Sprite b1_sprite = Sprite.Create(b1_texture, new Rect(0, 0, b1_texture.width, b1_texture.height), new Vector2(0, 0));
            
            ButtonImage buttonImage = new ButtonImage();
            buttonImage.scale = new Vector3(.4f, .4f, .4f);
            buttonImage.sprite = b1_sprite;
            
            Vector2 b1_pos = new Vector2(95, 58);
            Vector2 b1_size = new Vector2(45, 45);
            Action b1_action = delegate { OpenMultiplayerMenu(); };
            ButtonInfo b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "");
            CustomUIBuilder.CreateNewButton(UISection.V_Main, b1_info, false, buttonImage); 
        }

        public static void InitializeMultiplayerMenu()
        {

            GameObject parent = new GameObject("MP_MainButtons");
            parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
            parent.transform.localPosition = new Vector3(-380, 0, 0);
            parent.transform.localScale = new Vector3(.65f, .65f, .65f);

            CustomUIManager.MP_Main_Parent = parent.transform;
            
            Vector2 b1_pos = new Vector2(20, 58);
            Vector2 b1_size = new Vector2(336, 65);
            Action b1_action = delegate { OpenHostMenu(); };
            ButtonInfo b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Host a game");
            CustomUIBuilder.CreateNewButton(UISection.MP_Main, b1_info, false); 
            
            Vector2 b2_pos = new Vector2(20, -17);
            Vector2 b2_size = new Vector2(336, 65);
            Action b2_action = delegate { JoinWindow(); };
            ButtonInfo b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Join a game");
            CustomUIBuilder.CreateNewButton(UISection.MP_Main, b2_info, false); 
            
            Vector2 b3_pos = new Vector2(20, -167);
            Vector2 b3_size = new Vector2(336, 65);
            Action b3_action = delegate {  };
            ButtonInfo b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, "Network type");
            CustomUIBuilder.CreateNewButton(UISection.MP_Main, b3_info, true);
            
            Vector2 b4_pos = new Vector2(20, -242);
            Vector2 b4_size = new Vector2(336, 65);
            Action b4_action = delegate {  };
            ButtonInfo b4_info = new ButtonInfo(b4_pos, b4_size, b4_action, "Mod settings");
            CustomUIBuilder.CreateNewButton(UISection.MP_Main, b4_info, true);
            
            Vector2 b5_pos = new Vector2(20, -317);
            Vector2 b5_size = new Vector2(336, 65);
            Action b5_action = delegate { OpenMainMenu(); };
            ButtonInfo b5_info = new ButtonInfo(b5_pos, b5_size, b5_action, "Back to menu");
            CustomUIBuilder.CreateNewButton(UISection.MP_Main, b5_info, false);

            CustomUIManager.DisableUI(UISection.MP_Main);
        }

        private static void OpenMultiplayerMenu()
        {
            CustomUIManager.DisableUI(UISection.V_Main);
            CustomUIManager.EnableUI(UISection.MP_Main);
        }
        
        private static void OpenHostMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Main);
            CustomUIManager.EnableUI(UISection.MP_Host);
        }
        
        private static void OpenMainMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Main);
            CustomUIManager.EnableUI(UISection.V_Main);
        }
        
        private static void OpenLobby()
        {
            CustomUIManager.DisableUI(UISection.MP_Main);
            CustomUIManager.EnableUI(UISection.MP_Lobby);
        }

        private static void JoinWindow()
        { 
            CustomUIManager.LockUI(CustomUIManager.currentSection);
                
            Vector2 position = new Vector2(600, 0);
            Vector2 size = new Vector2(600, 400);
            Action a1 = delegate
            {
                for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                
                CustomUIBuilder.tmpInputWindow.Clear();
                CustomUIManager.UnlockUI(CustomUIManager.currentSection);
            };
            Action a2 = delegate
            {

                var _index = CustomUIBuilder.tmpInputWindow.Count - 1;
                InputField inputFiel2 = CustomUIBuilder.tmpInputWindow[_index].GetComponentInChildren<InputField>();
                InputField inputFiel1 = CustomUIBuilder.tmpInputWindow[_index-1].GetComponentInChildren<InputField>();
                string username = inputFiel2.text;
                string address = inputFiel1.text;
                
                Client.Instance.username = username;
                Client.Instance.ip = address;
                PreferencesManager.SavePreferences();

                if (!string.IsNullOrEmpty(Client.Instance.username))
                {
                    Client.Instance.ConnectToServer(address);
                    Application.runInBackground = true;

                    OpenLobby();
                }
                else
                    return;
                
                
                for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                
                CustomUIBuilder.tmpInputWindow.Clear();
                CustomUIManager.UnlockUI(CustomUIManager.currentSection);
            };
            
            CustomUIBuilder.CreateNewDoubleInputWindow(position, size, new[] { a1, a2 }, new []{"Close", "Join"}); 
        }
        
        [HarmonyPatch(typeof(MainSection), "OnMouseHover")]
        [HarmonyPrefix]
        private static bool MainSection_OnMouseHoverFix(int y, MainSection __instance)
        {
            if (y >= 6)
            {
                if (__instance.buttons.Length >= y)
                {
                    __instance.buttons[y].Select();
                    return false;
                }
            }

            return true;
        }
    }
}