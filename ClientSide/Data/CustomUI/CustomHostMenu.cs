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
    public static class CustomHostMenu
    {
        public static bool isSet;
        public static bool isSavesSet;
        public static bool isnewSaveSet;
        public static bool displaySaves;
        private static int displayedPage = 1;

        public static Dictionary<int,(int, int)> buttonPos = new Dictionary<int,(int, int)>()
        {
            {0,(280, 58)},
            {1,(280, 0)},
            {2,(280, -58)},
            {3,(280, -116)},
            {4,(530, 58)},
            {5,(530, 0)},
            {6,(530, -58)},
            {7,(530, -116)},
        };

        public static Dictionary<int, (int, Action)> buttonAction = new Dictionary<int, (int, Action)>()
        {
            
        };
        
        
        public static void DisableHostMenu()
        {
            if (CustomMainMenu.section != null)
            {
                for (int i = 12; i < 15; i++)
                {
                    if (CustomMainMenu.section.buttons[i] != null)
                    {
                        CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                    }
                }

                CustomMainMenu.EnableMultiplayerMenu();
            }
        }
        
        public static void EnableHostMenu()
        {
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 11 && i < 26)
                    {
                        if(CustomMainMenu.section.buttons[i] != null) 
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        if(CustomMainMenu.section.buttons[i] != null)
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public static void CreateHostMenu()
        {
            if (isSet)
            {
                EnableHostMenu();
                DisableSavesMenu();
                return;
            }
            
            for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
            {
                if (i < 11)
                {
                    if(CustomMainMenu.section.buttons[i] != null)
                        CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                }
            }
            isSet = true;
            Transform parent = CustomMainMenu.templateButtonObject.transform;
            
            
            var LoadObject = Object.Instantiate(CustomMainMenu.templateButtonObject);
            RectTransform  loadTransform =  LoadObject.GetComponent<RectTransform>();
            MainMenuButton  loadButton = LoadObject.GetComponent<MainMenuButton>();

            loadTransform.parent = parent;
            loadTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            loadButton.Y = 12;
            loadButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[12] = loadButton;

            loadTransform.anchoredPosition = new Vector2(-5, 58);
            loadTransform.sizeDelta = new Vector2(288, 55);
            loadButton.GetComponentInChildren<Text>().text = "Load a game";
            loadButton.OnClick = new MainMenuButton.ButtonEvent();
            Action  loadAction = delegate 
            {
                PreferencesManager.LoadAllModSaves();
                if (!displaySaves)
                {
                    if(!isSavesSet)
                        CreateSavesMenu();
                    else
                        EnableSavesMenu();
                }
                else
                {
                    DisableSavesMenu();
                }
            };
            loadButton.OnClick.AddListener(loadAction);
            LoadObject.SetActive(true);
            
            
            var backObject = Object.Instantiate(CustomMainMenu.templateButtonObject);
            RectTransform backTransform =  backObject.GetComponent<RectTransform>();
            MainMenuButton backButton = backObject.GetComponent<MainMenuButton>();

            backTransform.parent = parent;
            backTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
            backButton.Y = 13;
            backButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[13] = backButton;

            backTransform.anchoredPosition = new Vector2(-5, -200);
            backTransform.sizeDelta = new Vector2(288, 55);
            backButton.GetComponentInChildren<Text>().text = "Back to menu";
            backButton.OnClick = new MainMenuButton.ButtonEvent();
            Action backtoMenu = delegate
            {
                DisableHostMenu(); CustomMainMenu.EnableMultiplayerMenu();
                if (displaySaves)
                {
                    DisableSavesMenu();
                }

                DisableNewSaveWindow();
            };
            backButton.OnClick.AddListener(backtoMenu);
            backObject.SetActive(true);
            
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
            Transform parent = CustomMainMenu.templateButtonObject.transform;
            
            MainMenuManager manager = CustomMainMenu.section.menuManager;
            manager.HideAds();
            
            int index = 0;
            for (int i = 0; i < 16; i++)
            {
                if (displayedPage == 1)
                {
                    if(i > 7)
                        break;
                }
                
                var saveObject = Object.Instantiate(CustomMainMenu.templateButtonObject);
                RectTransform saveTransform =  saveObject.GetComponent<RectTransform>();
                MainMenuButton saveButton = saveObject.GetComponent<MainMenuButton>();

                saveTransform.parent = parent;
                saveTransform.parentInternal = CustomMainMenu.templateButtonObject.GetComponent<RectTransform>().parentInternal;
                saveButton.Y = 14+i;
                saveButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
                CustomMainMenu.section.buttons[14+i] = saveButton;
                
                saveTransform.anchoredPosition = new Vector2(buttonPos[i].Item1,buttonPos[i].Item2);
                saveTransform.sizeDelta = new Vector2(288, 55);
                saveButton.OnClick = new MainMenuButton.ButtonEvent();

                var index1 = index + 4; // Keep valid value for Action.
                if (SavesManager.ModSaves[index1].Name != "EmptySave")
                {
                    MelonLogger.Msg($"Save: {SavesManager.ModSaves[index1].Name}");
                    saveButton.GetComponentInChildren<Text>().text = SavesManager.ModSaves[index1].Name;
                    
                    Action selectSave = delegate
                    {
                        SelectSaves(index1); 
                        CustomLobbyMenu.saveIndex = index1;
                        CustomLobbyMenu.OpenLobby();
                    };
                    saveButton.OnClick.AddListener(selectSave);
                    
                    saveButton.isDisabled = false;
                    saveButton.DoStateTransition(SelectionState.Normal, true);
                    
                }
                else
                {
                    saveButton.GetComponentInChildren<Text>().text = "New Game";
                    int _i = i;
                    Action newSave = delegate 
                    { 
                        ChoseSaveName(14+_i,index1);  
                        CustomLobbyMenu.OpenLobby();
                        
                    };
                    saveButton.OnClick.AddListener(newSave);
                }
                saveObject.SetActive(true);
                index++;
            }
        }

        private static void ChoseSaveName(int buttonIndex, int saveIndex)
        {
            string saveName = "";
            
            if (isnewSaveSet)
            {
                EnableNewSaveWindow(buttonIndex, saveName, saveIndex);
                return;   
            }

            isnewSaveSet = true;
            
            var window = CustomMainMenu.section.transform.parent.FindChild("NameWindow");
            var saveWindow = window.GetComponentInChildren<NewSaveWindow>();
            window.gameObject.SetActive(true);
            
            Transform parent = window.transform;
            
            var cancelObject = Object.Instantiate(CustomMainMenu.templateButtonObject);
            RectTransform cancelTransform =  cancelObject.GetComponent<RectTransform>();
            MainMenuButton cancelButton = cancelObject.GetComponent<MainMenuButton>();

            cancelTransform.parent = parent;
            cancelTransform.parentInternal = parent;
            cancelButton.Y = 23;
            cancelButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[23] = cancelButton;
                
            cancelTransform.anchoredPosition = new Vector2(-68,-120);
            cancelTransform.sizeDelta = new Vector2(144, 55);
            cancelButton.OnClick = new MainMenuButton.ButtonEvent();
            cancelButton.GetComponentInChildren<Text>().text = "Cancel";
            cancelButton.OnClick = new MainMenuButton.ButtonEvent();
            Action backtoMenu = delegate
            {
                DisableNewSaveWindow();
            };
            cancelButton.OnClick.AddListener(backtoMenu);
            cancelObject.SetActive(true);
            
            var confirmObject = Object.Instantiate(CustomMainMenu.templateButtonObject);
            RectTransform confirmsTransform =  confirmObject.GetComponent<RectTransform>();
            MainMenuButton confirmButton = confirmObject.GetComponent<MainMenuButton>();

            confirmsTransform.parent = parent;
            confirmsTransform.parentInternal = parent;
            confirmButton.Y = 24;
            confirmButton.OnMouseHover = CustomMainMenu.templateButtonObject.GetComponent<MainMenuButton>().OnMouseHover;
            CustomMainMenu.section.buttons[24] = confirmButton;
                
            confirmsTransform.anchoredPosition = new Vector2(58,-120);
            confirmsTransform.sizeDelta = new Vector2(144, 55);
            confirmButton.OnClick = new MainMenuButton.ButtonEvent();
            confirmButton.GetComponentInChildren<Text>().text = "Confirm";
            
            
            confirmButton.OnClick = new MainMenuButton.ButtonEvent();
            Action confirmAction;
            confirmAction = ConfirmAction(buttonIndex,saveName, saveIndex, saveWindow);
            confirmButton.OnClick.AddListener(confirmAction);
            confirmObject.SetActive(true);
        }

        private static Action ConfirmAction(int buttonIndex,string saveName, int saveIndex, NewSaveWindow saveWindow)
        {
            return () =>
            {
                saveName = saveWindow.inputField.m_Text;
                saveWindow.gameObject.SetActive(false); 
                    
                CustomMainMenu.section.buttons[buttonIndex].GetComponentInChildren<Text>().text = saveName;
                CreateNewSave(saveIndex, saveName);
                
                CustomMainMenu.section.buttons[buttonIndex].OnClick = new MainMenuButton.ButtonEvent();
                Action selectSave = delegate { SelectSaves(saveIndex); };
                CustomMainMenu.section.buttons[buttonIndex].OnClick.AddListener(selectSave);
            };
        }

        private static void DisableNewSaveWindow()
        {
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 22 && i <= 25)
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                        }
                    }
                }
                var window = CustomMainMenu.section.transform.parent.FindChild("NameWindow");
                window.gameObject.SetActive(false);
            }
        }

        private static void EnableNewSaveWindow(int buttonIndex, string saveName, int saveIndex)
        {
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 22 && i <= 25)
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(true);

                        }
                    }
                }
                var window = CustomMainMenu.section.transform.parent.FindChild("NameWindow");
                var saveWindow = window.GetComponentInChildren<NewSaveWindow>();
                window.gameObject.SetActive(true);
                
                var confirmButton = CustomMainMenu.section.buttons[25];
                
                confirmButton.OnClick = new MainMenuButton.ButtonEvent();
                Action confirmAction;
                confirmAction = ConfirmAction(buttonIndex,saveName, saveIndex, saveWindow);
                confirmButton.OnClick.AddListener(confirmAction);
            }
        }

        private static void CreateNewSave(int index, string name)
        {
            bool alreadyExist = SavesManager.ModSaves.Any(save => save.Value.Name == name);
            if(alreadyExist) { MelonLogger.Msg("A save with the same name already exists."); }
            else
            {
                    var save = SavesManager.ModSaves[index];
                    if (save.Name == "EmptySave")
                    {
                        MelonLogger.Msg("Found a valid save slot!: " + index);
                        SavesManager.LoadSave(index, name);
                    }
            }
        }

        private static void SelectSaves(int index)
        {
                                        
            if(!ServerData.isRunning)
                Server.Start();
            else
            {
                Server.Stop();
                Server.Start();
            }
                            
            SavesManager.LoadSave(SavesManager.ModSaves[index].saveIndex, SavesManager.ModSaves[index].Name);
        }
        
        public static void DisableSavesMenu()
        {
            displaySaves = false;
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i >= 14 && i <= 25)
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
            MainMenuManager manager = CustomMainMenu.section.menuManager;
            manager.ShowAds();
        }

        private static void EnableSavesMenu()
        {
            displaySaves = true;
            if (CustomMainMenu.section != null)
            {
                for (int i = 0; i < CustomMainMenu.section.buttons.Length; i++)
                {
                    if (i > 11 && i < 26)
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(true);

                        }
                    }
                    else
                    {
                        if (CustomMainMenu.section.buttons[i] != null)
                        {
                            CustomMainMenu.section.buttons[i].gameObject.SetActive(false);
                        }
                    }
                }
            }

            MainMenuManager manager = CustomMainMenu.section.menuManager;
            manager.HideAds();
        }
    }
    
}