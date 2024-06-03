using System;
using System.Collections.Generic;
using System.Reflection;
using CMS21Together.ServerSide.Data;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable All

namespace CMS21Together
{
    public class MainMod : MelonMod
    {
        public const int MAX_SAVE_COUNT = 22; // need to add 6 to match correct save number: 16 = 22 (+1 for clientSlot)
        public const int MAX_PLAYER = 4;
        public const int PORT = 7777;
        public const string ASSEMBLY_MOD_VERSION = "0.3.3";
        public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
        public const KeyCode MOD_GUI_KEY = KeyCode.RightShift;
        
        public Client client;
        public ModUI modUI;
        public ContentManager contentManager;

        public bool isModInitialized;

        public override void OnEarlyInitializeMelon()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ApiCalls.CurrentDomain_AssemblyResolve);
        }

        public override void OnLateInitializeMelon()
        {
            GameObject modObject = new GameObject("TogetherMod");
            Object.DontDestroyOnLoad(modObject);
            client = modObject.AddComponent<Client>();
            client.Awake();
            
            modUI = modObject.AddComponent<ModUI>();
            modUI.Awake();
            
            contentManager = modObject.AddComponent<ContentManager>();
            
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
            if(sceneName == "Menu") contentManager.Initialize();
            if(!isModInitialized) {return;}
            contentManager.LoadCustomlogo();
            CustomUIManager.OnSceneChange(sceneName);
            
            if (client.isConnected || ServerData.isRunning)
            {
                GameData.DataInitialized = false;
                ModSceneManager.UpdatePlayerScene();
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
                    MelonCoroutines.Start(ClientData.Instance.Initialize());
                    MelonCoroutines.Start(CarManagement.UpdateCarOnSceneChange());

                    foreach (KeyValuePair<int, Player> player in ClientData.Instance.players)
                    {
                        if (ModSceneManager.isInGarage(player.Value))
                        {
                            MelonLogger.Msg($"Player: {player.Value.username} in garage, Spawning...");
                            if (!ClientData.Instance.PlayersGameObjects.ContainsKey(player.Value.id))
                            {
                                ClientData.Instance.SpawnPlayer(player.Value);
                            }
                        }
                    }
                }
            }
        }

        public override void OnUpdate()
        {
            if(!isModInitialized) {return;}
            if (GameData.DataInitialized)
            {
                if (Client.Instance.isConnected)
                {
                    if(ModSceneManager.isInGarage())
                        ClientData.Instance.UpdateClient();
                }
            }
            if (Client.Instance.isConnected)
            {
                if (ClientData.Instance.needToKeepAlive)
                {
                    if (!ClientData.Instance.isKeepingAlive)
                    {
                        MelonCoroutines.Start(ClientData.Instance.keepClientAlive());
                        MelonCoroutines.Start(ClientData.Instance.isServer_alive());
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
            
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Cursor3D.Get().BlockCursor(false);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                Cursor3D.Get().BlockCursor(true);
            }
        }
        
        
        
        public override void OnApplicationQuit() // Runs when the Game is told to Close.ca
        {
            PreferencesManager.SaveMelonLog();
            if (ServerData.isRunning)
            {
                foreach (int id in Server.clients.Keys)
                {
                    ServerSend.DisconnectClient(id, "Server is shutting down.");
                }
            }
        }
    }
}