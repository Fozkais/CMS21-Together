using System;
using System.Collections;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.CustomUI;

public static class UI_Lobby
{
     public static int  saveIndex;
        public static void InitializeLobbyMenu()
        {
            GameObject parent = new GameObject("MP_LobbyButtons");
            parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
            parent.transform.localPosition = new Vector3(-380, 0, 0);
            parent.transform.localScale = new Vector3(.65f, .65f, .65f);

            CustomUIManager.MP_Lobby_Parent = parent.transform;
            
            Vector2 b1_pos = new Vector2(20, 58);
            Vector2 b1_size = new Vector2(336, 65);
            Action b1_action = delegate
            {
              //  foreach (var player in ServerData.players.Values)
               // {
                 //   if (player != null && !player.isReady)
                  //  {
                  //      return;
                  //  }
              //  }
             //   MelonCoroutines.Start(StartGame());
            };
            ButtonInfo b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Start game");
            CustomUIBuilder.CreateNewButton(CustomUISection.MP_Lobby, b1_info, false); 
            
            Vector2 b2_pos = new Vector2(20, -17);
            Vector2 b2_size = new Vector2(336, 65);
            Action b2_action = delegate
            {
              //  foreach (int i in ClientData.Instance.players.Keys)
              //  {
                //    Player player =  ClientData.Instance.players[i];
                //    if (player != null)
                 //   {
                  //      if (player.id == Client.Instance.Id)
                     //   {
                      //      player.isReady = !player.isReady;
                           // ClientSend.SendReadyState(player.isReady, i);
                       //     ChangeReadyState(i, player.isReady);
                      //  }
                  //  }
            //    }
            };
            ButtonInfo b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Toggle Ready");
            CustomUIBuilder.CreateNewButton(CustomUISection.MP_Lobby, b2_info, false); 
            
            Vector2 b3_pos = new Vector2(20, -317);
            Vector2 b3_size = new Vector2(336, 65);
            Action b3_action = delegate { OpenMainMenu(); };
            ButtonInfo b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, "Disconnect");
            CustomUIBuilder.CreateNewButton(CustomUISection.MP_Lobby, b3_info, false); // Need to be last

            CustomUIBuilder.BuildLobbyHeader();
            
            
            CustomUIManager.DisableUI(CustomUISection.MP_Lobby);
        }
        
        public static void AddPlayerToLobby(string username, int index) // TODO: add a way for player to deseapear from lobby
        {
            var lobbyPlayerObject = new GameObject("LobbyPlayer");
            var img = lobbyPlayerObject.AddComponent<Image>();
            img.rectTransform.parent = CustomUIBuilder.GetParentFromSection(CustomUISection.MP_Lobby);
            img.rectTransform.parentInternal = CustomUIBuilder.GetParentFromSection(CustomUISection.MP_Lobby);
            
            Vector2 pos = new Vector2(600, 175 - (index * 75));
            
            img.color = new Color(  .031f, .027f, .033f  , 0.85f);
            img.rectTransform.sizeDelta = new Vector2(600, 75);
            img.rectTransform.anchoredPosition = pos;
            
            CustomUIManager.MP_Lobby_Addition.Add(lobbyPlayerObject);
            
            Vector2 t1_pos = new Vector2(500, 0);
            Vector2 t1_size = new Vector2(400, 100);
            CustomUIBuilder.CreateText(t1_pos, t1_size, username, 16, lobbyPlayerObject.transform);
            
            Vector2 t2_pos = new Vector2(680, 0);
            Vector2 t2_size = new Vector2(400, 100);
            CustomUIBuilder.CreateText(t2_pos, t2_size, "Not ready", 16, lobbyPlayerObject.transform);
            
            Vector2 t3_pos = new Vector2(940, 0);
            Vector2 t3_size = new Vector2(400, 100);
            CustomUIBuilder.CreateText(t3_pos, t3_size, "?ms", 16, lobbyPlayerObject.transform);
            
            CustomUIManager.MP_Lobby_Addition[0].SetActive(true);
        }

        public static void ChangeReadyState(int index, bool ready)
        {
            if (ready)
            {
                CustomUIManager.MP_Lobby_Addition[3 + index].transform.GetChild(1).GetComponent<Text>().text = "Ready";
            }
            else
            {
                CustomUIManager.MP_Lobby_Addition[3 + index].transform.GetChild(1).GetComponent<Text>().text =
                    "Not Ready";
            }
            CustomUIManager.MP_Lobby_Addition[3 + index].transform.GetChild(1).GetComponent<Text>().OnEnable();
        }
        

        public static void OpenMainMenu()
        {
            CustomUIManager.DisableUI(CustomUISection.MP_Lobby);
            CustomUIManager.EnableUI(CustomUISection.MP_Main);
            
            if (Server.Instance.isRunning)
            {
                Server.Instance.CloseServer();
            }
            else
            {
                Client.Instance.Disconnect();
            }
            
           // if (!ModSceneManager.isInMenu())
          //  {
         //       NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
         //   }
        }

        private static IEnumerator StartGame()
        {
            yield return new WaitForEndOfFrame();
            
            StartGame(saveIndex);
            SavesManager.ModSaves[saveIndex].alreadyLoaded = true;
            SavesManager.SaveModSave(saveIndex);
        }
        private static void StartGame(int _saveIndex)
        {
            SavesManager.StartGame(_saveIndex);
           // ServerSend.StartGame(null);
        }
}