﻿using System;
using System.Collections.Generic;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Data;
using CMS21MP.ServerSide;
using CMS21MP.ServerSide.DataHandle;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Client = CMS21MP.ClientSide.Client;

namespace CMS21MP
{
   public class MainMod : MelonMod
   {
      public const int MAX_SAVE_COUNT = 16;
      public const int MAX_PLAYER = 4;
      public const int PORT = 7777;
      public const string ASSEMBLY_MOD_VERSION = "0.2.8";
      public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
      public const KeyCode MOD_GUI_KEY = KeyCode.RightShift;
      
      public static bool usingSteamAPI = false;

      public Client client;
      public ModUI modGUI;
      public ThreadManager threadManager;

      public static bool isInitialized;

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

         PreferencesManager.LoadPreferences();
         isInitialized = true;
         LoggerInstance.Msg("Mod Initialized!");
         
      }

      public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
      {
         // MelonLogger.Msg("OnSceneWasLoaded: " + buildindex.ToString() + " | " + sceneName
         

         if (isInitialized)
         {
            if (client.isConnected || isServer)
            {
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
               if(sceneName == "garage" /*|| sceneName == "Junkyard" || sceneName == "Auto_salon"  TODO: Re-enable when Init modified to adpat between scene*/ ) 
               {
                  ClientData.Init();
               }
               
               if (SceneChecker.isInGarage())
               {
                  foreach (KeyValuePair<int, Player> player in ClientData.serverPlayers)
                  {
                     if (SceneChecker.isInGarage(player.Value))
                     {
                        MelonLogger.Msg($"Player: {player.Value.username} in garage, Spawning...");
                        ClientData.SpawnPlayer(player.Value, player.Key);
                     }
                  }
               }
            }
         }
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
               if(SceneChecker.isInGarage())
                  ClientData.UpdateClientInfo();

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
         }

         if (MainMod.isServer)
         {
            if (Server.clients.Count == 0)
            {
               Server.Stop();
            }
         }

         threadManager.UpdateThread();
      }

      public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
      {

      }

      public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
      {
         modGUI.showGui();
         if (Input.GetKeyDown(KeyCode.RightControl)) //Debug Mounting part simulteanously
         {
            Cursor3D.Get().BlockCursor(false);
         }

         if (Input.GetKeyDown(KeyCode.F4) && Input.GetKeyDown(KeyCode.LeftAlt))
         {
            PreferencesManager.SaveMelonLog();
         }
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
         PreferencesManager.SaveMelonLog();
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

