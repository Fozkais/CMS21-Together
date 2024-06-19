using System;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUISaves
    {
        public static int saveIndex = 0;
        
        public static void InitializeSavesMenu()
        {
            CustomUIManager.ResetSaveUI();

            if (CustomUIManager.MP_Saves_Parent == null)
            {
                GameObject parent = new GameObject("MP_SavesButtons");
                parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
                parent.transform.localPosition = new Vector3(-380, 0, 0);
                parent.transform.localScale = new Vector3(.65f, .65f, .65f);

                CustomUIManager.MP_Saves_Parent = parent.transform;
            }

            ShowSaveButton(saveIndex);
            
            Vector2 b1_pos = new Vector2(-62, -242);
            Vector2 b1_size = new Vector2(164, 65);
            Action b1_action = delegate { ShowPreviousSaves(); };
            ButtonInfo b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Previous");
            CustomUIBuilder.CreateNewButton(UISection.MP_Saves, b1_info, false);
            
            Vector2 b2_pos = new Vector2(106, -242);
            Vector2 b2_size = new Vector2(164, 65);
            Action b2_action = delegate { ShowNextSaves(); };
            ButtonInfo b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Next");
            CustomUIBuilder.CreateNewButton(UISection.MP_Saves, b2_info, false);
            
            Vector2 b3_pos = new Vector2(20, -317);
            Vector2 b3_size = new Vector2(336, 65);
            Action b3_action = delegate { OpenHostMenu(); };
            ButtonInfo b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, "Back to menu");
            CustomUIBuilder.CreateNewButton(UISection.MP_Saves, b3_info, false);
            
            CustomUIManager.DisableUI(UISection.MP_Saves);
        }
        
        public static void ShowPreviousSaves()
        {
            if(saveIndex >= 4)
                saveIndex -= 4;
            
            InitializeSavesMenu();
            CustomUIManager.EnableUI(UISection.MP_Saves);
        }

        public static void ShowNextSaves()
        {
            if(saveIndex < 16)
                saveIndex += 4;

            InitializeSavesMenu();
            CustomUIManager.EnableUI(UISection.MP_Saves);
        }

        public static void ShowSaveButton(int index)
        {
            Vector2 b_pos = new Vector2(20, 58);
            Vector2 b_size = new Vector2(336, 65);

            for (int i = index; i < index+4; i++)
            {
                var _index = i;
                Action b_action = delegate { SaveButtonAction(_index); };
                ButtonInfo b_info = new ButtonInfo(b_pos, b_size, b_action, GetSaveName(_index));
                CustomUIBuilder.CreateNewButton(UISection.MP_Saves, b_info, false);
                b_pos.y -= 75;
            }
        }

        private static void OpenHostMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Saves);
            CustomUIManager.EnableUI(UISection.MP_Host);
        }

        private static string GetSaveName(int index)
        {
            int validIndex = index + 4;
            if (SavesManager.ModSaves.ContainsKey(validIndex))
            {
                if (SavesManager.ModSaves[validIndex].Name != "EmptySave")
                {
                    return SavesManager.ModSaves[validIndex].Name;
                }
            }
            return "New game";
        }

        private static void SaveButtonAction(int index)
        {
            int validIndex = index + 4;
            if (SavesManager.ModSaves[validIndex].Name != "EmptySave")
            {
                if (!ServerData.isRunning)
                    Server.Start();
                else
                {
                    Server.Stop();
                    Server.Start();
                }
                CustomLobbyMenu.saveIndex = index;
                SavesManager.LoadSave(SavesManager.ModSaves[index]);
                
                CustomUIManager.DisableUI(UISection.MP_Saves);
                CustomUIManager.EnableUI(UISection.MP_Lobby);
                
            }
            else
            {
                CustomUIManager.LockUI(CustomUIManager.currentSection);
                
                Vector2 position = new Vector2(800, 800);
                Vector2 size = new Vector2(600, 400);
                Action a1 = delegate
                {
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[0]);
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[1]);
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[2]);
                    
                    CustomUIBuilder.tmpInputWindow.Clear();
                    CustomUIManager.UnlockUI(CustomUIManager.currentSection);
                };
                
                Action a2 = delegate {  CustomUIManager.UnlockUI(CustomUIManager.currentSection); };
                CustomUIBuilder.CreateNewInputWindow(position, size, new[] { a1, a2 }, new []{"Close", "Confirm"}); 
            }
        }
    }
}