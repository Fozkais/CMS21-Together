using CMS21MP.ClientSide;
using CMS21MP.SharedData;
using MelonLoader;
using UnityEngine;
using Client = CMS21MP.ClientSide.Client;

namespace CMS21MP
{
   public class MainMod : MelonMod
   {
      public const int MAX_SAVE_COUNT = 17;
      public const int MAX_PLAYER = 4;
      public const int PORT = 7777;
      public const string ASSEMBLY_MOD_VERSION = "0.1.0";
      public const string MOD_VERSION = "Together's " + ASSEMBLY_MOD_VERSION;
      public const KeyCode MOD_GUI_KEY = KeyCode.RightShift;

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
         threadManager = new ThreadManager();

         modGUI = new ModUI();
         modGUI.Initialize();

         client = new Client();
         client.Initialize();

         LoggerInstance.Msg("Mod Initialized!");
         PreferencesManager.LoadPreferences();
      }

      public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
      {
        
      }

      public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
      {
         // MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
      }

      public override void OnUpdate() // Runs once per frame.
      {
         threadManager.UpdateThread();
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

      public override void OnApplicationQuit() // Runs when the Game is told to Close.
      {
         Client.Instance.ClientOnApplicationQuit();
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

