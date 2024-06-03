using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Handle;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.UI.Logic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomLobbyMenu
    {

        public static int  saveIndex;
        public static bool isSet;

        public static List<GameObject> backgrounds = new List<GameObject>();
        public static List<GameObject> usernameText = new List<GameObject>();
        public static List<GameObject> readyText = new List<GameObject>();
        public static List<GameObject> kickButtons = new List<GameObject>();
        public static List<GameObject> lobbyHeader = new List<GameObject>();
        
        public static void OpenLobby(bool client=false)
        {
            if (isSet)
            {
                for (int i = 0; i < CustomUIManager.saveButtons.Count; i++)
                {
                    if(CustomUIManager.saveButtons[i] != null)
                        CustomUIManager.saveButtons[i].gameObject.SetActive(false);
                }
                for (int i = 0; i < CustomUIManager.hostMenuButtons.Count; i++)
                {
                    if(CustomUIManager.hostMenuButtons[i] != null)
                        CustomUIManager.hostMenuButtons[i].gameObject.SetActive(false);
                }
                for (int i = 0; i < CustomUIManager.lobbyMenuButtons.Count; i++)
                {
                    if (CustomUIManager.lobbyMenuButtons[i] != null)
                    {
                        CustomUIManager.lobbyMenuButtons[i].gameObject.SetActive(true);
                        if (i == 0 && client)
                        {
                            CustomMainMenu.section.buttons[i].isDisabled = true;
                            CustomMainMenu.section.buttons[i].DoStateTransition(SelectionState.Disabled, true);
                        }
                    }
                }

                CreateLobbyDisplay(client);
                
                CustomUIManager.inLobbyWindow = true;
            }
            else
            {
                CustomHostMenu.DisableSavesMenu();
                CreateLobbyMenu(client);
                CustomUIManager.inLobbyWindow = true;
            }
        }

        public static void CreateLobbyMenu(bool client=false)
        {
            GameObject template = CustomMainMenu.templateButtonObject;
            isSet = true;

            if (client)
            {
                for (int i = 0; i < CustomUIManager.multiplayerMenuButtons.Count; i++)
                {
                    if(CustomUIManager.multiplayerMenuButtons[i] != null)
                        CustomUIManager.multiplayerMenuButtons[i].gameObject.SetActive(false);
                }
            }
            
            for (int i = 0; i < CustomUIManager.saveButtons.Count; i++)
            {
                if(CustomUIManager.saveButtons[i] != null)
                    CustomUIManager.saveButtons[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < CustomUIManager.hostMenuButtons.Count; i++)
            {
                if(CustomUIManager.hostMenuButtons[i] != null)
                    CustomUIManager.hostMenuButtons[i].gameObject.SetActive(false);
            }
            
            Transform parent = template.transform;
            
            CreateLobbyDisplay(client);
            
            var startObject = Object.Instantiate(template);
            RectTransform  startTransform =  startObject.GetComponent<RectTransform>();
            MainMenuButton  startButton = startObject.GetComponent<MainMenuButton>();

            startTransform.parent = parent;
            startTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            startButton.Y = 26;
            /*startButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;*/
            CustomUIManager.lobbyMenuButtons.Add(startButton);

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

                MelonCoroutines.Start(StartGame());
            };
            startButton.OnClick.AddListener(startAction);
            startObject.SetActive(true);

            if (client)
            {
                startButton.isDisabled = true;
                startButton.DoStateTransition(SelectionState.Disabled, true);
            }
            
            
            var readyObject = Object.Instantiate(template);
            RectTransform  readyTransform =  readyObject.GetComponent<RectTransform>();
            MainMenuButton  readyButton = readyObject.GetComponent<MainMenuButton>();

            readyTransform.parent = parent;
            readyTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            readyButton.Y = 27;
            /*readyButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;*/
            CustomUIManager.lobbyMenuButtons.Add(readyButton);

            readyTransform.anchoredPosition = new Vector2(-5, 0);
            readyTransform.sizeDelta = new Vector2(288, 55);
            readyButton.GetComponentInChildren<Text>().text = "Toggle Ready";
            readyButton.OnClick = new MainMenuButton.ButtonEvent();
            Action  readyAction = delegate
            {
                foreach (int i in ClientData.Instance.players.Keys)
                {
                    Player player =  ClientData.Instance.players[i];
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
            backTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            backButton.Y = 28;
            /*backButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;*/
            CustomUIManager.lobbyMenuButtons.Add(backButton);

            backTransform.anchoredPosition = new Vector2(-5, -200);
            backTransform.sizeDelta = new Vector2(288, 55);
            if (client)
            {
                backButton.GetComponentInChildren<Text>().text = "Disconnect";
                backButton.OnClick = new MainMenuButton.ButtonEvent();
                Action backtoMenu = delegate
                {
                    DisableLobby(); 
                    CustomMainMenu.EnableMultiplayerMenu();
                };
                backButton.OnClick.AddListener(backtoMenu);
            }
            else
            {
                backButton.GetComponentInChildren<Text>().text = "Back to saves";
                backButton.OnClick = new MainMenuButton.ButtonEvent();
                Action backtoMenu = delegate
                {
                    DisableLobby(true); 
                    CustomHostMenu.CreateSavesMenu();
                };
                backButton.OnClick.AddListener(backtoMenu);
            }
            backObject.SetActive(true);
        }

        private static IEnumerator StartGame()
        {
            //ServerSend.SendCarLoadInfo(Client.Instance.Id);
            yield return new WaitForEndOfFrame();
            
            StartGame(saveIndex);
            SavesManager.ModSaves[saveIndex].alreadyLoaded = true;
            SavesManager.SaveModSave(saveIndex);
        }

        public static void CreateLobbyDisplay(bool client=false)
        {
            backgrounds = new List<GameObject>();
            readyText = new List<GameObject>();
            usernameText = new List<GameObject>();
            lobbyHeader = new List<GameObject>();
            kickButtons = new List<GameObject>();
            
            
            Transform textParent = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parent;
            
            
            float startYPosition = 100f;
            float rowHeight = 60f; 
            
            Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
            
            GameObject backgroundPanel2Object = new GameObject("BackgroundPanel");
            RectTransform backgroundPanel2Transform = backgroundPanel2Object.AddComponent<RectTransform>();
            Image backgroundPanel2Image = backgroundPanel2Object.AddComponent<Image>();
            
            backgroundPanel2Transform.parent = textParent;
                
            backgroundPanel2Transform.anchoredPosition = new Vector2(385, 150); 
            backgroundPanel2Transform.localScale = new Vector3(3.4f,0.4f,1); 
                
            backgroundPanel2Image.color = backgroundColor;
            
            var textObject = Object.Instantiate(CustomMainMenu.templateTextObject); 
            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            Text textComponent = textObject.GetComponent<Text>();
            
            textTransform.parent = textParent; 
            
            textTransform.anchoredPosition = new Vector2(430, 150); 
            textTransform.sizeDelta = new Vector2(288, 55); 
            
            textComponent.text = "Usernames :"; 
            textObject.SetActive(true);
            
            var text2Object = Object.Instantiate(CustomMainMenu.templateTextObject); 
            RectTransform text2Transform = text2Object.GetComponent<RectTransform>();
            Text text2Component = text2Object.GetComponent<Text>();
            
            text2Transform.parent = textParent;
            
            text2Transform.anchoredPosition = new Vector2(550, 150); 
            text2Transform.sizeDelta = new Vector2(288, 55);
            
            text2Component.text = "Ready State :"; 
            text2Object.SetActive(true);
            
            
            lobbyHeader.Add(textObject);
            lobbyHeader.Add(text2Object);
            lobbyHeader.Add(backgroundPanel2Object);
            
            
            for (int i = 0; i < 4; i++)
            {
                GameObject backgroundPanelObject = new GameObject("BackgroundPanel");
                RectTransform backgroundPanelTransform = backgroundPanelObject.AddComponent<RectTransform>();
                Image backgroundPanelImage = backgroundPanelObject.AddComponent<Image>();
                
                backgroundPanelTransform.parent = textParent;
                
                backgroundPanelTransform.anchoredPosition = new Vector2(385, startYPosition - (i * rowHeight)); 
                backgroundPanelTransform.localScale = new Vector3(3.4f,0.6f,1); 
                
                backgroundPanelImage.color = backgroundColor;
                
                backgroundPanelObject.SetActive(false);
                backgrounds.Add(backgroundPanelObject);
                
                
                GameObject pseudoTextObject = Object.Instantiate(CustomMainMenu.templateTextObject);
                RectTransform pseudoTextTransform = pseudoTextObject.GetComponent<RectTransform>();
                Text pseudoTextComponent = pseudoTextObject.GetComponent<Text>();
                
                pseudoTextTransform.parent = textParent;
                
                pseudoTextTransform.anchoredPosition = new Vector2(358, startYPosition - (i * rowHeight));
                pseudoTextTransform.sizeDelta = new Vector2(100, 30); 

                pseudoTextComponent.text = "player.username"; 

                pseudoTextObject.SetActive(true);
                
                usernameText.Add(pseudoTextObject);
                
                GameObject readyTextObject = Object.Instantiate(CustomMainMenu.templateTextObject);
                RectTransform readyTextTransform = readyTextObject.GetComponent<RectTransform>();
                Text readyTextComponent = readyTextObject.GetComponent<Text>();
                
                readyTextTransform.parent = textParent;
                
                readyTextTransform.anchoredPosition = new Vector2(500, startYPosition - (i * rowHeight));
                readyTextTransform.sizeDelta = new Vector2(100, 30); 
                
                readyTextComponent.text = false ? "Ready" : "Not Ready"; 
                readyTextObject.SetActive(true);
                
                readyText.Add(readyTextObject);

                // Création du bouton pour "Kick"
                GameObject kickButtonObject = Object.Instantiate(CustomMainMenu.templateButtonObject); 
                RectTransform kickButtonTransform = kickButtonObject.GetComponent<RectTransform>();
                MainMenuButton kickButtonComponent = kickButtonObject.GetComponent<MainMenuButton>();
                
                kickButtonTransform.parent = textParent;
                
                kickButtonTransform.anchoredPosition = new Vector2(510, startYPosition - (i * rowHeight));
                kickButtonTransform.sizeDelta = new Vector2(80, 30);
                
                kickButtonComponent.GetComponentInChildren<Text>().text = "Kick"; 
                kickButtonObject.SetActive(true);
                kickButtons.Add(kickButtonObject);
                
                if (client)
                {
                    kickButtonComponent.isDisabled = true;
                    kickButtonComponent.DoStateTransition(SelectionState.Disabled, true);
                }
            }
        }

        

        public static void DisableLobby(bool host=false)
        {
            for (int i = 0; i < CustomUIManager.lobbyMenuButtons.Count; i++)
            {
                CustomUIManager.lobbyMenuButtons[i].gameObject.SetActive(false);
            }
            if (!host)
            {
                for (int i = 0; i < CustomUIManager.hostMenuButtons.Count; i++)
                {
                    CustomUIManager.hostMenuButtons[i].gameObject.SetActive(true);
                }
            }

            try
            {
                for (int i = 0; i < backgrounds.Count; i++)
                {
                    Object.Destroy(backgrounds[i].gameObject);
                    Object.Destroy(usernameText[i].gameObject);
                    Object.Destroy(readyText[i].gameObject);
                    Object.Destroy(kickButtons[i].gameObject);
                }
                for (int i = 0; i < lobbyHeader.Count; i++)
                {
                    Object.Destroy(lobbyHeader[i]);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Error while clearing Lobby UI : " + e);
            }

            
            Server.Stop();
            if (!ModSceneManager.isInMenu())
            {
                NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
            }
            
            CustomUIManager.inLobbyWindow = false;
        }
        
        private static void StartGame(int _saveIndex)
        {
            SavesManager.StartGame(_saveIndex);
            ServerSend.StartGame(null);
        }
    }
}