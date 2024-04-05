using System.Collections.Generic;
using CMS21Together.ServerSide.Data;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;
// ReSharper disable All

namespace CMS21Together
{
    public class MainMod : MelonMod
    {
        public const int MAX_SAVE_COUNT = 22; // need to add 6 to match correct save number: 16 = 22 (+1 for clientSlot)
        public const int MAX_PLAYER = 4;
        public const int PORT = 7777;
        public const string ASSEMBLY_MOD_VERSION = "0.3.0";
        public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
        public const KeyCode MOD_GUI_KEY = KeyCode.RightShift;
        
        public Client client;
        public ModUI modUI;
        public ContentManager contentManager;

        public bool isModInitialized;

        public override void OnLateInitializeMelon()
        {
            client = new Client();
            client.Awake();
            
            modUI = new ModUI();
            modUI.Awake();
            
            contentManager = new ContentManager();
            contentManager.Initialize();
            

            SavesManager.GetVanillaSaves();
            SavesManager.InitializeModdedSaves();
            
            PreferencesManager.LoadPreferences();
            isModInitialized = true;
            LoggerInstance.Msg("Together Mod Initialized!");
        }

        public override void OnGUI()
        {
            if(!isModInitialized) {return;}
            modUI.OnGUI();
        }
        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            if(!isModInitialized) {return;}
            
            ContentManager.Instance.LoadCustomlogo();
            CustomUIManager.OnSceneChange(sceneName);
            
            if (client.isConnected || ServerData.isRunning)
            {
                if(ModSceneManager.isInMenu() && client.isConnected && !ServerData.isRunning)
                {
                    Client.Instance.Disconnect();
                    Application.runInBackground = false;
                }
                if(ModSceneManager.isInMenu() && ServerData.isRunning)
                {
                    Server.Stop();
                    Application.runInBackground = false;
                }
                if(ModSceneManager.isInGarage()) 
                {
                    ClientData.Init();

                    foreach (KeyValuePair<int, Player> player in ClientData.players)
                    {
                        if (ModSceneManager.isInGarage(player.Value))
                        {
                            MelonLogger.Msg($"Player: {player.Value.username} in garage, Spawning...");
                            if (!ClientData.PlayersGameObjects.ContainsKey(player.Value.id))
                            {
                                ClientData.SpawnPlayer(player.Value);
                            }
                        }
                    }
                }
            }
        }

        public override void OnUpdate()
        {
            if(!isModInitialized) {return;}
            if (GameData.DataInitialzed)
            {
                if (Client.Instance.isConnected)
                {
                    if(ModSceneManager.isInGarage())
                        ClientData.UpdateClient();
                }
            }
            if (Client.Instance.isConnected)
            {
                if (ClientData.needToKeepAlive)
                {
                    if (!ClientData.isKeepingAlive)
                    {
                        MelonCoroutines.Start(ClientData.keepClientAlive());
                        MelonCoroutines.Start(ClientData.isServer_alive());
                    }
                }

                if (ModSceneManager.isInMenu())
                {
                    CustomUIManager.UpdateLobby();
                }
            }

            if (ServerData.isRunning)
            {
                if (Server.clients.Count == 0)
                {
                    Server.Stop();
                }
            }
            ThreadManager.UpdateThread();
        }

        public override void OnLateUpdate()
        {
            if(!isModInitialized) {return;}
            modUI.showUI();
        }
        
        public override void OnApplicationQuit() // Runs when the Game is told to Close.ca
        {
            PreferencesManager.SaveMelonLog();
        }
    }
}