using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CMS21MP.ClientSide;
using CMS21MP.DataHandle;
using CMS21MP.ServerSide;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Client = CMS21MP.ClientSide.Client;
using Object = UnityEngine.Object;

namespace CMS21MP
{
   public class MainMod : MelonMod
   {
      public static PlayerManager gameManager;
      public Client client;
      public ModGUI modGUI;

      public static bool isPrefabSet;

      public ThreadManager threadManager;

      public static bool isHosting = false;
      public static bool isConnected = false;
      public static int playerConnected;
      public static int maxPlayer;

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

            if (isConnected)
            {
               foreach (KeyValuePair<int, PlayerInfo> player in PlayerManager.players)
               {
                  if (GameObject.Find($"{player.Value.username}") == null)
                  {
                     PlayerManager.instance.SpawnPlayer(player.Value.id, player.Value.username, new Vector3(),
                        new Quaternion());
                  }
               }
            }
         }
      }

      public override void
         OnSceneWasInitialized(int buildindex,
            string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
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
         LplayerPrefab.AddComponent<playerManagement>();
      }

      public void playerInit()
      {

         GameObject LplayerPrefab = GameObject.Find("First Person Controller");
         LplayerPrefab.AddComponent<PlayerInfo>();
         LplayerPrefab.AddComponent<playerManagement>();

         AssetBundle playerModel;

         playerModel = AssetBundle.LoadFromFile(@"Mods\cms21mp\playermodel");
         if (playerModel == null)
         {
            MelonLogger.Msg("Cant load playerModel bundle!");
         }
         else
         {

            var mesh = playerModel.LoadAsset<GameObject>("playermodel").GetComponentInChildren<SkinnedMeshRenderer>()
               .sharedMesh;
            Texture mainTexture = playerModel.LoadAsset<Texture>("mainTexture");
            Texture normalTexture = playerModel.LoadAsset<Texture>("Normal");
            Material playerMaterial = new Material(Shader.Find("HDRP/Lit"));
            playerMaterial.mainTexture = mainTexture;
            playerMaterial.shaderKeywords = new string[1] { "_NORMALMAP" };
            playerMaterial.SetTexture("_BumpMap", normalTexture);


            GameObject model = new GameObject();
            model.AddComponent<MeshFilter>();
            model.AddComponent<MeshRenderer>();
            model.GetComponent<MeshFilter>().mesh = mesh;
            model.GetComponent<MeshRenderer>().material = playerMat;

            model.name = "playerPrefab";
            model.AddComponent<PlayerInfo>();
            model.transform.localScale = new Vector3(1f, 1f, 1f);
            model.transform.position = new Vector3(0, -10, 0);
            model.transform.rotation = new Quaternion(0, 180, 0, 0);

            playerPrefab = model;

            isPrefabSet = true;
            playerModel.Unload(false);
            MelonLogger.Msg("ModelCreated SuccesFully");

         }

      }

   }

}
