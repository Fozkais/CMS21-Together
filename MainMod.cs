using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Functionnality;
using CMS21MP.DataHandle;
using CMS21MP.ServerSide;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Client = CMS21MP.ClientSide.Client;
using Inventory = Il2Cpp.Inventory;

namespace CMS21MP
{
   public class MainMod : MelonMod
   {
      
      public const string MOD_VERSION = "Pre-Release 0.3";
      public const string ASSEMBLY_MOD_VERSION = "0.3";
      public const KeyCode MOD_GUI_KEY = KeyCode.RightShift;
      
      public static PlayerManager gameManager;
      public Client client;
      public ModGUI modGUI;
      public static DLCSupport DLC;

      public static bool isPrefabSet;

      public ThreadManager threadManager;

      public static bool SteamMode = false;
      public static bool isHosting = false;
      public static bool isConnected = false;
      public static int playerConnected;
      public static int maxPlayer = 4;

      public static GameObject localPlayer;
      public static Inventory localInventory;
      public static CarLoader[] carLoaders;

      public Material playerMat;
      public GameObject playerPrefab;
      public static bool playableSceneChecker()
      {
         if (SceneManager.GetActiveScene().name == "garage" || SceneManager.GetActiveScene().name == "Junkyard" ||
             SceneManager.GetActiveScene().name == "Auto_salon")
         {
            return true;
         }

         return false;
      }

      public static bool playableSceneWithInventory()
      {
         if (SceneManager.GetActiveScene().name == "garage" || SceneManager.GetActiveScene().name == "Junkyard")
         {
            return true;
         }

         return false;
      }

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
         gameManager._mainMod = this;

         DLC = new DLCSupport();
         DLC.Initialize();
         

         LoggerInstance.Msg("Mod Initialized!");
      }

      public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
      {
         if (playableSceneChecker())
         {
            localPlayer = GameObject.FindObjectOfType<FPSInputController>().gameObject;
            carLoaders = GameScript.Get().carOnScene;

            if (playableSceneWithInventory())
            {
               localInventory = GameScript.Get().GetComponent<Inventory>();
            }

            //if (isConnected)
           // {
             //  foreach (KeyValuePair<int, PlayerInfo> player in PlayerManager.players)
              // {
               //   if (GameObject.Find($"{player.Value.username}") == null)
                 // {
                  //   PlayerManager.instance.SpawnPlayer(player.Value.id, player.Value.username, new Vector3(),
                  //      new Quaternion());
                 // }
              // }
            //}
         }
      }

      public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
      {
         // MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
      }

      public override void OnUpdate() // Runs once per frame.
      {
         if (!DLC.DLCListSet && SceneManager.GetActiveScene().name == "Menu")
         {
            DLC.CheckDLC();
         }
         
         
         if (MainMod.isConnected)
         {
            Application.runInBackground = true;
            if (MainMod.playableSceneChecker())
            {
               if (MainMod.localPlayer != null)
               {
                  MainMod.localPlayer.GetComponent<MPGameManager>().InfoUpdate();
                  
                  if (MainMod.isHosting)
                  {
                     if (MainMod.playableSceneWithInventory())
                     {
                        ServerData.UpdateItems();
                        ServerData.UpdateGroupItems();
                     }
                  }
               }
            }
         }
         else
         {
            Application.runInBackground = false;
         }
         
         threadManager.UpdateThread();
      }

      public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
      {

      }

      public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
      {
         if (isConnected && PlayerManager.players.ContainsKey(Client.instance.myId))
         {
            if (PlayerManager.players[Client.instance.myId].activeScene != SceneManager.GetActiveScene().name)
            {
               if (SceneManager.GetActiveScene().name == "garage" || SceneManager.GetActiveScene().name == "Junkyard" ||
                   SceneManager.GetActiveScene().name == "Auto_salon")
               {
                  setPlayer();
                  PlayerManager.players[Client.instance.myId].activeScene = SceneManager.GetActiveScene().name;
                  ClientSend.PlayerScene(SceneManager.GetActiveScene().name);
                  MelonLogger.Msg($"Sending new scene name! : {SceneManager.GetActiveScene().name}");
               }
            }
         }

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

      public void setPlayer()
      {
         GameObject LplayerPrefab = GameObject.Find("First Person Controller");
         LplayerPrefab.AddComponent<PlayerInfo>();
         LplayerPrefab.AddComponent<MPGameManager>();
      }

      // ReSharper disable Unity.PerformanceAnalysis
      public void playerInit()
      {

         GameObject LplayerPrefab = GameObject.Find("First Person Controller");
         LplayerPrefab.AddComponent<PlayerInfo>();
         LplayerPrefab.AddComponent<MPGameManager>();

         AssetBundle playerModel;
         AssetBundle playerTexture;

         playerModel = AssetBundle.LoadFromFile(@"Mods\cms21mp\playermodel");
         playerTexture = AssetBundle.LoadFromFile(@"Mods\cms21mp\playertexture");
         
         if (playerModel == null)
         {
            MelonLogger.Msg("Cant load playerModel bundle!");
         }
         else
         {

            var mesh = playerModel.LoadAsset<GameObject>("playermodel").GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            if (playerTexture != null)
            {
               Texture mainTexture = playerTexture.LoadAsset<Texture>("rp_nathan_animated_003_dif");
               mainTexture.filterMode = FilterMode.Point;
               Material playerMaterial = new Material(Shader.Find("HDRP/Unlit"));
               playerMaterial.mainTexture = mainTexture;
               playerMat = playerMaterial;
            }
            else
            {
               MelonLogger.Msg("Can't load playerTexture !");
            }


            GameObject model = new GameObject();
            model.AddComponent<MeshFilter>();
            model.AddComponent<MeshRenderer>();
            model.GetComponent<MeshFilter>().mesh = mesh;
            model.GetComponent<MeshRenderer>().material = playerMat;

            model.name = "playerModel";
            model.AddComponent<PlayerInfo>();
            model.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            model.transform.position = new Vector3(0, -10, 0);
            model.transform.rotation = new Quaternion(0, 180, 0, 0);

            playerPrefab = model;

            isPrefabSet = true;
            playerModel.Unload(false);
            playerTexture.Unload(false);
            MelonLogger.Msg("ModelCreated SuccesFully");

         }

      }

   }

}
