using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2CppCMS.MainMenu.Sections;
using Il2CppCMS.UI.Controls;
using Il2CppCMS.UI.Logic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI;

public static class CustomUIManager
{
    public static GameObject templateButton;
    public static GameObject templateText;
    public static GameObject templateInputField;
    public static GameObject templateSelector;
    
    public static List<ButtonState> V_Main_Buttons = new List<ButtonState>();
    
    public static List<ButtonState> MP_Main_Buttons = new List<ButtonState>();
    public static List<ButtonState> MP_Host_Buttons = new List<ButtonState>();
    public static List<ButtonState> MP_Saves_Buttons = new List<ButtonState>();
   
    public static List<ButtonState> MP_Lobby_Buttons = new List<ButtonState>();
    public static List<(int, GameObject)>  MP_Lobby_Addition = new List<(int, GameObject)>();
    
    public static Transform V_Main_Parent;
    public static Transform MP_Main_Parent;
    public static Transform MP_Host_Parent;
    public static Transform MP_Lobby_Parent;
    public static Transform MP_Saves_Parent;
    
    public static CustomUISection currentSection = CustomUISection.V_Main;

    public static void OnSceneChange(string scene)
    {
        if (scene == "Menu")
        {
            MelonCoroutines.Start(InitializeCustomUI());
        }
    }
    
    private static IEnumerator InitializeCustomUI()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();

        CustomUIBuilder.LoadCustomlogo();
        SavesManager.Initialize();
        
        GameObject.Find("Logo").gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        templateButton = GameObject.Find("MainMenuButton");
        templateInputField = GameObject.Find("Main").transform.GetChild(8).gameObject;
        templateText = templateButton.GetComponentInChildren<Text>().gameObject;
        templateSelector = GameObject.Find("MainMenuWindows").transform.GetChild(3).GetChild(0).gameObject
            .GetComponentInChildren<StringSelector>().gameObject;

        ResetLists();
        
