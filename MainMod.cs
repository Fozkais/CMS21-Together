using System;
using System.Collections.Generic;
using System.IO;
using CMS21MP.ClientSide;
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
       public MPGameManager gameManager;
       public Client client;
       public ModGUI modGUI;


       public static bool isPrefabSet = false;

       public ThreadManager threadManager;

       public static bool isHosting = false;
       public static bool isConnected = false;
       public static int playerConnected;
       public static int maxPlayer;

       public AssetBundle playerModel;

       public static Dictionary<int, List<Vector3>> MovUpdateQueue = new Dictionary<int, List<Vector3>>();
       public static Dictionary<int, List<Quaternion>> RotUpdateQueue = new Dictionary<int, List<Quaternion>>();
       
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

           gameManager = new MPGameManager();
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
              if(isConnected)
              {
                 GameObject.Find("First Person Controller").GetComponent<playerInputManagement>().playerPosUpdate();
              }
           }
           if (MovUpdateQueue.Count > 0)
           {
              foreach (KeyValuePair<int, List<Vector3>> element in MovUpdateQueue)
              {
                 for (int i = 0; i < element.Value.Count; i++)
                 {
                     ServerSend.PlayerPosition(element.Key, element.Value[i]);
                     MovUpdateQueue[element.Key].Remove(element.Value[i]);
                 }
              }
           }
           if (RotUpdateQueue.Count > 0)
           {
              foreach (KeyValuePair<int, List<Quaternion>> element in RotUpdateQueue)
              {
                 for (int i = 0; i < element.Value.Count; i++)
                 {
                    ServerSend.PlayerRotation(element.Key, element.Value[i]);
                    RotUpdateQueue[element.Key].Remove(element.Value[i]);
                 }
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
              model.AddComponent<PlayerManager>();
              model.GetComponent<MeshFilter>().mesh = gameObjectPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
              model.transform.localScale = new Vector3(1f, 1f, 1f);
              model.transform.position = new Vector3(0, -10, 0);
              model.transform.rotation = new Quaternion(0, 180, 0, 0);

              gameManager.playerPrefab = model;
              gameManager.localPlayerPrefab = playerPrefab;

              isPrefabSet = true;
           }
           else
           {
              MelonLogger.Msg("AssetBundle load failed");
           }
        }
    }
}
