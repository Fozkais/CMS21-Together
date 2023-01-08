using System;
using System.Collections.Generic;
using System.IO;
using CMS21MP.ClientSide;
using CMS21MP.ServerSide;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Client = CMS21MP.ClientSide.Client;

namespace CMS21MP
{
    public class MainMod : MelonMod
    {
       public GameManager gameManager;
       public Client client;
       public ModGUI modGUI;


       public bool isPrefabSet = false;

       public ThreadManager threadManager;

       public static bool isHosting = false;
       public static bool isConnected = false;
       public static int playerConnected;
       public static int maxPlayer;

       public AssetBundle playerModel;
       
       public static Dictionary<int, Vector3> UpdateQueue = new Dictionary<int, Vector3>();
       
        public override void OnInitializeMelon() // Runs during Game Initialization.
        {
           
        }

        public override void OnLateInitializeMelon() // Runs after Game has finished starting.
        {

           threadManager = new ThreadManager();
           
           client = new Client();
           client.Initialize();

           modGUI = new ModGUI();
           modGUI.Initialize();
           modGUI.Mod = this;

           gameManager = new GameManager();
           gameManager.Initialize();

           
           LoggerInstance.Msg("Mod Initialized!");
        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
           // MelonLogger.Msg("OnSceneWasLoaded: " + buildindex.ToString() + " | " + sceneName);
        }

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
           // MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
        }

        public override void OnUpdate() // Runs once per frame.
        {
           if (SceneManager.GetActiveScene().name == "garage")
           {
              if (!isPrefabSet)
              {
                 playerInit();
                 isPrefabSet = true;
              }
              else if(isConnected)
              {
                 GameObject.Find("First Person Controller").GetComponent<playerInputManagement>().playerPosUpdate();
              }
           }
           else
           {
              isPrefabSet = false;
              Client.instance.Disconnect();
           }
           if (UpdateQueue.Count > 0)
           {
              foreach (KeyValuePair<int, Vector3> element in UpdateQueue)
              {
                 ServerSend.PlayerPosition(element.Key, element.Value);
                 UpdateQueue.Remove(element.Key);
              }
           }
           
           
           threadManager.UpdateThread();
        }

        public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
        {
           
        }

        public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
        {
           modGUI.GuiOnUpdate();
        }

        public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
        {
           modGUI.OnGUI();
        }


        public override void OnDeinitializeMelon() // Called after preferenced saved and before application quit.
        {
           
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
           Client.instance.ClientOnApplicationQuit();
        }

        public override void OnPreferencesSaved() // Runs when Melon Preferences get saved.
        {
           // MelonLogger.Msg("OnPreferencesSaved");
        }

        public override void OnPreferencesLoaded() // Runs when Melon Preferences get loaded.
        {
           // MelonLogger.Msg("OnPreferencesLoaded");
        }
        
        public void playerInit()
        {
           ClassInjector.RegisterTypeInIl2Cpp<PlayerManager>();
           ClassInjector.RegisterTypeInIl2Cpp<playerInputManagement>();

           GameObject playerPrefab = GameObject.Find("First Person Controller");
           playerPrefab.AddComponent<PlayerManager>();
           playerPrefab.AddComponent<playerInputManagement>();
           
          // playerModel = AssetBundle.LoadFromFile(Path.Combine(Directory.GetCurrentDirectory(),"AssetBundles", "playermodel.model"));
          // if (playerModel == null)
          // {
           //   LoggerInstance.Msg("Can't load bundle! : " + Path.Combine(Directory.GetCurrentDirectory(),"AssetBundles", "playermodel.model"));
          // }
           
           //var mesh = playerModel.LoadAsset<GameObject>("playerModel").GetComponentInChildren<MeshFilter>().sharedMesh;
           GameObject model = new GameObject();
           model.name = "playerPrefab";
           model.AddComponent<MeshFilter>();
           model.AddComponent<MeshRenderer>();
           model.AddComponent<PlayerManager>();
          // model.GetComponent<MeshFilter>().mesh = mesh;
           model.transform.localScale = new Vector3(1, 1, 1);

           gameManager.playerPrefab = model;
           gameManager.localPlayerPrefab = playerPrefab;
           
        }
    }
}
