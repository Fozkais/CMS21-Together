using System;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Data;
using CMS21MP.ServerSide;
using CMS21MP.ServerSide.DataHandle;
using CMS21MP.SharedData;
using MelonLoader;
using Steamworks;
using UnityEngine;
using Client = CMS21MP.ClientSide.Client;

namespace CMS21MP
{
   public class MainMod : MelonMod
   {
      public const int MAX_SAVE_COUNT = 16;
      public const int MAX_PLAYER = 4;
      public const int PORT = 7777;
      public const string ASSEMBLY_MOD_VERSION = "0.1.2";
      public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
      public const KeyCode MOD_GUI_KEY = KeyCode.RightShift;
      
      public static bool usingSteamAPI = false;

      public Client client;
      public ModUI modGUI;
      public ThreadManager threadManager;

      public static bool isServer = false;
      public static bool isClient { get { return !isServer; } }
      

      public override void OnInitializeMelon() // Runs during Game Initialization.
      {
         
      }

      public override void OnLateInitializeMelon() // Runs after Game has finished starting.
      {
         SaveSystem.GetVanillaSaves();
         
         threadManager = new ThreadManager();

         modGUI = new ModUI();
         modGUI.Initialize();

         client = new Client();
         client.Initialize();

         LoggerInstance.Msg("Mod Initialized!");
         PreferencesManager.LoadPreferences();
         //PreferencesManager.LoadAllModSaves();

         SteamClient.Init(1190000);
         if (SteamClient.IsValid)
         {
            CallbackHandler.InitializeCallbacks();
            SteamData.Name = SteamClient.Name;
            SteamData.steamID = SteamClient.SteamId;
            SteamData.steamIDString = SteamClient.SteamId.ToString();
            SteamData.connectedToSteam = true;
            
            MelonLogger.Msg("Steam Initialized! :" + SteamData.Name);
         }
         else
         {
            MelonLogger.Msg("Steam not Initialized!");
         }
      }

      public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
      {
         // MelonLogger.Msg("OnSceneWasLoaded: " + buildindex.ToString() + " | " + sceneName

         #region MultiplayerState

            if(sceneName == "Menu" && client.isConnected && !isServer)
            {
               Client.Instance.Disconnect();
               Application.runInBackground = false;
            }
            if(sceneName == "Menu" && isServer)
            {
               Server.Stop();
               Application.runInBackground = false;
            }
            if(sceneName == "garage" || sceneName == "Junkyard" || sceneName == "Auto_salon")
            {
               ClientData.Init();
            }
            #endregion
      }

      public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
      {
         // MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
      }

      public override void OnUpdate() // Runs once per frame.
      {

         if (GameData.DataInitialzed)
         {
            if (Client.Instance.isConnected)
            {
               ClientData.UpdateClientInfo();
            }
         }
         
         threadManager.UpdateThread();
         
         if(SteamData.connectedToSteam) { SteamClient.RunCallbacks(); }
         if(SteamData.StartReceivingPacket) { ClientData.ReceivePacket(); }
      }

      public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
      {

      }

      public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
      {
         modGUI.showGui();
      }

      public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
      {
         modGUI.OnMPGUI();
      }


      public override void OnDeinitializeMelon() // Called after preferenced saved and before application quit.
      {

      }

      public override void OnApplicationQuit() // Runs when the Game is told to Close.ca
      {
         //Client.Instance.ClientOnApplicationQuit();
         //PreferencesManager.SaveAllModSaves();
      }

      public override void OnPreferencesSaved() // Runs when Melon Preferences get saved.
      {
         // MelonLogger.Msg("OnPreferencesSaved");
      }

      public override void OnPreferencesLoaded() // Runs when Melon Preferences get loaded.
      {
         // MelonLogger.Msg("OnPreferencesLoaded");
      }

   }

}

