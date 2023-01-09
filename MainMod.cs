using System;
using System.Collections.Generic;
using System.IO;
using CMS21MP.ClientSide;
using CMS21MP.DataHandle;
using CMS21MP.ServerSide;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Client = CMS21MP.ClientSide.Client;

namespace CMS21MP
{
    public class MainMod : MelonMod
    {
       public PlayerManager gameManager;
       public Client client;
       public ModGUI modGUI;
       
       public static bool isPrefabSet = false;

       public ThreadManager threadManager;

       public static bool isHosting = false;
       public static bool isConnected = false;
       public static int playerConnected;
       public static int maxPlayer;

       public static GameObject localPlayer;

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

           gameManager = new PlayerManager();
           gameManager.Initialize();

           
           LoggerInstance.Msg("Mod Initialized!");
        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
           if (SceneManager.GetActiveScene().name == "garage")
           {
              localPlayer = GameObject.FindObjectOfType<FPSInputController>().gameObject;
           }
        }

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
           // MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
        }

        public override void OnUpdate() // Runs once per frame.
        {
           DataUpdating.UpdateData();
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
           ClassInjector.RegisterTypeInIl2Cpp<playerManagement>();

           GameObject playerPrefab = GameObject.Find("First Person Controller");
           playerPrefab.AddComponent<PlayerInfo>();
           playerPrefab.AddComponent<playerManagement>();

           AssetBundle pModel = AssetBundle.LoadFromFile(@"Mods\cms21mp\playermodel");

           if (pModel != null)
           {
              MelonLogger.Msg("Assets found inside of file are:");
              foreach (string item in pModel.AllAssetNames())
              {
                 MelonLogger.Msg(item);
              }

              UnityEngine.Object ueobject = pModel.LoadAsset("assets/prefabs/playermodel.prefab");
              GameObject gameObjectPrefab = ueobject.TryCast<GameObject>(); //Is this gameobject? Lets try at least
              // MelonLogger.Msg(gameObjectPrefab.ToString());

              //Lets spawn this object to visible world (it will be on location 0 0 0) 
              GameObject model = GameObject.Instantiate(gameObjectPrefab);
              model.GetComponentInChildren<MeshRenderer>().material =
                 Singleton<GameInventory>.Instance.materials["body_paint"];

              model.name = "playerPrefab";
              model.AddComponent<MeshFilter>();
              model.AddComponent<MeshRenderer>();
              model.AddComponent<PlayerInfo>();
              model.GetComponent<MeshFilter>().mesh = gameObjectPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
              model.transform.localScale = new Vector3(1f, 1f, 1f);
              model.transform.position = new Vector3(0, -10, 0);
              model.transform.rotation = new Quaternion(0, 180, 0, 0);

              gameManager.playerPrefab = model;
              gameManager.localPlayerPrefab = playerPrefab;

              isPrefabSet = true;
              MelonLogger.Msg("ModelCreated SuccesFully");
           }
           else
           {
              MelonLogger.Msg("AssetBundle load failed");
           }
        }
    }
}
