using MelonLoader;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUIManager
    {
        public static void OnSceneChange(string scene)
        {
            if(scene == "Menu")
            {
                MelonCoroutines.Start(CustomMainMenu.DefaultMenuPatch());
            }
        }
    }
}