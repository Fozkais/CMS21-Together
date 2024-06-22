using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CMS21Together.ServerSide;
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
    public static class CustomMainMenu
    {
        public static MainSection section;
        public static GameObject templateButtonObject;
        public static GameObject templateTextObject;
        private static GameObject multiplayerObject;
        public static Texture2D mpIcon;
        private static bool isSet;


        public static IEnumerator DefaultMenuPatch()
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForEndOfFrame();

            isSet = false;
            CustomHostMenu.isSet = false;
            CustomHostMenu.isSavesSet = false;
            CustomHostMenu.isnewSaveSet = false;


            GameObject.Find("Logo").gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

            templateButtonObject = GameObject.Find("MainMenuButton");
            templateTextObject = templateButtonObject.GetComponentInChildren<Text>().gameObject;
            RectTransform playTransform = templateButtonObject.GetComponent<RectTransform>();

            section = GameObject.Find("MainButtons").GetComponent<MainSection>();

            playTransform.anchoredPosition = new Vector2(-26, 58);
            playTransform.sizeDelta = new Vector2(180, 44);

            multiplayerObject = Object.Instantiate(templateButtonObject);
            GameObject imageObj = new GameObject("Image");
            imageObj.transform.parent = multiplayerObject.transform;
            imageObj.AddComponent<Image>();

            multiplayerObject.GetComponentInChildren<Text>().gameObject.SetActive(false);

            RectTransform mpTransform = multiplayerObject.GetComponent<RectTransform>();
            MainMenuButton mpButton = multiplayerObject.GetComponent<MainMenuButton>();
            Image mpImage = imageObj.GetComponent<Image>();

            mpTransform.parent = playTransform.parent;
            mpTransform.parentInternal = playTransform.parentInternal;
            mpButton.Y = 6;
            CustomUIManager.multiplayerButton = mpButton;
            mpTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            mpTransform.anchoredPosition = new Vector2(95, 58);
            mpTransform.sizeDelta = new Vector2(53, 53);

            Texture2D myTexture = DataHelper.LoadCustomTexture("CMS21Together.Assets.peoples.png");
            Sprite mpSprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                new Vector2(0, 0));

            mpImage.sprite = mpSprite;
            imageObj.transform.position = new Vector3(0, 0, 0);
            imageObj.transform.localPosition = new Vector3(0, 0, 0);
            imageObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            mpButton.AssignAction(null);
            mpButton.OnClick = new MainMenuButton.ButtonEvent();

            Action openMenu = delegate { MultiplayerMenu(); };

            mpButton.OnClick.AddListener(openMenu);
            mpButton.AssignAction(openMenu);
        }

        public static void MultiplayerMenu()
        {
            if (isSet)
            {
                EnableMultiplayerMenu();
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                if (section.buttons[i] != null)
                    section.buttons[i].gameObject.SetActive(false);
            }
            CustomUIManager.multiplayerButton.gameObject.SetActive(false);   

            var hostObject = Object.Instantiate(templateButtonObject);
            RectTransform hostTransform = hostObject.GetComponent<RectTransform>();
            MainMenuButton hostButton = hostObject.GetComponent<MainMenuButton>();

            var parent = templateButtonObject.transform.parent;

            hostTransform.parent = parent;
            hostTransform.parentInternal = templateButtonObject.GetComponent<RectTransform>().parentInternal;
            hostButton.Y = 7;
           // hostButton.OnMouseHover = /*section.OnMouseHover(7);*/ templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomUIManager.multiplayerMenuButtons.Add(hostButton);

            hostTransform.anchoredPosition = new Vector2(-5, 58);
            hostTransform.sizeDelta = new Vector2(288, 55);
            hostButton.GetComponentInChildren<Text>().text = "Host a game";
            hostButton.OnClick = new MainMenuButton.ButtonEvent();
            Action hostMenu = delegate { CustomHostMenu.CreateHostMenu(); };
            hostButton.OnClick.AddListener(hostMenu);
            hostObject.SetActive(true);

            var joinObject = Object.Instantiate(templateButtonObject);
            RectTransform joinTransform = joinObject.GetComponent<RectTransform>();
            MainMenuButton joinButton = joinObject.GetComponent<MainMenuButton>();

            joinTransform.parent = parent;
            joinTransform.parentInternal = templateButtonObject.GetComponent<RectTransform>().parentInternal;
            joinButton.Y = 8;
           // joinButton.OnMouseHover = templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomUIManager.multiplayerMenuButtons.Add(joinButton);

            joinTransform.anchoredPosition = new Vector2(-5, 10);
            joinTransform.sizeDelta = new Vector2(288, 55);
            joinButton.GetComponentInChildren<Text>().text = "Join a server";
            joinButton.OnClick = new MainMenuButton.ButtonEvent();
            Action joinMenu = delegate { EnableJoinWindow(false); };
            joinButton.OnClick.AddListener(joinMenu);
            joinObject.SetActive(true);

            var networkObject = Object.Instantiate(templateButtonObject);
            RectTransform networkTransform = networkObject.GetComponent<RectTransform>();
            MainMenuButton networkButton = networkObject.GetComponent<MainMenuButton>();

            networkTransform.parent = parent;
            networkTransform.parentInternal = templateButtonObject.GetComponent<RectTransform>().parentInternal;
            networkButton.Y = 9;
            //networkButton.OnMouseHover = templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomUIManager.multiplayerMenuButtons.Add(networkButton);

            networkTransform.anchoredPosition = new Vector2(-5, -38);
            networkTransform.sizeDelta = new Vector2(288, 55);
            networkButton.GetComponentInChildren<Text>().text = "Network type";
            networkButton.OnClick = new MainMenuButton.ButtonEvent();
          //  networkButton.isDisabled = true;
          //  networkButton.DoStateTransition(SelectionState.Disabled, true);
            Action networkChange = delegate { ChangeNetworkType(); };
            networkButton.OnClick.AddListener(networkChange);
            networkObject.SetActive(true);

            var settingObject = Object.Instantiate(templateButtonObject);
            RectTransform settingTransform = settingObject.GetComponent<RectTransform>();
            MainMenuButton settingButton = settingObject.GetComponent<MainMenuButton>();

            settingTransform.parent = parent;
            settingTransform.parentInternal = templateButtonObject.GetComponent<RectTransform>().parentInternal;
            settingButton.Y = 10;
           // settingButton.OnMouseHover = templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomUIManager.multiplayerMenuButtons.Add(settingButton);

            settingTransform.anchoredPosition = new Vector2(-5, -152);
            settingTransform.sizeDelta = new Vector2(288, 55);
            settingButton.GetComponentInChildren<Text>().text = "Mod settings";
            settingButton.OnClick = new MainMenuButton.ButtonEvent();
            settingButton.isDisabled = true;
            settingButton.DoStateTransition(SelectionState.Disabled, true);
            Action settingChange = delegate { };
            settingButton.OnClick.AddListener(settingChange);
            settingObject.SetActive(true);

            var backObject = Object.Instantiate(templateButtonObject);
            RectTransform backTransform = backObject.GetComponent<RectTransform>();
            MainMenuButton backButton = backObject.GetComponent<MainMenuButton>();

            backTransform.parent = parent;
            backTransform.parentInternal = templateButtonObject.GetComponent<RectTransform>().parentInternal;
            backButton.Y = 11;
           // backButton.OnMouseHover = templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomUIManager.multiplayerMenuButtons.Add(backButton);

            backTransform.anchoredPosition = new Vector2(-5, -200);
            backTransform.sizeDelta = new Vector2(288, 55);
            backButton.GetComponentInChildren<Text>().text = "Back to menu";
            backButton.OnClick = new MainMenuButton.ButtonEvent();
            Action backtoMenu = delegate { EnableMainMenu(); };
            backButton.OnClick.AddListener(backtoMenu);
            backObject.SetActive(true);


            networkButton.DoStateTransition(SelectionState.Disabled, true);
            settingButton.DoStateTransition(SelectionState.Disabled, true);

            isSet = true;
        }

        public static void ChangeNetworkType()
        {
            switch (Client.Instance.currentType)
            {
                case NetworkType.TcpUdp:
                    Client.Instance.currentType = NetworkType.steamNetworking;
                    
                    break;
                case NetworkType.steamNetworking:
                    Client.Instance.currentType = NetworkType.TcpUdp;
                    break;
            }
            
            switch (Server.currentType)
            {
                case NetworkType.TcpUdp:
                    Server.currentType = NetworkType.steamNetworking;
                    
                    break;
                case NetworkType.steamNetworking:
                    Server.currentType = NetworkType.TcpUdp;
                    break;
            }
            MelonLogger.Msg("Client Networking Type as been set to : " + Client.Instance.currentType.ToString());
            MelonLogger.Msg("Server Networking Type as been set to : " + Server.currentType.ToString());
            // TODO: Show a info box to tell on wich networking type is being used 
        }

        public static void EnableMultiplayerMenu()
        {
            for (int i = 0; i < section.buttons.Count; i++)
            {
                if (section.buttons[i] != null)
                    section.buttons[i].gameObject.SetActive(false);
            }
            
            CustomUIManager.multiplayerButton.gameObject.SetActive(false);   
            
            for (int i = 0; i <  CustomUIManager.multiplayerMenuButtons.Count; i++)
            {
                if (CustomUIManager.multiplayerMenuButtons[i] != null)
                    CustomUIManager.multiplayerMenuButtons[i].gameObject.SetActive(true);
            }

            CustomUIManager.multiplayerMenuButtons[2].isDisabled = true;
            CustomUIManager.multiplayerMenuButtons[2].DoStateTransition(SelectionState.Disabled, true);
            CustomUIManager.multiplayerMenuButtons[3].isDisabled = true;
            CustomUIManager.multiplayerMenuButtons[3].DoStateTransition(SelectionState.Disabled, true);
        }
        public static void EnableMainMenu()
        {
            for (int i = 0; i <  CustomUIManager.multiplayerMenuButtons.Count; i++)
            {
                if (CustomUIManager.multiplayerMenuButtons[i] != null)
                    CustomUIManager.multiplayerMenuButtons[i].gameObject.SetActive(false);
            }
            
            for (int i = 0; i < section.buttons.Length; i++)
            {
                if (section.buttons[i] != null)
                    section.buttons[i].gameObject.SetActive(true);
            }
            CustomUIManager.multiplayerButton.gameObject.SetActive(true);
            
        }
        private static void EnableJoinWindow(bool phase, string ip = null)
        {
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 22 && i <= 25)
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(true);

                        }
                    }
                }

                var window = CustomMainMenu.section.transform.parent.FindChild("NameWindow");
                var saveWindow = window.GetComponentInChildren<NewSaveWindow>();
                window.gameObject.SetActive(true);

                if (!phase)
                {
                    window.GetComponentInChildren<Text>().text = "Enter IP Address or ServerID";
                    saveWindow.inputField.text = Client.Instance.ip;
                    
                    GameObject confirmButtonObject = Object.Instantiate(templateButtonObject); 
                    GameObject cancelButtonObject = Object.Instantiate(templateButtonObject); 
                    
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
                    
                    GameObject cancelButtonObject = Object.Instantiate(templateButtonObject); 
                    GameObject confirmButtonObject = Object.Instantiate(templateButtonObject); 
                    
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
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 22 && i <= 25)
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                        }
                    }
                }

                foreach (GameObject button in buttons)
                {
                    Object.Destroy(button);
                }
                
                var window = CustomMainMenu.section.transform.parent.FindChild("NameWindow");
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