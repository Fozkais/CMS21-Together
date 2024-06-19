using System;
using System.Collections.Generic;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.UI.Logic;
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

        public static void CreateNewInputWindow(Vector2 position, Vector2 size, Action[] actions, string[] texts)
        {
            var imgObject = new GameObject("Image");
            var img = imgObject.AddComponent<Image>();
            img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
            img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);
            
            img.color = new Color(  .031f, .027f, .033f  , 1);
            img.rectTransform.sizeDelta = size;
            img.rectTransform.anchoredPosition = position;
            
            tmpInputWindow.Add(imgObject);
            
            Vector2 b1_pos = position;
            Vector2 b1_size = new Vector2(168, 65);
            Action b1_action = actions[0];
            ButtonInfo buttonInfo1 = new ButtonInfo(b1_pos, b1_size, b1_action, texts[0], imgObject.transform);
            tmpInputWindow.Add(CustomUIBuilder.CreateNewButton(CustomUIManager.currentSection, buttonInfo1, false, null,false).button.gameObject);
            
            Vector2 b2_pos = position + new Vector2(110, 0);
            Vector2 b2_size = new Vector2(168, 65);
            Action b2_action = actions[1];
            ButtonInfo buttonInfo = new ButtonInfo(b2_pos, b2_size, b2_action, texts[1], imgObject.transform);
            tmpInputWindow.Add( CustomUIBuilder.CreateNewButton(CustomUIManager.currentSection, buttonInfo, false,null,false).button.gameObject);
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
                var imgObject = new GameObject("Image");
                imgObject.transform.parent = buttonObject.transform;
                imgObject.AddComponent<Image>();
                imgObject.GetComponent<Image>().sprite = image.sprite;
                imgObject.transform.position = image.position;
                imgObject.transform.localPosition = image.position;
                imgObject.transform.localScale = image.scale;
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