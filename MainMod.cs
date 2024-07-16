using System;
using System.Collections;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using SteamManager = CMS21Together.Shared.SteamManager;

// ReSharper disable All

namespace CMS21Together
{
    public class MainMod : MelonMod
    {
        public const int MAX_SAVE_COUNT = 22;
        public const int MAX_PLAYER = 4;
        public const int PORT = 7777;
        public const string ASSEMBLY_MOD_VERSION = "1.0.0";
        public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
        public bool isModInitialized;

        public override void OnEarlyInitializeMelon()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ApiCalls.CurrentDomain_AssemblyResolve);
        }

        public override void OnLateInitializeMelon()
        {
            GameObject modObject = new GameObject("TogetherMod");
            Object.DontDestroyOnLoad(modObject);

            Client.Instance = modObject.AddComponent<Client>();
            Server.Instance = modObject.AddComponent<Server>();
            SteamManager.Instance = modObject.AddComponent<SteamManager>();
           
            ClientData.UserData = TogetherModManager.LoadUserData();
            isModInitialized = true;
            LoggerInstance.Msg("Together Mod Initialized!");
        }
        

        public override void OnGUI()
        {
            if(!isModInitialized) {return;}
        }
        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            if(!isModInitialized) {return;}
            CustomUIManager.OnSceneChange(sceneName);

            if (sceneName == "Menu")
            {
                if (Server.Instance.isRunning)
                    Server.Instance.CloseServer();
                if(Client.Instance.isConnected)
                    Client.Instance.Disconnect();
                
                Application.runInBackground = false;
            }

            if (Client.Instance.isConnected)
            {
                if(sceneName == "garage" && ClientData.Instance.playerPrefab == null)
                    ClientData.Instance.LoadPlayerPrefab();
                
            }
            
        }

        public override void OnUpdate()
        {
            if(!isModInitialized) {return;}
            if(!Client.Instance.isConnected) {return;}

            if (SceneManager.GetActiveScene().name == "garage")
            {
                ClientData.Instance.UpdateClient();
            }
            
            ThreadManager.UpdateThread();
        }


        public static void StartCoroutine(Il2CppSystem.Collections.IEnumerator routine)
        {
            GameManager.Instance.StartCoroutine(routine);
        }

        public override void OnLateUpdate()
        {
            if(!isModInitialized) {return;}
        }
        
        public override void OnApplicationQuit() // Runs when the Game is told to Close.ca
        {
           TogetherModManager.SavePreferences();
        }
    }
}