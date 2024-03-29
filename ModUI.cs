﻿using System;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Data;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.ServerSide;
using CMS21MP.ServerSide.DataHandle;
using CMS21MP.SharedData;
using MelonLoader;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CMS21MP
{
    public class ModUI : MonoBehaviour
    {
        public static ModUI Instance;
        public string username = "player";
        private string save_name = "save1";
        public string ipAddress = "127.0.0.1";

        public bool showMainInterface;
        public bool showHostInterface;
        public bool saveExists; // Example: save files exist

        private int saveToLoadIndex;

        private GUIStyle labelStyle;
        private GUIStyle textStyle;
        private GUIStyle buttonStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle backgroundStyle;

        private bool isDragging;
        private Vector2 offset;
        private Rect windowRect = new Rect(Screen.width / 2.5f, Screen.height / 3.5f, 200, 280);
        private Vector2 hostInterfaceOffset = new Vector2(220, 0);
        
        private Texture2D backgroundTexture;

        private Vector2 scrollPosition = Vector2.zero; // Variable de classe pour stocker la position de défilement

        private float buttonHeight = 20f; // Hauteur des boutons
        private float maxScrollViewHeight = 121f; // Hauteur maximale du ScrollView

        private bool showLobbyInterface;

        public void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
            }
        }

        private void UI_Utils()
        {
            // Capture les événements de la souris
            Event currentEvent = Event.current;
            Vector2 mousePosition = currentEvent.mousePosition;

            if (currentEvent.type == EventType.MouseDown && windowRect.Contains(mousePosition))
            {
                // Capture le début du déplacement de l'interface
                isDragging = true;
                offset = mousePosition - windowRect.position;
                GUI.FocusWindow(0);
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                // Capture la fin du déplacement de l'interface
                isDragging = false;
            }
            else if (currentEvent.type == EventType.MouseDrag && isDragging)
            {
                // Met à jour la position de l'interface en fonction du déplacement de la souris
                windowRect.position = mousePosition - offset;
            }

            // Rendu de l'interface principale et de l'interface de l'hôte
            InitializeStyles();
        }

        public void ShowMainInterface()
        {
            showHostInterface = false;
            showLobbyInterface = false;
            showMainInterface = true;
        }

        public void OnMPGUI()
        {
            UI_Utils();
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                if(!showLobbyInterface && !showHostInterface)
                {
                    if (showMainInterface)
                    {
                        RenderMainInterface();
                    }
                }

                if (!showHostInterface && !showMainInterface)
                {
                    if (showLobbyInterface)
                    {
                        RenderLobbyInterface();
                    }
                }

                if (!showMainInterface && !showLobbyInterface)
                {
                    if (showHostInterface)
                    {
                        Rect hostWindowRect = new Rect(windowRect.position + hostInterfaceOffset, new Vector2(220, 300));
                        RenderHostInterface(hostWindowRect);
                    }
                }
            }
            else
            {
                if (showMainInterface)
                {
                    RenderLobbyInterface();
                }
            }


            /*f (SceneManager.GetActiveScene().name == "Menu")
            {
                if (!MainMod.isServer || !Client.Instance.isConnected)
                {
                    if (showMainInterface)
                    {
                        RenderMainInterface();
                    }

                    // Rendu de l'interface de l'hôte attachée à l'interface principale
                    if (showHostInterface)
                    {
                        // Met à jour la position de l'interface de l'hôte en fonction de l'interface principale
                        Rect hostWindowRect = new Rect(windowRect.position + hostInterfaceOffset, new Vector2(220, 300));
                        // Rendu de l'interface de l'hôte
                        RenderHostInterface(hostWindowRect);
                    }
                }
                else
                {
                    if (showLobbyInterface)
                    {
                        RenderLobbyInterface();
                    }
                }
            }
            else
            {
                if (MainMod.isServer || Client.Instance.isConnected)
                {
                    if (showMainInterface)
                    {
                        RenderLobbyInterface();
                    }
                }
            }*/

        }

        private Texture2D CreateRoundedBackgroundTexture(Color color, float alpha, float cornerRadius)
        {
            int width = 128; // Largeur de la texture
            int height = 128; // Hauteur de la texture

            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            // Remplir les pixels de la texture avec la couleur et l'alpha spécifiés
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(color.r, color.g, color.b, alpha);
            }

            // Dessiner les coins arrondis
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Vérifier si le pixel est à l'intérieur du coin arrondi
                    if ((x < cornerRadius && y < cornerRadius) || (x >= width - cornerRadius && y < cornerRadius) ||
                        (x < cornerRadius && y >= height - cornerRadius) ||
                        (x >= width - cornerRadius && y >= height - cornerRadius))
                    {
                        // Vérifier si le pixel est à l'extérieur du rayon du coin arrondi
                        if (new Vector2(cornerRadius - x, cornerRadius - y).magnitude > cornerRadius)
                        {
                            // Mettre le pixel en transparent
                            pixels[y * width + x] = new Color(color.r, color.g, color.b, 0f);
                        }
                    }
                }
            }

            // Appliquer les pixels à la texture
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        private void InitializeStyles()
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                //margin = new RectOffset(10, 5, 5, 5),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                // margin = new RectOffset(10, 5, 5, 5),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                // margin = new RectOffset(10, 5, 5, 5),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };

            if(backgroundTexture == null)
                backgroundTexture = CreateRoundedBackgroundTexture(Color.black, 0.5f, 2f); 
            
            backgroundStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = backgroundTexture }, // Utilisez votre texture de fond avec des coins arrondis
            };
        }

        private void RenderMainInterface()
        {
            GUI.Box(new Rect(windowRect.x - 5, windowRect.y, windowRect.width, windowRect.height), "", backgroundStyle);
            GUILayout.BeginArea(windowRect);

            GUILayout.Label(MainMod.MOD_VERSION, labelStyle, GUILayout.Width(190), GUILayout.Height(40));

            if (!MainMod.usingSteamAPI)
            {
                GUILayout.Label("Username:", textStyle);
                ModUI.Instance.username = GUILayout.TextField(ModUI.Instance.username, textFieldStyle, GUILayout.Width(190));
                
                GUILayout.Label("IP Address:", textStyle);
                ModUI.Instance.ipAddress = GUILayout.TextField(ModUI.Instance.ipAddress, textFieldStyle, GUILayout.Width(190));
                
            }
            else
            {
                GUILayout.Label("Lobby ID:", textStyle);
            }

            GUILayout.Space(10);
            
            /*if (GUILayout.Button("Use steam?", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                MainMod.usingSteamAPI = !MainMod.usingSteamAPI; TODO: TO RE-ENABLE WHEN STEAM API IS WORKING
            }*/
            

            if (GUILayout.Button("Join Lobby", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                JoinLobby();
                ShowLobbyInterface();
            }

            if (GUILayout.Button("Host Game", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                showHostInterface = true;
                showMainInterface = false;
                PreferencesManager.LoadAllModSaves();
            }

            GUILayout.EndArea();

            PreferencesManager.SavePreferences();
        }

        private void RenderHostInterface(Rect hostWindowRect)
        {
            GUI.Box(new Rect(hostWindowRect.x - 5, hostWindowRect.y, hostWindowRect.width, hostWindowRect.height), "",
                backgroundStyle);
            GUILayout.BeginArea(hostWindowRect);

            GUILayout.Label("Save Management", labelStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                showHostInterface = false;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("New Game", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                bool isNameAlreadyExists = SaveSystem.ModSaves.Any(save => save.Value.Name == save_name);

                if (isNameAlreadyExists)
                {
                    Debug.Log("Save with the same name already exists.");
                }
                else
                {
                    for (int index = 4; index < SaveSystem.ModSaves.Count; index++)
                    {
                        var save = SaveSystem.ModSaves[index];
                        if (save.Name == "EmptySave")
                        {
                            SaveSystem.LoadSave(index, save_name);
                            break;
                        }
                    }
                }
            }

            GUILayout.Space(5);

            GUILayout.Label("Save Name:", textStyle);
            save_name = GUILayout.TextField(save_name, textFieldStyle, GUILayout.Width(190));

            GUILayout.Space(10);

            if (!SaveSystem.ModSaves.Any(save => save.Value.Name != "EmptySave"))
            {
                GUILayout.Label("No Save", labelStyle);
            }
            else
            {
               DisplaySaves();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndArea();
        }

        public void DisplaySaves()
        {
             float scrollViewHeight = Mathf.Min(SaveSystem.ModSaves.Count * (buttonHeight + 20), maxScrollViewHeight);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(220),
                    GUILayout.Height(scrollViewHeight));

                foreach (var data in SaveSystem.ModSaves)
                {
                    int saveIndex = data.Value.saveIndex;
                    ModSaveData modSaveData = data.Value;

                    if (modSaveData.Name != "EmptySave")
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(buttonHeight)))
                        {
                            // Actions à effectuer lors du clic sur le bouton de suppression
                            SaveSystem.RemoveSave(saveIndex);
                            break;
                        }

                        if (GUILayout.Button(modSaveData.Name, buttonStyle, GUILayout.Width(160), GUILayout.Height(buttonHeight)))
                        {
                            
                            if(!MainMod.isServer)
                                Server.Start();
                            else
                            {
                                Server.Stop();
                                Server.Start();
                            }
                            ShowLobbyInterface();
                            
                            saveToLoadIndex = SaveSystem.LoadSave(saveIndex, modSaveData.Name);

                        }

                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndScrollView();
        }


        private void RenderLobbyInterface()
        {
            Rect lobbyWindowRect;
            
            if (MainMod.isServer || Client.Instance.isConnected)
                lobbyWindowRect = new Rect(Screen.width / 2f - 100, Screen.height / 2f - 100, 400, 400);
            else
                lobbyWindowRect = windowRect;
            
            GUI.Box(new Rect(lobbyWindowRect.x - 5, lobbyWindowRect.y, lobbyWindowRect.width + 5, lobbyWindowRect.height), "", backgroundStyle);
            
            GUILayout.BeginArea(lobbyWindowRect);

            GUILayout.Label("Lobby", labelStyle);
            
            // Partie haute avec 3 colonnes
            GUILayout.BeginHorizontal();

            if (MainMod.isServer)
            {
                
                // Colonne 1 - Noms
                GUILayout.BeginVertical(GUILayout.Width(70));
                foreach (int i in Server.clients.Keys)
                {
                    Player player = Server.clients[i].player;
                    if (player != null)
                    {
                        GUILayout.Label("   " + player.username, textStyle);
                        GUILayout.Space(5);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Colonne 2 - États  
                GUILayout.BeginVertical(GUILayout.Width(90));
                foreach (int i in Server.clients.Keys)
                {
                    Player player = Server.clients[i].player;

                    if (player != null)
                    {
                        if (player.isReady)
                        {
                            GUILayout.Label("Ready", textStyle);
                        }
                        else
                        {
                            GUILayout.Label("Not Ready", textStyle);
                        }
                        GUILayout.Space(25);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Colonne 3 - Interactions
                GUILayout.BeginVertical(GUILayout.Width(100));
                foreach (int i in Server.clients.Keys)
                {
                    Player player = Server.clients[i].player;
                    if (player != null)
                    {
                        if (MainMod.isServer && player.id != 1) 
                        {  
                            GUILayout.Space(25);
                            if (GUILayout.Button("Kick", buttonStyle)) 
                            {
                                ServerSend.DisconnectClient(i, "You've been kicked from server!");
                                Server.clients[i].Disconnect(Server.clients[i].id);
                            }
                        }

                        if (Server.clients[i].id == Client.Instance.Id)
                        {
                            if (GUILayout.Button("Ready", buttonStyle)) 
                            {
                                player.isReady = !player.isReady;
                                ClientSend.SendReadyState(player.isReady, i);
                            }
                        }
                        
                    }
                } 
            }
            else
            { 
                // Colonne 1 - Noms
                GUILayout.BeginVertical(GUILayout.Width(70));
                foreach (int i in ClientData.serverPlayers.Keys)
                {
                    Player player =  ClientData.serverPlayers[i];
                    if (player != null)
                    {
                        GUILayout.Label("   " + player.username, textStyle);
                        GUILayout.Space(5);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Colonne 2 - États  
                GUILayout.BeginVertical(GUILayout.Width(90));
                foreach (int i in ClientData.serverPlayers.Keys)
                {
                    Player player =  ClientData.serverPlayers[i];

                    if (player != null)
                    {
                        if (player.isReady)
                        {
                            GUILayout.Label("Ready", textStyle);
                        }
                        else
                        {
                            GUILayout.Label("Not Ready", textStyle);
                        }
                        GUILayout.Space(25);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Colonne 3 - Interactions
                GUILayout.BeginVertical(GUILayout.Width(100));
                foreach (int i in ClientData.serverPlayers.Keys)
                {
                    Player player =  ClientData.serverPlayers[i];
                    if (player != null)
                    {
                        if (ClientData.serverPlayers[i].id == Client.Instance.Id)
                        {
                            GUILayout.Space((i * 25));
                            if (GUILayout.Button("Ready", buttonStyle)) 
                            {
                                player.isReady = !player.isReady;
                                ClientSend.SendReadyState(player.isReady, i);
                            }
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace(); // Pousse les éléments vers le haut

            GUILayout.BeginHorizontal();

            if (MainMod.isServer)
            {
                if(GUILayout.Button("Start Game", buttonStyle, GUILayout.Width(175), GUILayout.Height(35))) 
                {
                    foreach (var client in Server.clients)
                    {
                        if (client.Value.player != null && !client.Value.player.isReady)
                        {
                            return;
                        }
                    }

                    StartGame(saveToLoadIndex - 1);
                    SaveSystem.ModSaves[saveToLoadIndex-1].alreadyLoaded = true;
                    PreferencesManager.SaveModSave(saveToLoadIndex-1);
                    showLobbyInterface = false;
                }
                if(GUILayout.Button("Close Server", buttonStyle, GUILayout.Width(175), GUILayout.Height(35))) 
                {
                    showLobbyInterface = false;
                    showHostInterface = false;
                    showMainInterface = true;

                    Server.Stop();

                }
                
            }
            else
            {
                if(GUILayout.Button("Disconnect", buttonStyle, GUILayout.Width(100), GUILayout.Height(35))) 
                {
                    showLobbyInterface = false;
                    showHostInterface = false;
                    showMainInterface = true;

                    Client.Instance.Disconnect();

                }
            }
            
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        public void JoinLobby()
        {
             if (!string.IsNullOrEmpty(ModUI.Instance.username) && !string.IsNullOrEmpty(ModUI.Instance.ipAddress))
            {
                Client.Instance.ConnectToServer(ModUI.Instance.ipAddress);
                ShowLobbyInterface();
                Application.runInBackground = true;
            }
        }

        public void ShowLobbyInterface()
        {
            showMainInterface = false;
            showHostInterface = false;
            showLobbyInterface = true;
        }
        
        private void StartGame(int index)
        {
            showMainInterface = false;
            showLobbyInterface = false;
            SaveSystem.StartGame(index);
            ServerSend.StartGame();
        }

        public void showGui()
        {
            if(Input.GetKeyDown(MainMod.MOD_GUI_KEY))
                showMainInterface = !showMainInterface;
        }
    }
}
