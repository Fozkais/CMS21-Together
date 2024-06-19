using System;
using System.Linq;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared.Data;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ServerSide;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Shared
{
    [RegisterTypeInIl2Cpp]
    public class ModUI : MonoBehaviour
    {
        public static ModUI Instance;

        public bool showModUI;
        public guiWindow window;
        private GUIStyle label_S, text_S, button_S, textField_S;
        
        private float buttonHeight = 20f;
        private float maxScrollViewHeight = 121f; 
        private Vector2 scrollPosition = Vector2.zero;
        
        private Rect mRect = new Rect(Screen.width / 2.5f, Screen.height / 3.5f, 315, 280);
        
        private string saveName = "save";
        private int saveIndex;
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
            window = guiWindow.main;
        }

        public void OnGUI()
        {
            if (showModUI)
            {
                Initialize();
                if (ModSceneManager.isInMenu())
                {
                    switch (window)
                    {
                        case guiWindow.main:
                            RenderMain();
                            break;
                        case guiWindow.host:
                            RenderHost();
                            break;
                        case guiWindow.lobby:
                            RenderLobby();
                            break;
                    }
                }
                else
                {
                    RenderLobby();
                }
            }
        }

        private void Initialize()
        {
            label_S = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            text_S = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            button_S = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            textField_S = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };
        }
        
        private void RenderMain()
        {
            GUILayout.BeginArea(mRect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(MainMod.MOD_VERSION, label_S, GUILayout.Width(190), GUILayout.Height(40));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Username:", text_S);
            Client.Instance.username = GUILayout.TextField(Client.Instance.username, textField_S, GUILayout.Width(190));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
    
            GUILayout.Label("IP Address:", text_S);
            Client.Instance.ip = GUILayout.TextField(Client.Instance.ip, textField_S, GUILayout.Width(190));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Join Lobby", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                JoinLobby();
                window = guiWindow.lobby;
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Host Game", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                window = guiWindow.host;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
    
            GUILayout.EndArea();
            
            PreferencesManager.SavePreferences();
        }

        private void RenderHost()
        {
            GUILayout.BeginArea(mRect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Saves Management", label_S);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Save Name:", text_S);
            saveName = GUILayout.TextField(saveName, textField_S, GUILayout.Width(190));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create Game", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                bool alreadyExist = SavesManager.ModSaves.Any(save => save.Value.Name == saveName);
                if(alreadyExist) { MelonLogger.Msg("A save with the same name already exists."); }
                else
                {
                    for (int index = 4; index < SavesManager.ModSaves.Count; index++)
                    {
                        
                        var save = SavesManager.ModSaves[index];
                        if (save.Name == "EmptySave")
                        {
                            MelonLogger.Msg("Found a valid save slot!: " + index);
                            SavesManager.LoadSave(save);
                            break;
                        }
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Cancel", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                window = guiWindow.main;
                
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (!SavesManager.ModSaves.Any(save => save.Value.Name != "EmptySave"))
            {
                GUILayout.Label("No Existing Save.", label_S);
            }
            else
            {
                DisplaySaves();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

        }
        private void DisplaySaves()
        {
            float scrollViewHeight = Mathf.Min(SavesManager.ModSaves.Count * (buttonHeight + 20), maxScrollViewHeight);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(220), 
                GUILayout.Height(scrollViewHeight));
            
            GUILayout.FlexibleSpace();

            foreach (var data in SavesManager.ModSaves.Values)
            {
                if (data.Name != "EmptySave")
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(buttonHeight)))
                    {
                        // Actions à effectuer lors du clic sur le bouton de suppression
                        SavesManager.RemoveModSave(data.saveIndex);
                        break;
                    }
                    if (GUILayout.Button(data.Name, button_S, GUILayout.Width(160), GUILayout.Height(buttonHeight)))
                    {
                            
                        if(!ServerData.isRunning)
                            Server.Start();
                        else
                        {
                            Server.Stop();
                            Server.Start();
                        }
                        window = guiWindow.lobby;
                        SavesManager.LoadSave(data);
                        saveIndex = data.saveIndex;

                    }
                    
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();  
        }

        private void RenderLobby()
        {
         //   GUI.Box(new Rect(mRect.x - 5, mRect.y, mRect.width, mRect.height),"", background_S);
            GUILayout.BeginArea(mRect, GUI.skin.box);
            
            GUILayout.Label("Lobby", label_S);
            
            GUILayout.BeginHorizontal();
            // Colonne 1 - Noms
            GUILayout.BeginVertical(GUILayout.Width(70));
            foreach (int i in ClientData.Instance.players.Keys)
            {
                Player player =  ClientData.Instance.players[i];
                if (player != null)
                {
                    GUILayout.Label("   " + player.username, text_S);
                    GUILayout.Space(5);
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Colonne 2 - États  
            GUILayout.BeginVertical(GUILayout.Width(90));
            foreach (int i in ClientData.Instance.players.Keys)
            {
                Player player =  ClientData.Instance.players[i];

                if (player != null)
                {
                    if (player.isReady)
                    {
                        GUILayout.Label("Ready", text_S);
                    }
                    else
                    {
                        GUILayout.Label("Not Ready", text_S);
                    }
                    GUILayout.Space(25);
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Colonne 3 - Interactions
            GUILayout.BeginVertical(GUILayout.Width(100));
            foreach (int i in ClientData.Instance.players.Keys)
            {
                //GUILayout.Space(5);
                Player player =  ClientData.Instance.players[i];
                if (player != null)
                {
                    if (player.id == Client.Instance.Id)
                    {
                        if (GUILayout.Button("Ready", button_S)) 
                        {
                            player.isReady = !player.isReady;
                            ClientSend.SendReadyState(player.isReady, i);
                        }
                    }
                    if (Client.Instance.Id == 1 && player.id != 1) 
                    {  
                        if (GUILayout.Button("Kick", button_S)) 
                        {
                            ServerSend.DisconnectClient(player.id, "You've been kicked from server!");
                            Server.clients[player.id].Disconnect(player.id);
                        }
                    }
                }
                GUILayout.Space(40);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            //GUILayout.FlexibleSpace(); // Pousse les éléments vers le haut
            
            if (ServerData.isRunning)
            {
                if (ModSceneManager.isInMenu())
                {
                    if (GUILayout.Button("Start Game", button_S, GUILayout.Width(175), GUILayout.Height(35)))
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
                        SavesManager.SaveModSave(saveIndex - 1);
                        showModUI = false;
                    }
                }

                if(GUILayout.Button("Close Server", button_S, GUILayout.Width(175), GUILayout.Height(35))) 
                {
                    window = guiWindow.main;
                    Server.Stop();
                    if (!ModSceneManager.isInMenu())
                    {
                        NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                    }

                }
                
            }
            else
            {
                if(GUILayout.Button("Disconnect", button_S, GUILayout.Width(100), GUILayout.Height(35)))
                {
                    window = guiWindow.main;
                    Client.Instance.Disconnect();
                }
            }
            
            GUILayout.Space(15);
            GUILayout.EndArea();
            
        }

        private void StartGame(int _saveIndex)
        {
            SavesManager.StartGame(_saveIndex);
            ServerSend.StartGame(new ModProfileData(SavesManager.GetProfile(_saveIndex)));
        }
        private void JoinLobby()
        {
            if (!string.IsNullOrEmpty(Client.Instance.username) && !string.IsNullOrEmpty(Client.Instance.ip))
            {
                Client.Instance.ConnectToServer(Client.Instance.ip);
                window = guiWindow.lobby;
                Application.runInBackground = true;
            }
        }

        public void showUI()
        {
            if (Input.GetKeyDown(MainMod.MOD_GUI_KEY))
            {
                if(ModSceneManager.currentScene() == GameScene.unknow)
                    return;
                
                showModUI = !showModUI;
            }

        }
    }

    public enum guiWindow
    {
        main,
        host,
        lobby
    }
}