        UI_Main.InitializeMainMenu();
        UI_Main.InitializeMultiplayerMenu();
        UI_Host.InitializeHostMenu();
        UI_Saves.InitializeSavesMenu();
        UI_Lobby.InitializeLobbyMenu();
    }
    
    private static void ResetLists()
    {
        V_Main_Buttons.Clear();
        MP_Lobby_Buttons.Clear();
        MP_Lobby_Addition.Clear();
        MP_Saves_Buttons.Clear();
        MP_Main_Buttons.Clear();
        MP_Host_Buttons.Clear();
    }
    
     public static void DisableUI(CustomUISection section)
        {
            if (section == CustomUISection.V_Main)
            {
                V_Main_Parent.GetComponent<MainMenuSection>().Close();
                DisableUIList(V_Main_Buttons);
            }
            else if (section == CustomUISection.MP_Main)
                DisableUIList(MP_Main_Buttons);
            else if (section == CustomUISection.MP_Host)
                DisableUIList(MP_Host_Buttons);
            else if (section == CustomUISection.MP_Lobby)
            {
                DisableUIAddition(MP_Lobby_Addition);
                DisableUIList(MP_Lobby_Buttons);
            }
            else if (section == CustomUISection.MP_Saves)
                DisableUIList(MP_Saves_Buttons);
        }
     public static void EnableUI(CustomUISection section)
     {
         if (section == CustomUISection.V_Main) 
         {
             V_Main_Parent.GetComponent<MainMenuSection>().Open();
             EnableUIList(V_Main_Buttons);
         }
         else if (section == CustomUISection.MP_Main)
             EnableUIList(MP_Main_Buttons);
         else if (section == CustomUISection.MP_Host)
             EnableUIList(MP_Host_Buttons);
         else if (section == CustomUISection.MP_Lobby)
         {
             switch (Server.Instance.isRunning)
             {
                 case false:
                     MP_Lobby_Buttons[MP_Lobby_Buttons.Count - 1].button.text.text = "Disconnect";
                     MP_Lobby_Buttons[MP_Lobby_Buttons.Count - 1].button.text.OnEnable();
                        
                     MP_Lobby_Buttons[0].button.DoStateTransition(SelectionState.Disabled, true);
                     MP_Lobby_Buttons[0].button.SetDisabled(true, true);
                     break;
                 case true:
                     MP_Lobby_Buttons[MP_Lobby_Buttons.Count - 1].button.text.text = "Back to saves";
                     MP_Lobby_Buttons[MP_Lobby_Buttons.Count - 1].button.text.OnEnable();
                        
                     MP_Lobby_Buttons[0].button.DoStateTransition(SelectionState.Normal, true);
                     MP_Lobby_Buttons[0].button.SetDisabled(false, true);
                     break;
             }
             EnableUIList(MP_Lobby_Buttons);
             EnableUIAddition(MP_Lobby_Addition);
         }
         else if (section == CustomUISection.MP_Saves)
             EnableUIList(MP_Saves_Buttons);
            
         currentSection = section;
     }
        
    public static void LockUI(CustomUISection section)
    {
        if (section == CustomUISection.V_Main)
            LockUIList(V_Main_Buttons);
        else if (section == CustomUISection.MP_Main)
            LockUIList(MP_Main_Buttons);
        else if (section == CustomUISection.MP_Host)
            LockUIList(MP_Host_Buttons);
        else if (section == CustomUISection.MP_Lobby)
            LockUIList(MP_Lobby_Buttons);
        else if (section == CustomUISection.MP_Saves)
            LockUIList(MP_Saves_Buttons);
    }
    public static void UnlockUI(CustomUISection section)
    {
        if (section == CustomUISection.V_Main)
            UnlockUIList(V_Main_Buttons);
        else if (section == CustomUISection.MP_Main)
            UnlockUIList(MP_Main_Buttons);
        else if (section == CustomUISection.MP_Host)
            UnlockUIList(MP_Host_Buttons);
        else if (section == CustomUISection.MP_Lobby)
            UnlockUIList(MP_Lobby_Buttons);
        else if (section == CustomUISection.MP_Saves)
            UnlockUIList(MP_Saves_Buttons);
    }
    
    private static void EnableUIList(List<ButtonState> buttonsToEnable)
    {
        foreach (ButtonState buttonState in buttonsToEnable)
        {
            buttonState.button.gameObject.SetActive(true);
            if (buttonState.Disabled)
            {
                buttonState.button.SetDisabled(true, true);
                buttonState.button.DoStateTransition(SelectionState.Disabled, true);
            }
        }
    }
    private static void DisableUIList(List<ButtonState> buttonsToDisable)
    {
        foreach (ButtonState buttonState in buttonsToDisable)
        {
            buttonState.button.gameObject.SetActive(false);
        }
    }
    private static void DisableUIAddition(List<GameObject> objectsToDisable)
    {
        for (var index = 0; index < objectsToDisable.Count; index++)
        {
            var obj = objectsToDisable[index];
            if (index > 3)
            {
                objectsToDisable.Remove(obj);
                Object.Destroy(obj);
            }
            else
                obj.SetActive(false);
        }
    }
    
    private static void DisableUIAddition(List<(int, GameObject)> objectsToDisable)
    {
        for (var index = 0; index < objectsToDisable.Count; index++)
        {
            var obj = objectsToDisable[index];
            if (index > 3)
            {
                objectsToDisable.Remove(obj);
                Object.Destroy(obj.Item2);
            }
            else
                obj.Item2.SetActive(false);
        }
    }
    private static void EnableUIAddition(List<GameObject> objectsToEnable)
    {
        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(true);
        }
    }
    
    private static void EnableUIAddition(List<(int,GameObject)> objectsToEnable)
    {
        foreach ((int, GameObject) obj in objectsToEnable)
        {
            obj.Item2.SetActive(true);
        }
    }
    
    private static void LockUIList(List<ButtonState> buttonsToLock)
    {
        foreach (ButtonState buttonState in buttonsToLock)
        {
            buttonState.button.SetDisabled(true, true);
            buttonState.button.DoStateTransition(SelectionState.Disabled, true);
        }
    }
    private static void UnlockUIList(List<ButtonState> buttonsToLock)
    {
        foreach (ButtonState buttonState in buttonsToLock)
        {
            buttonState.button.SetDisabled(false, true);
            buttonState.button.DoStateTransition(SelectionState.Normal, true);
        }
    }

    public static void ResetSaveUI()
    {
        for (var index = 0; index < MP_Saves_Buttons.Count; index++)
        {
            var buttonState = MP_Saves_Buttons[index];
            Object.Destroy( buttonState.button.gameObject);
        }
        MP_Saves_Buttons.Clear();
    }
    
}