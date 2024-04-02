using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUIManager
    {

        public static bool InLobbyWindow;
        public static void OnSceneChange(string scene)
        {
            if(scene == "Menu")
            {
                CustomLobbyMenu.lobbyPBorder = DataHelper.LoadCustomTexture("CMS21Together.Assets.lobbyBorder.png");
                CustomHostMenu.isSet = false;
                CustomHostMenu.isSavesSet = false;
                CustomHostMenu.isnewSaveSet = false;
                CustomLobbyMenu.isSet = false;
                InLobbyWindow = false;
                MelonCoroutines.Start(CustomMainMenu.DefaultMenuPatch());
            }
        }

        public static void UpdateLobby()
        {
            if(!InLobbyWindow) return;
            
            for (int i = 0; i < CustomLobbyMenu.backgrounds.Count; i++)
            {
                int correctIndex;
                if (i == 0) correctIndex = 1;
                else correctIndex = i - 1;
                if (ClientData.players.TryGetValue(correctIndex, out var player))
                {
                    if (!CustomLobbyMenu.backgrounds[i].active)
                    {
                        CustomLobbyMenu.backgrounds[i].SetActive(true);
                        CustomLobbyMenu.usernameText[i].SetActive(true);
                        CustomLobbyMenu.usernameText[i].GetComponent<Text>().text = player.username;
                        CustomLobbyMenu.readyText[i].SetActive(true);
                        CustomLobbyMenu.kickButtons[i].SetActive(true);
                        
                    }
                    else
                    {
                        if (player.isReady)
                            CustomLobbyMenu.readyText[i].GetComponent<Text>().text = "Ready";
                        else
                            CustomLobbyMenu.readyText[i].GetComponent<Text>().text = "Not Ready";
                        
                    }
                }
                else
                {
                    if(CustomLobbyMenu.backgrounds[i].active)
                        CustomLobbyMenu.backgrounds[i].SetActive(false);
                    if(CustomLobbyMenu.usernameText[i].active)
                        CustomLobbyMenu.usernameText[i].SetActive(false);
                    if(CustomLobbyMenu.readyText[i].active)
                        CustomLobbyMenu.readyText[i].SetActive(false);
                    if(CustomLobbyMenu.kickButtons[i].active)
                        CustomLobbyMenu.kickButtons[i].SetActive(false);
                }
            }
        }
    }
}