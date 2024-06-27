using System;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.UI.Controls;
using Il2CppCMS.UI.Logic;
using Il2CppSystem.Globalization;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    public static class CustomUIBuilder
    {
        private static int CurrentButtonIndex = 6;

        public static List<GameObject> tmpInputWindow = new List<GameObject>();

        
        public static void CreateSaveInfoPanel(ModSaveData saveData)
        {
            var saveInfoObject = new GameObject("SaveInfoWindow");
            
            var img = saveInfoObject.AddComponent<Image>();
            img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
            img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);
            img.color = new Color(  .031f, .027f, .033f  , 0.85f);
            img.rectTransform.sizeDelta =  new Vector2(600, 700);
            img.rectTransform.anchoredPosition =  new Vector2(460, -50);

            Vector2 t1_pos = new Vector2(620, 315);
            Vector2 t1_size = new Vector2(400, 100);
            CreateText(t1_pos, t1_size, "Save Info", 16, saveInfoObject.transform);
            
            var splitter1 = new GameObject("splitter");
            var splitter1Img = splitter1.AddComponent<Image>();
            splitter1Img.rectTransform.parent = saveInfoObject.transform;
            splitter1Img.rectTransform.parentInternal = saveInfoObject.transform;
            splitter1Img.color = new Color(  1f, 1f, 1f  , 0.5f);
            splitter1Img.rectTransform.sizeDelta =  new Vector2(580, 2);
            splitter1Img.rectTransform.anchoredPosition =  new Vector2(0, 280);

            if (saveData.selectedGamemode == Gamemode.none)
                saveData.selectedGamemode = SavesManager.GetGamemodeFromDifficulty(SavesManager.profileData[saveData.saveIndex].Difficulty);

            string time;
            var timePlayed = TimeSpan.FromMinutes(SavesManager.profileData[saveData.saveIndex].PlayTime);
            if (timePlayed.TotalHours >= 1.0)
            {
                time = $"{Math.Round(timePlayed.TotalHours)} h";
            }
            else if (!(timePlayed.TotalMinutes < 1.0))
            {
                time = $"{Math.Round(timePlayed.TotalMinutes)} min";
            }
            else
            {
             time = "1 min";
            }
            
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = GlobalData.DefaultCultureInfo;
            string lastSave = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(SavesManager.profileData[saveData.saveIndex].LastSave)).ToLocalTime().DateTime.ToString("g");
            CultureInfo.CurrentCulture = currentCulture;
            
            Vector2 t2_pos = new Vector2(620, 230);
            Vector2 t2_size = new Vector2(400, 100);
            CreateText(t2_pos, t2_size, "Name : " + saveData.Name, 14, saveInfoObject.transform);
            
            Vector2 t3_pos = new Vector2(620, 180);
            Vector2 t3_size = new Vector2(400, 100);
            CreateText(t3_pos, t3_size, "Gamemode : " + saveData.selectedGamemode.ToString(), 14, saveInfoObject.transform);
            
            Vector2 t4_pos = new Vector2(620, 130);
            Vector2 t4_size = new Vector2(400, 100);
            CreateText(t4_pos, t4_size, "Time Played : " +  time, 14, saveInfoObject.transform);
            
            Vector2 t5_pos = new Vector2(620, 80);
            Vector2 t5_size = new Vector2(400, 100);
            CreateText(t5_pos, t5_size, "Last save : " +  lastSave, 14, saveInfoObject.transform);
            
            var splitter2 = Object.Instantiate(splitter1, saveInfoObject.transform, true);
            var splitter2Img = splitter2.GetComponent<Image>();
            splitter2Img.rectTransform.sizeDelta =  new Vector2(580, 2);
            splitter2Img.rectTransform.anchoredPosition =  new Vector2(0, 30);
            
            tmpInputWindow.Add(saveInfoObject);
        }

        public static void BuildLobbyHeader()
        {
            var lobbyHeaderObject = new GameObject("LobbyHeader");
            var img = lobbyHeaderObject.AddComponent<Image>();
            img.rectTransform.parent = GetParentFromSection(UISection.MP_Lobby);
            img.rectTransform.parentInternal = GetParentFromSection(UISection.MP_Lobby);
            
            img.color = new Color(  .031f, .027f, .033f  , 0.85f);
            img.rectTransform.sizeDelta = new Vector2(600, 75);
            img.rectTransform.anchoredPosition = new Vector2(600, 175);
            
            CustomUIManager.MP_Lobby_Addition.Add(lobbyHeaderObject);
            
            Vector2 t1_pos = new Vector2(500, 0);
            Vector2 t1_size = new Vector2(400, 100);
            CustomUIManager.MP_Lobby_Addition.Add(CreateText(t1_pos, t1_size, "Player", 16, lobbyHeaderObject.transform));
            
            Vector2 t2_pos = new Vector2(680, 0);
            Vector2 t2_size = new Vector2(400, 100);
            CustomUIManager.MP_Lobby_Addition.Add(CreateText(t2_pos, t2_size, "Ready State", 16, lobbyHeaderObject.transform));
            
            Vector2 t3_pos = new Vector2(940, 0);
            Vector2 t3_size = new Vector2(400, 100);
            CustomUIManager.MP_Lobby_Addition.Add(CreateText(t3_pos, t3_size, "Ping", 16, lobbyHeaderObject.transform));
            
            CustomUIManager.MP_Lobby_Addition[0].SetActive(true);
            CustomUIManager.MP_Lobby_Addition[1].SetActive(true);
            CustomUIManager.MP_Lobby_Addition[2].SetActive(true);
            CustomUIManager.MP_Lobby_Addition[3].SetActive(true);
        }

        public static StringSelector CreateNewChoiceButton(Vector2 position, Vector2 size, string[] choices, Transform parent)
        {
            var selectorObject = Object.Instantiate(CustomUIManager.templateChoiceButton);
            var rectTransform = selectorObject.GetComponent<RectTransform>();
            
            rectTransform.parent = parent;
            rectTransform.parentInternal = parent;

            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            StringSelector selector = selectorObject.GetComponent<StringSelector>();
            selector.options = new Il2CppSystem.Collections.Generic.List<string>();
            for (int i = 0; i < choices.Length; i++)
                selector.options.Add(choices[i]);

            return selector;
        }
        
        
        public static void CreateNewInputWindow(Vector2 position, Vector2 size, Action[] actions, string[] texts, InputFieldType type)
        {
            var inputFieldObject = new GameObject("InputFieldWindow");
            var img = inputFieldObject.AddComponent<Image>();
            img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
            img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);
            
            img.color = new Color(  .031f, .027f, .033f  , 0.85f);
            img.rectTransform.sizeDelta = size;
            img.rectTransform.anchoredPosition = position;
            
            tmpInputWindow.Add(inputFieldObject);

            Vector2 b1_pos = new Vector2(-200, -100);
            Vector2 b1_size = new Vector2(168, 65);
            Action b1_action = actions[0];
            ButtonInfo buttonInfo1 = new ButtonInfo(b1_pos, b1_size, b1_action, texts[0], inputFieldObject.transform);
            tmpInputWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo1, false, null,false).button.gameObject);
            
            Vector2 b2_pos = new Vector2(200, -100);
            Vector2 b2_size = new Vector2(168, 65);
            Action b2_action = actions[1];
            ButtonInfo buttonInfo = new ButtonInfo(b2_pos, b2_size, b2_action, texts[1], inputFieldObject.transform);
            tmpInputWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo, false,null,false).button.gameObject);

           

            if (type == InputFieldType.newSave)
            {
                Vector2 t1_pos = new Vector2(660, 130);
                Vector2 t1_size = new Vector2(400, 100);
                tmpInputWindow.Add(CreateText(t1_pos, t1_size, "Enter save name :", 16, inputFieldObject.transform));
                
                Vector2 t2_pos = new Vector2(660, 5);
                Vector2 t2_size = new Vector2(400, 100);
                tmpInputWindow.Add(CreateText(t2_pos, t2_size, "Gamemode :", 16, inputFieldObject.transform));
                
                Vector2 s1_pos = new Vector2(-180, 0);
                Vector2 s1_size = new Vector2(400, 100);
                StringSelector selector = CreateNewChoiceButton(s1_pos, s1_size, new[] { "Sandbox", "Campaign" }, inputFieldObject.transform);
                tmpInputWindow.Add(selector.gameObject);
                selector.gameObject.SetActive(true);
                selector.EnableArrows();
                selector.SetValue(0);
                selector.optionText.resizeTextMaxSize = 24;
                selector.optionText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-125, 2.5f);
                selector.rightArrow.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, 0);
                
                Vector2 i1_pos = new Vector2(0, 260);
                Vector2 i1_size = new Vector2(400, 100);
                tmpInputWindow.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform)); // need to be the last added
            }
            else
            {
                Vector2 t1_pos = new Vector2(660, 100);
                Vector2 t1_size = new Vector2(400, 100);
                tmpInputWindow.Add(CreateText(t1_pos, t1_size, "Enter username :", 16, inputFieldObject.transform));
                Vector2 i1_pos = new Vector2(0, 175);
                Vector2 i1_size = new Vector2(400, 100);
                tmpInputWindow.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform)); // need to be the last added
            }
            
           
            
            tmpInputWindow[1].SetActive(true);
            tmpInputWindow[2].SetActive(true);
        }
        
        public static void CreateNewDoubleInputWindow(Vector2 position, Vector2 size, Action[] actions, string[] texts)
        {
            var inputFieldObject = new GameObject("DoubleInputFieldWindow");
            var img = inputFieldObject.AddComponent<Image>();
            img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
            img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);
            
            img.color = new Color(  .031f, .027f, .033f  , 0.85f);
            img.rectTransform.sizeDelta = size;
            img.rectTransform.anchoredPosition = position;
            
            tmpInputWindow.Add(inputFieldObject);

            Vector2 b1_pos = new Vector2(-200, -150);
            Vector2 b1_size = new Vector2(168, 65);
            Action b1_action = actions[0];
            ButtonInfo buttonInfo1 = new ButtonInfo(b1_pos, b1_size, b1_action, texts[0], inputFieldObject.transform);
            tmpInputWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo1, false, null,false).button.gameObject);
            
            Vector2 b2_pos = new Vector2(200, -150);
            Vector2 b2_size = new Vector2(168, 65);
            Action b2_action = actions[1];
            ButtonInfo buttonInfo = new ButtonInfo(b2_pos, b2_size, b2_action, texts[1], inputFieldObject.transform);
            tmpInputWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo, false,null,false).button.gameObject);
            
            Vector2 t1_pos = new Vector2(530, 140);
            Vector2 t1_size = new Vector2(400, 100);
            tmpInputWindow.Add(CreateText(t1_pos, t1_size, "Enter server address :", 16, inputFieldObject.transform));
            
            Vector2 t2_pos = new Vector2(530, 20);
            Vector2 t2_size = new Vector2(400, 100);
            tmpInputWindow.Add(CreateText(t2_pos, t2_size, "Enter username :", 16, inputFieldObject.transform));
            
            Vector2 i1_pos = new Vector2(0, 250);
            Vector2 i1_size = new Vector2(400, 100);
            tmpInputWindow.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform)); // need to be the last added
            
            Vector2 i2_pos = new Vector2(0, 130);
            Vector2 i2_size = new Vector2(400, 100);
            tmpInputWindow.Add(CreateNewInputField(i2_pos, i2_size, inputFieldObject.transform));
            
            tmpInputWindow[1].SetActive(true);
            tmpInputWindow[2].SetActive(true);
        }

        public static GameObject CreateNewInputField(Vector2 position, Vector2 size,Transform parent)
        {
            var inputFieldObject = Object.Instantiate(CustomUIManager.templateInputField, parent, true);
            GameObject inputField = null;
            
            for (int i = 0; i < inputFieldObject.transform.childCount; i++)
            {
                GameObject obj = inputFieldObject.transform.GetChild(i).gameObject;
                if (obj.name != "InputField")
                    Object.Destroy(obj);
                else
                    inputField = obj;
            }

            inputFieldObject.GetComponent<Image>().enabled = false;
            
            inputFieldObject.GetComponent<RectTransform>().anchoredPosition = position;
            inputFieldObject.GetComponent<RectTransform>().sizeDelta = size;


            /*MelonLogger.Msg($"ChildrenCount = {inputFieldObject.transform.childCount}");
            if (inputField != null)
            {
                MelonLogger.Msg($"ChildrenCount = {inputField.transform.childCount}");
                var placeHolder = inputField.GetComponentsInChildren<Text>()[0].gameObject;
                if (placeHolder != null)
                {
                    placeHolder.GetComponent<Text>().text = placeholderText; // <- was a parameter
                    placeHolder.SetActive(true);
                }
                
                inputField.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }*/
            
            inputFieldObject.SetActive(true);
            return inputFieldObject;
        }

        public static GameObject CreateText(Vector2 position, Vector2 size, string text, int fontSize, Transform parent)
        {
            GameObject textObject = Object.Instantiate(CustomUIManager.templateText, parent, true);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.sizeDelta = size;
            textRect.localPosition = position;

            textObject.GetComponent<Text>().text = text;
            textObject.GetComponent<Text>().fontSize = fontSize;

            return textObject;
        }

        public static GameObject CreateImage(ButtonImage image, Transform parent)
        {
            var imgObject = new GameObject("Image");
            imgObject.transform.parent = parent;
            imgObject.AddComponent<Image>();
            imgObject.GetComponent<Image>().sprite = image.sprite;
            imgObject.transform.position = image.position;
            imgObject.transform.localPosition = image.position;
            imgObject.transform.localScale = image.scale;

            return imgObject;
        }
        
        public static ButtonState CreateNewButton(UISection section, ButtonInfo buttonInfo, bool disabled, ButtonImage image=null, bool save=true)
        {
            GameObject buttonObject = Object.Instantiate(CustomUIManager.templateButton);
            RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
            MainMenuButton button = buttonObject.GetComponent<MainMenuButton>();

            if (buttonInfo.customParent == null)
            {
                buttonTransform.parent = GetParentFromSection(section);
                buttonTransform.parentInternal = GetParentFromSection(section);
            }
            else
            {
                buttonTransform.parent = buttonInfo.customParent;
                buttonTransform.parentInternal = buttonInfo.customParent;
            }
            
            button.Y = CurrentButtonIndex;
            CurrentButtonIndex += 1;
            
            buttonTransform.anchoredPosition = buttonInfo.position;
            buttonTransform.sizeDelta = buttonInfo.size;
            /*button.OnMouseHover = CustomUIManager.templateButton.GetComponent<MainMenuButton>().OnMouseHover; TODO: Fix MouseHover ?*/
            
            button.OnClick = new MainMenuButton.ButtonEvent();
            button.OnClick.AddListener(buttonInfo.action);

            if (image != null)
            {
                buttonObject.GetComponentInChildren<Text>().gameObject.SetActive(false);
                CreateImage(image, buttonObject.transform);
            }
            else
            {
                buttonObject.GetComponentInChildren<Text>().text = buttonInfo.text;
            }

            if (disabled)
            {
                button.DoStateTransition(SelectionState.Disabled, true);
                button.SetDisabled(true, true);
            }

            var buttonState = new ButtonState()
            {
                button = button,
                Disabled = disabled
            };

            buttonObject.transform.localScale = new Vector3(1, 1, 1);
            
            if(save)
                AddButtonToSection(buttonState, section);
            
            return buttonState;
        }

        public static void AddButtonToSection(ButtonState buttonState,UISection section)
        {
            if(section == UISection.V_Main)
                CustomUIManager.V_Main_Buttons.Add(buttonState);
            else if(section == UISection.MP_Main)
                CustomUIManager.MP_Main_Buttons.Add(buttonState);
            else if(section == UISection.MP_Host)
                CustomUIManager.MP_Host_Buttons.Add(buttonState);
            else if(section == UISection.MP_Lobby)
                CustomUIManager.MP_Lobby_Buttons.Add(buttonState);            
            else if(section == UISection.MP_Saves)
                CustomUIManager.MP_Saves_Buttons.Add(buttonState);
            else
                MelonLogger.Msg("[VanillaUIModifier->AddButtonToSection] section is not valid.");
        }

        public static Transform GetParentFromSection(UISection section)
        {
            if (section == UISection.V_Main)
                return CustomUIManager.V_Main_Parent;
            else if (section == UISection.MP_Main)
                return CustomUIManager.MP_Main_Parent;
            else if (section == UISection.MP_Host)
                return CustomUIManager.MP_Host_Parent;
            else if (section == UISection.MP_Lobby)
                return CustomUIManager.MP_Lobby_Parent;
            else if (section == UISection.MP_Saves)
                return CustomUIManager.MP_Saves_Parent;
            else
                MelonLogger.Msg("[VanillaUIModifier->GetParentFromSection] section is not valid.");
            
            return null;
        }
    }

    public enum InputFieldType
    {
        newSave,
        username
    }
    public class ButtonImage
    {
        public Sprite sprite;
        public Vector3 position;
        public Vector3 scale;
    }

    public class ButtonInfo
    {
        public Vector2 position;
        public Vector2 size;
        public Action action;
        public string text;
        public Transform customParent;

        public ButtonInfo(Vector2 _position, Vector2 _size, Action _action, string _text, Transform _customParent=null)
        {
            position = _position;
            size = _size;
            action = _action;
            text = _text;
            customParent = _customParent;
        }
    }

    public struct ButtonState
    {
        public MainMenuButton button;
        public bool Disabled;
        public bool isPressed;
    }

    public enum UISection
    {
        V_Main,
        MP_Main,
        MP_Host,
        MP_Lobby,
        MP_Saves
    }
}