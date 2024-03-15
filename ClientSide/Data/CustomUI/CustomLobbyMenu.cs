using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Handle;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using Il2Cpp;
using Il2CppCMS.MainMenu.Controls;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomLobbyMenu
    {

        public static int  saveIndex;
        public static bool isSet;
        public static Texture2D lobbyPBorder;
        
        public static Dictionary<int,(int, int)> buttonPos = new Dictionary<int,(int, int)>()
        {
            {0,(280, 58)},
            {1,(280, 0)},
            {2,(280, -58)},
            {3,(280, -116)},
        };
        
        public static void OpenLobby()
        {
            if (isSet)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 25)
                    {
                        if(CustomMainMenu.section.buttons[i] != null)
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        if(CustomMainMenu.section.buttons[i] != null)
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                CustomHostMenu.DisableSavesMenu();
                CreateLobbyMenu();
            }
        }

        public static void CreateLobbyMenu()
        {
            GameObject template = CustomMainMenu.templateObject;
            isSet = true;
            for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
            {
                if (i < 26)
                {
                    if(CustomMainMenu.section.buttons[i] != null)
                        CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                }
            }
            
            
            Transform parent = template.transform;
            
            var startObject = Object.Instantiate(template);
            RectTransform  startTransform =  startObject.GetComponent<RectTransform>();
            MainMenuButton  startButton = startObject.GetComponent<MainMenuButton>();

            startTransform.parent = parent;
            startTransform.parentInternal = CustomMainMenu.templateObject.GetComponent<RectTransform>().parentInternal;
            startButton.Y = 26;
            startButton.OnMouseHover = CustomMainMenu.templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[26] = startButton;

            startTransform.anchoredPosition = new Vector2(-5, 58);
            startTransform.sizeDelta = new Vector2(288, 55);
            startButton.GetComponentInChildren<Text>().text = "Start Game";
            startButton.OnClick = new MainMenuButton.ButtonEvent();
            Action  startAction = delegate 
            {
                foreach (var player in ServerData.players.Values)
                {
                    if (player != null && !player.isReady)
                    {
                        return;
                    }
                }

                StartGame(saveIndex - 1);
                SavesManager.ModSaves[saveIndex - 1].alreadyLoaded = true;
                PreferencesManager.SaveModSave(saveIndex - 1);
            };
            startButton.OnClick.AddListener(startAction);
            startObject.SetActive(true);
            
            
            var readyObject = Object.Instantiate(template);
            RectTransform  readyTransform =  readyObject.GetComponent<RectTransform>();
            MainMenuButton  readyButton = readyObject.GetComponent<MainMenuButton>();

            readyTransform.parent = parent;
            readyTransform.parentInternal = CustomMainMenu.templateObject.GetComponent<RectTransform>().parentInternal;
            readyButton.Y = 27;
            readyButton.OnMouseHover = CustomMainMenu.templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[27] = readyButton;

            readyTransform.anchoredPosition = new Vector2(-5, 0);
            readyTransform.sizeDelta = new Vector2(288, 55);
            readyButton.GetComponentInChildren<Text>().text = "Not Ready";
            readyButton.OnClick = new MainMenuButton.ButtonEvent();
            Action  readyAction = delegate
            {
                if (readyButton.GetComponentInChildren<Text>().text == "Not Ready")
                    readyButton.GetComponentInChildren<Text>().text = "Ready";
                else
                    readyButton.GetComponentInChildren<Text>().text = "Not Ready";
                
                foreach (int i in ClientData.players.Keys)
                {
                    Player player =  ClientData.players[i];
                    if (player != null)
                    {
                        if (player.id == Client.Instance.Id)
                        {
                            player.isReady = !player.isReady;
                            ClientSend.SendReadyState(player.isReady, i);
                        }
                    }
                }
            };
            readyButton.OnClick.AddListener(readyAction);
            readyObject.SetActive(true);
            
            
            var backObject = Object.Instantiate(template);
            RectTransform backTransform =  backObject.GetComponent<RectTransform>();
            MainMenuButton backButton = backObject.GetComponent<MainMenuButton>();

            backTransform.parent = parent;
            backTransform.parentInternal = CustomMainMenu.templateObject.GetComponent<RectTransform>().parentInternal;
            backButton.Y = 28;
            backButton.OnMouseHover = CustomMainMenu.templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[28] = backButton;

            backTransform.anchoredPosition = new Vector2(-5, -200);
            backTransform.sizeDelta = new Vector2(288, 55);
            backButton.GetComponentInChildren<Text>().text = "Back to saves";
            backButton.OnClick = new MainMenuButton.ButtonEvent();
            Action backtoMenu = delegate
            {
                DisableLobby(); 
                CustomHostMenu.CreateSavesMenu();
            };
            backButton.OnClick.AddListener(backtoMenu);
            backObject.SetActive(true);
        }


        public static void AddPlayerToLobby(string username, int id)
        {
            
        }


        private static void DisableLobby()
        {
            CustomMainMenu.section.buttons[26].gameObject.SetActive(false);
            CustomMainMenu.section.buttons[27].gameObject.SetActive(false);
            CustomMainMenu.section.buttons[28].gameObject.SetActive(false);
            
            Server.Stop();
            if (!ModSceneManager.isInMenu())
            {
                NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
            }
        }
        
        private static void StartGame(int _saveIndex)
        {
            SavesManager.StartGame(_saveIndex);
            ServerSend.StartGame();
        }
    }
}