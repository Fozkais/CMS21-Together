using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            Vector3 V3null = Vector3.zero;
            
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
            Action b2_action = delegate {  };
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
        
        private static void EnableJoinWindow(bool phase, string ip = null)
        {
            if (CustomUIMain.section != null)
            {
                for (int i = 0; i < CustomUIMain.section.buttons.Length; i++)
                {
                    if (i > 22 && i <= 25)
                    {
                        if (CustomUIMain.section.buttons[i] != null)
                        {
                            CustomUIMain.section.buttons[i].gameObject.SetActive(true);

                        }
                    }
                }

                var window = CustomUIMain.section.transform.parent.FindChild("NameWindow");
                var saveWindow = window.GetComponentInChildren<NewSaveWindow>();
                window.gameObject.SetActive(true);

                if (!phase)
                {
                    window.GetComponentInChildren<Text>().text = "Enter IP Address";
                    saveWindow.inputField.text = Client.Instance.ip;
                    
                    GameObject confirmButtonObject = Object.Instantiate( CustomUIManager.templateButton); 
                    GameObject cancelButtonObject = Object.Instantiate( CustomUIManager.templateButton); 
                    
                    RectTransform confirmButtonTransform = confirmButtonObject.GetComponent<RectTransform>();
                    MainMenuButton confirmButtonComponent = confirmButtonObject.GetComponent<MainMenuButton>();
                
                    confirmButtonTransform.parent = window;
                    confirmButtonTransform.parentInternal = window;
                
                    confirmButtonTransform.anchoredPosition = new Vector2(145, -100);
                    confirmButtonTransform.sizeDelta = new Vector2(120, 50);
                
                    confirmButtonComponent.GetComponentInChildren<Text>().text = "Confirm"; 
                    confirmButtonObject.SetActive(true);

                    confirmButtonComponent.OnClick = new MainMenuButton.ButtonEvent();
                    Action confirmAction;
                    confirmAction = ConfirmJoinAction(new []{confirmButtonObject, cancelButtonObject},false, saveWindow.inputField);
                    confirmButtonComponent.OnClick.AddListener(confirmAction);
                    
                    RectTransform cancelButtonTransform =cancelButtonObject.GetComponent<RectTransform>();
                    MainMenuButton cancelButtonComponent = cancelButtonObject.GetComponent<MainMenuButton>();
                
                    cancelButtonTransform.parent = window;
                    cancelButtonTransform.parentInternal = window;
                
                    cancelButtonTransform.anchoredPosition = new Vector2(-145, -100);
                    cancelButtonTransform.sizeDelta = new Vector2(120, 50);
                
                    cancelButtonTransform.GetComponentInChildren<Text>().text = "Cancel"; 
                    cancelButtonObject.SetActive(true);

                    cancelButtonComponent.OnClick = new MainMenuButton.ButtonEvent();
                    Action cancelAction;
                    cancelAction = delegate {
                        DisableInputsWindow(new []{confirmButtonObject, cancelButtonObject});
                        saveWindow.inputField.text = "";
                    };
                    cancelButtonComponent.OnClick.AddListener(cancelAction);
                }
                else
                {
                    window.GetComponentInChildren<Text>().text = "Enter Username";
                    saveWindow.inputField.text = Client.Instance.username;
                    
                    GameObject cancelButtonObject = Object.Instantiate( CustomUIManager.templateButton); 
                    GameObject confirmButtonObject = Object.Instantiate( CustomUIManager.templateButton); 
                    
                    RectTransform confirmButtonTransform = confirmButtonObject.GetComponent<RectTransform>();
                    MainMenuButton confirmButtonComponent = confirmButtonObject.GetComponent<MainMenuButton>();
                
                    confirmButtonTransform.parent = window;
                    confirmButtonTransform.parentInternal = window;
                
                    confirmButtonTransform.anchoredPosition = new Vector2(145, -100);
                    confirmButtonTransform.sizeDelta = new Vector2(120, 50);
                
                    confirmButtonComponent.GetComponentInChildren<Text>().text = "Connect"; 
                    confirmButtonObject.SetActive(true);

                    confirmButtonComponent.OnClick = new MainMenuButton.ButtonEvent();
                    Action confirmAction;
                    confirmAction = ConfirmJoinAction(new []{confirmButtonObject, cancelButtonObject}, true, saveWindow.inputField, ip);
                    confirmButtonComponent.OnClick.AddListener(confirmAction);
                    
                    RectTransform cancelButtonTransform =cancelButtonObject.GetComponent<RectTransform>();
                    MainMenuButton cancelButtonComponent = cancelButtonObject.GetComponent<MainMenuButton>();
                
                    cancelButtonTransform.parent = window;
                    cancelButtonTransform.parentInternal = window;
                
                    cancelButtonTransform.anchoredPosition = new Vector2(-145, -100);
                    cancelButtonTransform.sizeDelta = new Vector2(120, 50);
                    
                    cancelButtonTransform.GetComponentInChildren<Text>().text = "Cancel"; 
                    cancelButtonObject.SetActive(true);

                    cancelButtonComponent.OnClick = new MainMenuButton.ButtonEvent();
                    Action cancelAction;
                    cancelAction = delegate
                    {
                        DisableInputsWindow(new []{confirmButtonObject, cancelButtonObject});
                        saveWindow.inputField.text = "";
                    };
                    cancelButtonComponent.OnClick.AddListener(cancelAction);
                }
            }
        }
        private static void DisableInputsWindow(GameObject[] buttons)
        {
            if (CustomUIMain.section != null)
            {
                for (int i = 0; i < CustomUIMain.section.buttons.Length; i++)
                {
                    if (i > 22 && i <= 25)
                    {
                        if (CustomUIMain.section.buttons[i] != null)
                        {
                            CustomUIMain.section.buttons[i].gameObject.SetActive(false);
                        }
                    }
                }

                foreach (GameObject button in buttons)
                {
                    Object.Destroy(button);
                }
                
                var window = CustomUIMain.section.transform.parent.FindChild("NameWindow");
                window.gameObject.SetActive(false);
            }
        }
        private static Action ConfirmJoinAction( GameObject[] buttons, bool phase,InputField input, string ip=null)
        {
            return () =>
            {
                if (!phase)
                {
                    DisableInputsWindow(buttons);
                    EnableJoinWindow(true, input.text);
                }
                else
                {
                    DisableInputsWindow(buttons);
                    
                    Client.Instance.username = input.text;
                    Client.Instance.ip = ip;
                    PreferencesManager.SavePreferences();

                    if (!string.IsNullOrEmpty(Client.Instance.username))
                    {
                        Client.Instance.ConnectToServer(ip);
                        Application.runInBackground = true;
                        
                        CustomLobbyMenu.OpenLobby(true);
                    }

                }
            };


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