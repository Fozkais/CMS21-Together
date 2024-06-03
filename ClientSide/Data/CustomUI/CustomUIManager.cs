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
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    [HarmonyPatch]
    public static class CustomUIManager
    {

        public static bool inLobbyWindow;
        public static MainMenuButton multiplayerButton;
        public static List<MainMenuButton> multiplayerMenuButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> hostMenuButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> saveButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> inputFieldButtons = new List<MainMenuButton>();
        public static List<MainMenuButton> lobbyMenuButtons = new List<MainMenuButton>();
        
        public static void OnSceneChange(string scene)
        {
            if(scene == "Menu")
            {
                CustomHostMenu.isSet = false;
                CustomHostMenu.isSavesSet = false;
                CustomHostMenu.isnewSaveSet = false;
                CustomHostMenu.displaySaves = false;
                CustomLobbyMenu.isSet = false;
                inLobbyWindow = false;
                multiplayerButton = null;
                multiplayerMenuButtons = new List<MainMenuButton>();
                hostMenuButtons = new List<MainMenuButton>();
                saveButtons = new List<MainMenuButton>();
                inputFieldButtons = new List<MainMenuButton>();
                lobbyMenuButtons = new List<MainMenuButton>();
                CustomLobbyMenu.backgrounds = new List<GameObject>();
                CustomLobbyMenu.readyText = new List<GameObject>();
                CustomLobbyMenu.usernameText = new List<GameObject>();
                CustomLobbyMenu.kickButtons = new List<GameObject>();
                MelonCoroutines.Start(CustomMainMenu.DefaultMenuPatch());
                SavesManager.Initialize();
            }
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
        
        public static bool DisableInputFix(bool disable, bool disableFeatures, MainSection __instance)
        {
            MelonLogger.Msg("Begin.");
            __instance.inputIsEnabled = !disable;
            if (disableFeatures)
            {
                for (int i = 0; i < 5; i++)
                {
                    __instance.buttons[i].SetDisabledSoft(disable);
                }
                MelonLogger.Msg("1Begin.");
                if (disable)
                {
                    __instance.listNavigationManager.DisableAllFeatures();
                }
                else
                {
                    __instance.listNavigationManager.EnableAllFeatures();
                }
            }
            
            MelonLogger.Msg("Finished.");

            return false;
        }
        
        public static bool SetButtonsAlphaFix(float value, MainSection __instance)
        {
            MelonLogger.Msg("Begin.");
            for (int i = 0; i < 5; i++)
            {
                __instance.buttons[i].SetAlpha(value);
            }

            MelonLogger.Msg("Finished2.");
            
            return  false;;
        }
        
        public static bool Hide(bool hiddenFromOutside, DLCWindow __instance)
        {
            //base.Hide(hiddenFromOutside);
            __instance.SetAsActive(active: false);
            __instance.EnableGrid(enable: false);
            __instance.dlcButton.SetClickedState(clicked: false);
            if (!GameSettings.ConsoleMode)
            {
                __instance.dlcButton.Deselect();
            }
            DisableInputFix(false,false, CustomMainMenu.section);
            __instance.menuManager.DisableParallax(disable: false);
            __instance.menuManager.ChangeParallaxGrayscale(0f);
            __instance.menuManager.ChangeLogoGrayscale(0f);
            SetButtonsAlphaFix(1f, CustomMainMenu.section);
            __instance.UnregisterButtonsEvents();
            __instance. UnregisterGridEvents();
            __instance.RemoveDescriptionsActions();
            DescriptionHelper.HideDescription(WindowID.DLC);
            __instance.EnableUI(enable: false);
            __instance.isActive = false;
            
            return false;
        }
    }
}