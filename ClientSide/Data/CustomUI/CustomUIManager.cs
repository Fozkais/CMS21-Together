using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.MainMenu.Sections;
using Il2CppCMS.MainMenu.Windows;
using Il2CppCMS.UI;
using Il2CppCMS.UI.Helpers;
using Il2CppCMS.UI.Logic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    [HarmonyPatch]
    public static class CustomUIManager
    {

        public static bool inLobbyWindow;
        public static GameObject templateButton;
        public static GameObject templateText;
        public static GameObject templateInputField;
        
        public static List<MainMenuButton> multiplayerMenuButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> hostMenuButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> saveButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> inputFieldButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> lobbyMenuButtons = new List<MainMenuButton>();

        public static List<ButtonState> V_Main_Buttons = new List<ButtonState>();
        public static List<ButtonState> MP_Main_Buttons = new List<ButtonState>();
        public static List<ButtonState> MP_Host_Buttons = new List<ButtonState>();
        public static List<ButtonState> MP_Lobby_Buttons = new List<ButtonState>();
        public static List<ButtonState> MP_Saves_Buttons = new List<ButtonState>();
        
        public static Transform V_Main_Parent;
        public static Transform MP_Main_Parent;
        public static Transform MP_Host_Parent;
        public static Transform MP_Lobby_Parent;
        public static Transform MP_Saves_Parent;

        public static UISection currentSection = UISection.V_Main;
        
        public static void OnSceneChange(string scene)
        {
            if(scene == "Menu")
                MelonCoroutines.Start(InitializeUI());
        }

        private static IEnumerator InitializeUI()
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForEndOfFrame();
            
            SavesManager.Initialize();
            
            GameObject.Find("Logo").gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            templateButton = GameObject.Find("MainMenuButton");
            templateInputField = GameObject.Find("Main").transform.GetChild(8).gameObject;
            templateText = templateButton.GetComponentInChildren<Text>().gameObject;
            
            CustomUIMain.InitializeMainMenu();
            CustomUIMain.InitializeMultiplayerMenu();
            CustomUIHost.InitializeHostMenu();
            CustomUISaves.InitializeSavesMenu();

        }

        private static void ResetUI()
        {
            CustomUIHost.isSet = false;
            CustomUIHost.isSavesSet = false;
            CustomUIHost.isnewSaveSet = false;
            CustomUIHost.displaySaves = false;
            CustomLobbyMenu.isSet = false;
            inLobbyWindow = false;
            multiplayerMenuButtons = new List<MainMenuButton>();
            hostMenuButtons = new List<MainMenuButton>();
            saveButtons = new List<MainMenuButton>();
            inputFieldButtons = new List<MainMenuButton>();
            lobbyMenuButtons = new List<MainMenuButton>();
            CustomLobbyMenu.backgrounds = new List<GameObject>();
            CustomLobbyMenu.readyText = new List<GameObject>();
            CustomLobbyMenu.usernameText = new List<GameObject>();
            CustomLobbyMenu.kickButtons = new List<GameObject>();
            SavesManager.Initialize();
        }

        public static void UpdateLobby()
        {
            if (!inLobbyWindow) return;

            for (int i = 0; i < 4; i++)
            {
                if (ClientData.Instance.players.TryGetValue(i + 1, out var player))
                {
                    if (CustomLobbyMenu.usernameText.All(s => s.GetComponent<Text>().text != player.username))
                    {
                        if (!CustomLobbyMenu.backgrounds[i].active)
                        {
                            CustomLobbyMenu.backgrounds[i].SetActive(true);
                            CustomLobbyMenu.usernameText[i].SetActive(true);
                            CustomLobbyMenu.usernameText[i].GetComponent<Text>().text = player.username;
                            CustomLobbyMenu.readyText[i].SetActive(true);
                            CustomLobbyMenu.kickButtons[i].SetActive(true);

                        }
                    }
                    else
                    {
                        if (player.isReady)
                            CustomLobbyMenu.readyText[i].GetComponent<Text>().text = "Ready";
                        else
                            CustomLobbyMenu.readyText[i].GetComponent<Text>().text = "Not Ready";

                    }
                }
                else
                {
                    if (CustomLobbyMenu.backgrounds[i].active)
                        CustomLobbyMenu.backgrounds[i].SetActive(false);
                    if (CustomLobbyMenu.usernameText[i].active)
                        CustomLobbyMenu.usernameText[i].SetActive(false);
                    if (CustomLobbyMenu.readyText[i].active)
                        CustomLobbyMenu.readyText[i].SetActive(false);
                    if (CustomLobbyMenu.kickButtons[i].active)
                        CustomLobbyMenu.kickButtons[i].SetActive(false);
                }
            }
        }
        

        public static void DisableUI(UISection section)
        {
            if (section == UISection.V_Main)
            {
                V_Main_Parent.GetComponent<MainMenuSection>().Close();
                DisableUIList(V_Main_Buttons);
            }
            else if (section == UISection.MP_Main)
                DisableUIList(MP_Main_Buttons);
            else if (section == UISection.MP_Host)
                DisableUIList(MP_Host_Buttons);
            else if (section == UISection.MP_Lobby)
                DisableUIList(MP_Lobby_Buttons);
            else if (section == UISection.MP_Saves)
                DisableUIList(MP_Saves_Buttons);
        }
        
        public static void LockUI(UISection section)
        {
            if (section == UISection.V_Main)
                LockUIList(V_Main_Buttons);
            else if (section == UISection.MP_Main)
                LockUIList(MP_Main_Buttons);
            else if (section == UISection.MP_Host)
                LockUIList(MP_Host_Buttons);
            else if (section == UISection.MP_Lobby)
                LockUIList(MP_Lobby_Buttons);
            else if (section == UISection.MP_Saves)
                LockUIList(MP_Saves_Buttons);
        }
        
        public static void UnlockUI(UISection section)
        {
            if (section == UISection.V_Main)
                UnlockUIList(V_Main_Buttons);
            else if (section == UISection.MP_Main)
                UnlockUIList(MP_Main_Buttons);
            else if (section == UISection.MP_Host)
                UnlockUIList(MP_Host_Buttons);
            else if (section == UISection.MP_Lobby)
                UnlockUIList(MP_Lobby_Buttons);
            else if (section == UISection.MP_Saves)
                UnlockUIList(MP_Saves_Buttons);
        }
        
        public static void EnableUI(UISection section)
        {
            if (section == UISection.V_Main) 
            {
                V_Main_Parent.GetComponent<MainMenuSection>().Open();
                EnableUIList(V_Main_Buttons);
            }
            else if (section == UISection.MP_Main)
                EnableUIList(MP_Main_Buttons);
            else if (section == UISection.MP_Host)
                EnableUIList(MP_Host_Buttons);
            else if (section == UISection.MP_Lobby)
                EnableUIList(MP_Lobby_Buttons);
            else if (section == UISection.MP_Saves)
                EnableUIList(MP_Saves_Buttons);
            
            currentSection = section;
        }
        
        private static void EnableUIList(List<ButtonState> buttonsToEnable)
        {
            foreach (ButtonState buttonState in buttonsToEnable)
            {
                buttonState.button.gameObject.SetActive(true);
                if (buttonState.Disabled)
                {
                    buttonState.button.SetDisabled(true, true);
                    buttonState.button.DoStateTransition(SelectionState.Disabled, true);
                }
            }
        }

        private static void DisableUIList(List<ButtonState> buttonsToDisable)
        {
            foreach (ButtonState buttonState in buttonsToDisable)
            {
                buttonState.button.gameObject.SetActive(false);
            }
        }
        
        private static void LockUIList(List<ButtonState> buttonsToLock)
        {
            foreach (ButtonState buttonState in buttonsToLock)
            {
                buttonState.button.SetDisabled(true, true);
                buttonState.button.DoStateTransition(SelectionState.Disabled, true);
            }
        }
        
        private static void UnlockUIList(List<ButtonState> buttonsToLock)
        {
            foreach (ButtonState buttonState in buttonsToLock)
            {
                buttonState.button.SetDisabled(false, true);
                buttonState.button.DoStateTransition(SelectionState.Normal, true);
            }
        }

        public static void ResetSaveUI()
        {
            for (var index = 0; index < MP_Saves_Buttons.Count; index++)
            {
                var buttonState = MP_Saves_Buttons[index];
                Object.Destroy( buttonState.button.gameObject);
            }
            MP_Saves_Buttons.Clear();
        }
        
    }
}