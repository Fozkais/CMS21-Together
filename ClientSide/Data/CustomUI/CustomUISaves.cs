using System;
using System.Linq;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.UI.Controls;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
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
            for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                    
            CustomUIBuilder.tmpInputWindow.Clear();
            
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
            if (GetSaveName(index) != "New game")
            {
                for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                CustomUIBuilder.tmpInputWindow.Clear();
                
                CustomUIBuilder.CreateSaveInfoPanel(SavesManager.ModSaves[index+4]);
                /*Vector2 position = new Vector2(600, 0);
                Vector2 size = new Vector2(600, 300);
                Action a1 = delegate
                {
                    for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                        Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);



                    CustomUIBuilder.tmpInputWindow.Clear();
                    CustomUIManager.UnlockUI(CustomUIManager.currentSection);
                };
                Action a2 = delegate
                {

                    var _index = CustomUIBuilder.tmpInputWindow.Count - 1;
                    InputField inputField = CustomUIBuilder.tmpInputWindow[_index].GetComponentInChildren<InputField>();
                    string username = inputField.text;

                    Client.Instance.username = username;
                    PreferencesManager.SavePreferences();

                    for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                        Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);

                    CustomUIBuilder.tmpInputWindow.Clear();
                    CustomUIManager.UnlockUI(CustomUIManager.currentSection);

                    if (!ServerData.isRunning)
                        Server.Start();
                    else
                    {
                        Server.Stop();
                        Server.Start();
                    }
                    CustomUILobby.saveIndex = index+4;
                    SavesManager.LoadSave(SavesManager.ModSaves[index+4]);

                    CustomUIManager.DisableUI(UISection.MP_Saves);
                    CustomUIManager.EnableUI(UISection.MP_Lobby);
                };
                CustomUIBuilder.CreateNewInputWindow(position, size, new[] { a1, a2 }, new []{"Close", "Confirm"}, InputFieldType.username); */

            }
            else
            {
                for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                    Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                    
                CustomUIBuilder.tmpInputWindow.Clear();
                
                CustomUIManager.LockUI(CustomUIManager.currentSection);
                
                Vector2 position = new Vector2(600, 0);
                Vector2 size = new Vector2(600, 300);
                Action a1 = delegate
                {
                    for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                        Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                    
                    CustomUIBuilder.tmpInputWindow.Clear();
                    CustomUIManager.UnlockUI(CustomUIManager.currentSection);
                };
                Action a2 = delegate
                {
                    var _index = CustomUIBuilder.tmpInputWindow.Count - 1;
                    
                    InputField inputField = CustomUIBuilder.tmpInputWindow[_index].GetComponentInChildren<InputField>();
                    StringSelector selector = CustomUIBuilder.tmpInputWindow[_index-1].GetComponentInChildren<StringSelector>();
                    string saveName = inputField.text;
                    int selectectGamemode = selector.Current;

                    if (SavesManager.ModSaves.Any(save => save.Value.Name == saveName))
                    {
                        MelonLogger.Msg("A save with the same name already exists."); // TODO: add a info window to display on game
                        return;
                    }

                    SavesManager.ModSaves[index+4].Name = saveName;
                    SavesManager.ModSaves[index + 4].selectedGamemode = SavesManager.GetGamemodeFromInt(selectectGamemode);
                    CustomUIManager.MP_Saves_Buttons[index].button.GetComponentInChildren<Text>().text = SavesManager.ModSaves[index+4].Name;
                    CustomUIManager.MP_Saves_Buttons[index].button.OnEnable();
                    
                    for (int i = 0; i < CustomUIBuilder.tmpInputWindow.Count; i++)
                        Object.Destroy(CustomUIBuilder.tmpInputWindow[i]);
                    
                    CustomUIBuilder.tmpInputWindow.Clear();
                    CustomUIManager.UnlockUI(CustomUIManager.currentSection);
                };
                
                CustomUIBuilder.CreateNewInputWindow(position, size, new[] { a1, a2 }, new []{"Close", "Confirm"}, InputFieldType.newSave); 
            }
        }
    }
}