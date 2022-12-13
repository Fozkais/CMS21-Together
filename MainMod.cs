using MelonLoader;

namespace CMS21MP
{
    public class MainMod : MelonMod
    {
       public Client client;
       public ModGUI modGUI;
       
        public override void OnApplicationStart() // Runs after Game Initialization.
        {
           
        }

        public override void OnApplicationLateStart() // Runs after OnApplicationStart.
        {
           client = new Client(); 
           client.Initialize();
           
           modGUI = new ModGUI();
           modGUI.Mod = this;
           MelonLogger.Msg("Mod Initialized!");
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
           // MelonLogger.Msg("OnUpdate");
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

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
          //  MelonLogger.Msg("OnApplicationQuit");
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
