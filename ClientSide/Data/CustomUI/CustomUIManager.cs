using System.Collections.Generic;
using System.Linq;
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
                CustomHostMenu.isSet = false;
                CustomHostMenu.isSavesSet = false;
                CustomHostMenu.isnewSaveSet = false;
                CustomLobbyMenu.isSet = false;
                InLobbyWindow = false;
                CustomLobbyMenu.backgrounds = new List<GameObject>();
                CustomLobbyMenu.readyText = new List<GameObject>();
                CustomLobbyMenu.usernameText = new List<GameObject>();
                CustomLobbyMenu.kickButtons = new List<GameObject>();
                MelonCoroutines.Start(CustomMainMenu.DefaultMenuPatch());
            }
        }

        public static void UpdateLobby()
        {
            if (!InLobbyWindow) return;

            for (int i = 0; i < 4; i++)
            {
                if (ClientData.players.TryGetValue(i + 1, out var player))
                {
                    MelonLogger.Msg("found player info");
                    if (CustomLobbyMenu.usernameText.All(s => s.GetComponent<Text>().text != player.username))
                    {
                        MelonLogger.Msg("found invalid ui info");
                        if (!CustomLobbyMenu.backgrounds[i].active)
                        {
                            MelonLogger.Msg("updating UI");
                            CustomLobbyMenu.backgrounds[i].SetActive(true);
                            CustomLobbyMenu.usernameText[i].SetActive(true);
                            CustomLobbyMenu.usernameText[i].GetComponent<Text>().text = player.username;
                            CustomLobbyMenu.readyText[i].SetActive(true);
                            CustomLobbyMenu.kickButtons[i].SetActive(true);

                        }
                    }
                    else
                    {
                        MelonLogger.Msg("info are valid");
                        if (player.isReady)
                            CustomLobbyMenu.readyText[i].GetComponent<Text>().text = "Ready";
                        else
                            CustomLobbyMenu.readyText[i].GetComponent<Text>().text = "Not Ready";

                    }
                }
                else
                {
                    if (CustomLobbyMenu.backgrounds[i].active)
                        CustomLobbyMenu.backgrounds[i].SetActive(false);
                    if (CustomLobbyMenu.usernameText[i].active)
                        CustomLobbyMenu.usernameText[i].SetActive(false);
                    if (CustomLobbyMenu.readyText[i].active)
                        CustomLobbyMenu.readyText[i].SetActive(false);
                    if (CustomLobbyMenu.kickButtons[i].active)
                        CustomLobbyMenu.kickButtons[i].SetActive(false);
                }
            }
        }
    }
}