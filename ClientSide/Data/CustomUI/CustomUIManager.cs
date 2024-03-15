using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUIManager
    {
        public static void OnSceneChange(string scene)
        {
            if(scene == "Menu")
            {
                CustomLobbyMenu.lobbyPBorder = DataHelper.LoadCustomTexture("CMS21Together.Assets.lobbyBorder.png");
                CustomHostMenu.isSet = false;
                CustomHostMenu.isSavesSet = false;
                CustomHostMenu.isnewSaveSet = false;
                CustomLobbyMenu.isSet = false;
                MelonCoroutines.Start(CustomMainMenu.DefaultMenuPatch());
            }
        }
    }
}