using System;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using Il2CppCMS.MainMenu;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.MainMenu.Sections;
using Il2CppCMS.MainMenu.Windows;
using Il2CppCMS.UI.Logic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUIHost
    {
        public static bool isSet;
        public static bool isSavesSet;
        public static bool isnewSaveSet;
        public static bool displaySaves;
        private static int displayedPage = 1;

        public static Dictionary<int, (int, int)> buttonPos = new Dictionary<int, (int, int)>()
        {
            { 0, (280, 58) },
            { 1, (280, 0) },
            { 2, (280, -58) },
            { 3, (280, -116) },
            { 4, (530, 58) },
            { 5, (530, 0) },
            { 6, (530, -58) },
            { 7, (530, -116) },
        };


        public static void InitializeHostMenu()
        {
            GameObject parent = new GameObject("MP_HostButtons");
            parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
            parent.transform.localPosition = new Vector3(-380, 0, 0);
            parent.transform.localScale = new Vector3(.65f, .65f, .65f);

            CustomUIManager.MP_Host_Parent = parent.transform;
            
            Vector2 b1_pos = new Vector2(20, 58);
            Vector2 b1_size = new Vector2(336, 65);
            Action b1_action = delegate { OpenSavesMenu(); };
            ButtonInfo b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Load/Create a game");
            CustomUIBuilder.CreateNewButton(UISection.MP_Host, b1_info, false);
            
            Vector2 b2_pos = new Vector2(20, -17);
            Vector2 b2_size = new Vector2(336, 65);
            Action b2_action = delegate {  };
            ButtonInfo b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Delete a game");
            CustomUIBuilder.CreateNewButton(UISection.MP_Host, b2_info, true);
            
            Vector2 b3_pos = new Vector2(20, -317);
            Vector2 b3_size = new Vector2(336, 65);
            Action b3_action = delegate { OpenMultiplayerMenu(); };
            ButtonInfo b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, "Back to menu");
            CustomUIBuilder.CreateNewButton(UISection.MP_Host, b3_info, false);
            
            CustomUIManager.DisableUI(UISection.MP_Host);
        }

        private static void OpenMultiplayerMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Host);
            CustomUIManager.EnableUI(UISection.MP_Main);
        }
        
        private static void OpenSavesMenu()
        {
            CustomUIManager.DisableUI(UISection.MP_Host);
            
            CustomUISaves.saveIndex = 0;
            CustomUIManager.EnableUI(UISection.MP_Saves);
        }

        public static void CreateSavesMenu()
        {
            if (isSavesSet)
            {
                EnableSavesMenu();
                return;
            }

            displaySaves = true;
            isSavesSet = true;
            Transform parent =  CustomUIManager.templateButton.transform;

            MainMenuManager manager = CustomUIMain.section.menuManager;
            manager.HideAds();

            int index = 0;
            for (int i = 0; i < 16; i++)
            {
                if (displayedPage == 1)
                {
                    if (i > 7)
                        break;
                }

                var saveObject = Object.Instantiate( CustomUIManager.templateButton);
                RectTransform saveTransform = saveObject.GetComponent<RectTransform>();
                MainMenuButton saveButton = saveObject.GetComponent<MainMenuButton>();

                saveTransform.parent = parent;
                saveTransform.parentInternal =
                    CustomUIManager.templateButton.GetComponent<RectTransform>().parentInternal;
                saveButton.Y = 15 + i;
                /*saveButton.OnMouseHover =
                    CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;*/
                CustomUIManager.saveButtons.Add(saveButton);

                saveTransform.anchoredPosition = new Vector2(buttonPos[i].Item1, buttonPos[i].Item2);
                saveTransform.sizeDelta = new Vector2(288, 55);
                saveButton.OnClick = new MainMenuButton.ButtonEvent();

                var index1 = index + 4; // Keep valid value for Action.
                if (SavesManager.ModSaves[index1].Name != "EmptySave")
                {
                    saveButton.GetComponentInChildren<Text>().text = SavesManager.ModSaves[index1].Name;
                    Action selectSave = delegate
                    {
                        SelectSaves(index1);
                    };
                    saveButton.OnClick.AddListener(selectSave);
                }
                else
                {
                    saveButton.GetComponentInChildren<Text>().text = "New Game";
                    int _i = i;
                    Action newSave = delegate
                    {
                        ChoseSaveName(_i, index1);
                    };
                    saveButton.OnClick.AddListener(newSave);
                }

                saveObject.SetActive(true);
                index++;
            }
        }

        private static void ChoseSaveName(int buttonIndex, int saveIndex)
        {

            if (isnewSaveSet)
            {
                EnableNewSaveWindow(buttonIndex, saveIndex);
                return;
            }

            isnewSaveSet = true;

            var window = CustomUIMain.section.transform.parent.FindChild("NameWindow");
            var saveWindow = window.GetComponentInChildren<NewSaveWindow>();
            window.gameObject.SetActive(true);

            Transform parent = window.transform;

            var cancelObject = Object.Instantiate( CustomUIManager.templateButton);
            RectTransform cancelTransform = cancelObject.GetComponent<RectTransform>();
            MainMenuButton cancelButton = cancelObject.GetComponent<MainMenuButton>();

            cancelTransform.parent = parent;
            cancelTransform.parentInternal = parent;
            cancelButton.Y = 23;
            /*cancelButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;*/
            CustomUIManager.inputFieldButtons.Add(cancelButton);

            cancelTransform.anchoredPosition = new Vector2(-68, -120);
            cancelTransform.sizeDelta = new Vector2(144, 55);
            cancelButton.GetComponentInChildren<Text>().text = "Cancel";
            cancelButton.OnClick = new MainMenuButton.ButtonEvent();
            Action backtoMenu = delegate { DisableInputsWindow(); };
            cancelButton.OnClick.AddListener(backtoMenu);
            cancelObject.SetActive(true);

            var confirmObject = Object.Instantiate( CustomUIManager.templateButton);
            RectTransform confirmsTransform = confirmObject.GetComponent<RectTransform>();
            MainMenuButton confirmButton = confirmObject.GetComponent<MainMenuButton>();

            confirmsTransform.parent = parent;
            confirmsTransform.parentInternal = parent;  
            confirmButton.Y = 24;
            /*confirmButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;*/
            CustomUIManager.inputFieldButtons.Add(confirmButton);

            confirmsTransform.anchoredPosition = new Vector2(58, -120);
            confirmsTransform.sizeDelta = new Vector2(144, 55);
            confirmButton.OnClick = new MainMenuButton.ButtonEvent();
            confirmButton.GetComponentInChildren<Text>().text = "Confirm";


            confirmButton.OnClick = new MainMenuButton.ButtonEvent();
            Action confirmAction;
            confirmAction = ConfirmAction(buttonIndex, saveIndex, saveWindow);
            confirmButton.OnClick.AddListener(confirmAction);
            confirmObject.SetActive(true);
        }
        private static Action ConfirmAction(int buttonIndex, int saveIndex, NewSaveWindow saveWindow)
        {
            return () =>
            {
                var saveName = saveWindow.inputField.text;

                if (CreateNewSave(saveIndex, saveName))
                {
                    saveWindow.gameObject.SetActive(false);
                    CustomUIManager.saveButtons[buttonIndex].GetComponentInChildren<Text>().text = saveName;
                    CustomUIManager.saveButtons[buttonIndex].OnClick = new MainMenuButton.ButtonEvent();
                    Action selectSave = delegate { SelectSaves(saveIndex); };
                    CustomUIManager.saveButtons[buttonIndex].OnClick.AddListener(selectSave);
                }
            };
        }
        private static void DisableInputsWindow()
        {
            for (int i = 0; i < CustomUIManager.inputFieldButtons.Count; i++)
            {
                if (CustomUIManager.inputFieldButtons[i] != null)
                {
                    CustomUIManager.inputFieldButtons[i].gameObject.SetActive(false);
                }
            }

            var window = CustomUIMain.section.transform.parent.FindChild("NameWindow");
            window.gameObject.SetActive(false);
        }
        private static void EnableNewSaveWindow(int buttonIndex, int saveIndex)
        {
            for (int i = 0; i < CustomUIManager.saveButtons.Count; i++)
            {
                if (CustomUIManager.saveButtons[i] != null)
                {
                    CustomUIManager.saveButtons[i].gameObject.SetActive(true);
                }
            }

            var window = CustomUIMain.section.transform.parent.FindChild("NameWindow");
                var saveWindow = window.GetComponentInChildren<NewSaveWindow>();
                window.gameObject.SetActive(true);
                saveWindow.inputField.m_Text = "SaveName";

                var confirmButton = CustomUIMain.section.buttons[25];

                confirmButton.OnClick = new MainMenuButton.ButtonEvent();
                Action confirmAction;
                confirmAction = ConfirmAction(buttonIndex, saveIndex, saveWindow);
                confirmButton.OnClick.AddListener(confirmAction);
        }
        private static bool CreateNewSave(int index, string name)
        {
            bool alreadyExist = SavesManager.ModSaves.Any(save => save.Value.Name == name);
            if (alreadyExist)
            {
                MelonLogger.Msg("A save with the same name already exists.");
            }
            else
            {
                var save = SavesManager.ModSaves[index];
                if (save.Name == "EmptySave")
                {
                    save.Name = name;
                    SavesManager.LoadSave(save);
                    return true;
                }
            }

            return false;
        }
        private static void SelectSaves(int index)
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
            CustomLobbyMenu.OpenLobby();
        }
        public static void DisableSavesMenu()
        {
            displaySaves = false;
            for (int i = 0; i < CustomUIManager.saveButtons.Count; i++)
            {
                if(CustomUIManager.saveButtons[i] != null)
                {
                    CustomUIManager.saveButtons[i].gameObject.SetActive(false);
                }
            }
            
            MainMenuManager manager = CustomUIMain.section.menuManager;
            manager.ShowAds();
        }
        private static void EnableSavesMenu()
        {
            displaySaves = true;
            for (int i = 0; i < CustomUIManager.saveButtons.Count; i++)
            {
                if(CustomUIManager.saveButtons[i] != null)
                {
                    CustomUIManager.saveButtons[i].gameObject.SetActive(true);
                }
            }
        }
        
    }

}