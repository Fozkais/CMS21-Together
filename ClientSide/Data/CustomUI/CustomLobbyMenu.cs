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

        public static List<GameObject> backgrounds = new List<GameObject>();
        public static List<GameObject> usernameText = new List<GameObject>();
        public static List<GameObject> readyText = new List<GameObject>();
        public static List<GameObject> kickButtons = new List<GameObject>();
        
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

                CustomUIManager.InLobbyWindow = true;
            }
            else
            {
                CustomHostMenu.DisableSavesMenu();
                CreateLobbyMenu();
                CustomUIManager.InLobbyWindow = true;
            }
        }

        public static void CreateLobbyMenu()
        {
            GameObject template = CustomMainMenu.templateButtonObject;
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
            Transform textParent = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parent;
            
            
            float startYPosition = 100f; // Vous pouvez ajuster cette valeur selon votre interface
            float rowHeight = 60f; // Vous pouvez ajuster cette valeur selon votre interface
            
            Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
            
            GameObject backgroundPanel2Object = new GameObject("BackgroundPanel");
            RectTransform backgroundPanel2Transform = backgroundPanel2Object.AddComponent<RectTransform>();
            Image backgroundPanel2Image = backgroundPanel2Object.AddComponent<Image>();

            // Définir le parent du panneau rectangulaire
            backgroundPanel2Transform.parent = textParent;
                
            backgroundPanel2Transform.anchoredPosition = new Vector2(385, 150); // Position en Y en fonction de l'index
            backgroundPanel2Transform.localScale = new Vector3(3.4f,0.4f,1); // Échelle par défaut
                
            backgroundPanel2Image.color = backgroundColor;
            
            var textObject = Object.Instantiate(CustomMainMenu.templateTextObject); // Assurez-vous que textTemplate soit un GameObject contenant un composant Text
            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            Text textComponent = textObject.GetComponent<Text>();

// Définir le parent de l'objet texte
            textTransform.parent = textParent; // 'parent' est le parent souhaité pour le texte

// Définir la position et la taille du texte
            textTransform.anchoredPosition = new Vector2(430, 150); // Définir les coordonnées x et y souhaitées
            textTransform.sizeDelta = new Vector2(288, 55); // Définir la largeur et la hauteur souhaitées

// Définir le contenu du texte
            textComponent.text = "Usernames :"; // Modifier "Votre texte ici" selon le texte souhaité
// Activer l'objet texte
            textObject.SetActive(true);
            
            var text2Object = Object.Instantiate(CustomMainMenu.templateTextObject); // Assurez-vous que textTemplate soit un GameObject contenant un composant Text
            RectTransform text2Transform = text2Object.GetComponent<RectTransform>();
            Text text2Component = text2Object.GetComponent<Text>();
            
            text2Transform.parent = textParent; // 'parent' est le parent souhaité pour le texte
            
            text2Transform.anchoredPosition = new Vector2(550, 150); // Définir les coordonnées x et y souhaitées
            text2Transform.sizeDelta = new Vector2(288, 55); // Définir la largeur et la hauteur souhaitées
            
            text2Component.text = "Ready State :"; 
            text2Object.SetActive(true);
            
            
            for (int i = 0; i < 4; i++)
            {
                // Création du panneau rectangulaire pour le fond gris semi-transparent
                GameObject backgroundPanelObject = new GameObject("BackgroundPanel");
                RectTransform backgroundPanelTransform = backgroundPanelObject.AddComponent<RectTransform>();
                Image backgroundPanelImage = backgroundPanelObject.AddComponent<Image>();

                // Définir le parent du panneau rectangulaire
                backgroundPanelTransform.parent = textParent;
                
                backgroundPanelTransform.anchoredPosition = new Vector2(385, startYPosition - (i * rowHeight)); // Position en Y en fonction de l'index
                backgroundPanelTransform.localScale = new Vector3(3.4f,0.6f,1); // Échelle par défaut
                
                backgroundPanelImage.color = backgroundColor;
                
                backgrounds.Add(backgroundPanelObject);
                
                
                GameObject pseudoTextObject = Object.Instantiate(CustomMainMenu.templateTextObject);
                RectTransform pseudoTextTransform = pseudoTextObject.GetComponent<RectTransform>();
                Text pseudoTextComponent = pseudoTextObject.GetComponent<Text>();
                
                pseudoTextTransform.parent = textParent;
                
                pseudoTextTransform.anchoredPosition = new Vector2(358, startYPosition - (i * rowHeight));
                pseudoTextTransform.sizeDelta = new Vector2(100, 30); // Ajustez la taille en conséquence

                pseudoTextComponent.text = "player.username"; 

                pseudoTextObject.SetActive(true);
                
                usernameText.Add(pseudoTextObject);
                
                GameObject readyTextObject = Object.Instantiate(CustomMainMenu.templateTextObject);
                RectTransform readyTextTransform = readyTextObject.GetComponent<RectTransform>();
                Text readyTextComponent = readyTextObject.GetComponent<Text>();
                
                readyTextTransform.parent = textParent;
                
                readyTextTransform.anchoredPosition = new Vector2(500, startYPosition - (i * rowHeight));
                readyTextTransform.sizeDelta = new Vector2(100, 30); // Ajustez la taille en conséquence
                
                readyTextComponent.text = false ? "Ready" : "Not Ready"; // Supposons que chaque PlayerData a une propriété 'ready' indiquant l'état de disponibilité
                readyTextObject.SetActive(true);
                
                readyText.Add(readyTextObject);

                // Création du bouton pour "Kick"
                GameObject kickButtonObject = Object.Instantiate(template); // Supposons que CustomMainMenu.templateButtonObject soit un bouton préfabriqué
                RectTransform kickButtonTransform = kickButtonObject.GetComponent<RectTransform>();
                MainMenuButton kickButtonComponent = kickButtonObject.GetComponent<MainMenuButton>();
                
                kickButtonTransform.parent = textParent;
                
                kickButtonTransform.anchoredPosition = new Vector2(510, startYPosition - (i * rowHeight));
                kickButtonTransform.sizeDelta = new Vector2(80, 30); // Ajustez la taille en conséquence
                
                kickButtonComponent.GetComponentInChildren<Text>().text = "Kick"; // Vous pouvez ajuster le texte du bouton selon vos besoins
                /*Action  kickAction = delegate 
                {
                    ServerSend.DisconnectClient(player.id, "You've been kicked from server!");
                    Server.clients[player.id].Disconnect(player.id);
                };
                kickButtonComponent.OnClick.AddListener(kickAction);*/
                kickButtonObject.SetActive(true);
                
                kickButtons.Add(kickButtonObject);

                // Ajoutez ici le gestionnaire d'événements pour le bouton "Kick" si nécessaire
            }

            
            
            var startObject = Object.Instantiate(template);
            RectTransform  startTransform =  startObject.GetComponent<RectTransform>();
            MainMenuButton  startButton = startObject.GetComponent<MainMenuButton>();

            startTransform.parent = parent;
            startTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            startButton.Y = 26;
            startButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
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
            readyTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            readyButton.Y = 27;
            readyButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[27] = readyButton;

            readyTransform.anchoredPosition = new Vector2(-5, 0);
            readyTransform.sizeDelta = new Vector2(288, 55);
            readyButton.GetComponentInChildren<Text>().text = "Toggle Ready";
            readyButton.OnClick = new MainMenuButton.ButtonEvent();
            Action  readyAction = delegate
            {
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
            backTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            backButton.Y = 28;
            backButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
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
            
            CustomUIManager.InLobbyWindow = false;
        }
        
        private static void StartGame(int _saveIndex)
        {
            SavesManager.StartGame(_saveIndex);
            ServerSend.StartGame();
        }
    }
}