using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CMS21Together.Shared;
using HarmonyLib;
using Il2CppCMS.MainMenu;
using Il2CppCMS.MainMenu.Controls;
using Il2CppCMS.MainMenu.Sections;
using Il2CppCMS.UI.Logic;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI
{
    [HarmonyPatch]
    public static class CustomMainMenu
    {
        public static MainSection section;
        public static GameObject templateObject;
        private static GameObject multiplayerObject;
        public static Texture2D mpIcon;
        private static bool isSet;

        public static IEnumerator DefaultMenuPatch()
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForEndOfFrame();

            isSet = false;
            CustomHostMenu.isSet = false;
            CustomHostMenu.isSavesSet = false;
            CustomHostMenu.isnewSaveSet = false;
            

            GameObject.Find("Logo").gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            
            templateObject = GameObject.Find("MainMenuButton");
            RectTransform playTransform = templateObject.GetComponent<RectTransform>();

            section = GameObject.Find("MainButtons").GetComponent<MainSection>();
            List<MainMenuButton> defaultButton = new List<MainMenuButton>();
            for (int i = 0; i < section.buttons.Length; i++)
            {
                defaultButton.Add(section.buttons[i]);
            }

            section.buttons = new Il2CppReferenceArray<MainMenuButton>(31);
            for (int i = 0; i < defaultButton.Count; i++)
            {
                section.buttons[i] = defaultButton[i];
            }

            playTransform.anchoredPosition = new Vector2(-26, 58);
            playTransform.sizeDelta = new Vector2(180, 44);

            multiplayerObject = Object.Instantiate(templateObject);
            GameObject imageObj = new GameObject("Image");
            imageObj.transform.parent = multiplayerObject.transform;
            imageObj.AddComponent<Image>();

            multiplayerObject.GetComponentInChildren<Text>().gameObject.SetActive(false);

            RectTransform mpTransform = multiplayerObject.GetComponent<RectTransform>();
            MainMenuButton mpButton = multiplayerObject.GetComponent<MainMenuButton>();
            Image mpImage = imageObj.GetComponent<Image>();

            mpTransform.parent = playTransform.parent;
            mpTransform.parentInternal = playTransform.parentInternal;
            mpButton.Y = 6;
            mpButton.OnMouseHover = templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            section.buttons[6] = mpButton;

            mpTransform.anchoredPosition = new Vector2(95, 58);
            mpTransform.sizeDelta = new Vector2(53, 53);

            Texture2D myTexture = DataHelper.LoadCustomTexture("CMS21Together.Assets.peoples.png");
            Sprite mpSprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                new Vector2(0, 0));

            mpImage.sprite = mpSprite;
            imageObj.transform.position = new Vector3(0, 0, 0);
            imageObj.transform.localPosition = new Vector3(0, 0, 0);
            imageObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            
            mpButton.AssignAction(null);
            mpButton.OnClick = new MainMenuButton.ButtonEvent();

            Action openMenu = delegate { MultiplayerMenu(); };
            
            mpButton.OnClick.AddListener(openMenu);
            mpButton.AssignAction(openMenu);
            
            // Fix Singleplayer button

            section.buttons[0].OnClick = new MainMenuButton.ButtonEvent();
            Action openSinglePlayer = delegate { OpenSingleplayer(); };
            
            section.buttons[0].OnClick.AddListener(openSinglePlayer);
            section.buttons[0].AssignAction(openSinglePlayer);



        }

        private static void OpenSingleplayer()
        {
            section.gameObject.SetActive(false);
            section.transform.parent.Find("PlayButtons").gameObject.SetActive(true);
        }

        public static void MultiplayerMenu()
        {
            if (isSet)
            {
                EnableMultiplayerMenu();
                return;
            }

            for (int i = 0; i < 7; i++)
            {
                if(section.buttons[i] != null)
                    section.buttons[i].gameObject.SetActive(false);
            }
            
            var hostObject = Object.Instantiate(templateObject);
            RectTransform  hostTransform =  hostObject.GetComponent<RectTransform>();
            MainMenuButton hostButton = hostObject.GetComponent<MainMenuButton>();

            var parent = templateObject.transform.parent;
            
            hostTransform.parent = parent;
            hostTransform.parentInternal = templateObject.GetComponent<RectTransform>().parentInternal;
            hostButton.Y = 7;
            hostButton.OnMouseHover = templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            section.buttons[7] = hostButton;

            hostTransform.anchoredPosition = new Vector2(-5, 58);
            hostTransform.sizeDelta = new Vector2(288, 55);
            hostButton.GetComponentInChildren<Text>().text = "Host a game";
            hostButton.OnClick = new MainMenuButton.ButtonEvent();
            Action hostMenu = delegate { CustomHostMenu.CreateHostMenu(); };
            hostButton.OnClick.AddListener(hostMenu);
            hostObject.SetActive(true);
            
            var joinObject = Object.Instantiate(templateObject);
            RectTransform  joinTransform =  joinObject.GetComponent<RectTransform>();
            MainMenuButton joinButton = joinObject.GetComponent<MainMenuButton>();

            joinTransform.parent = parent;
            joinTransform.parentInternal = templateObject.GetComponent<RectTransform>().parentInternal;
            joinButton.Y = 8;
            joinButton.OnMouseHover = templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            section.buttons[8] = joinButton;

            joinTransform.anchoredPosition = new Vector2(-5, 10);
            joinTransform.sizeDelta = new Vector2(288, 55);
            joinButton.GetComponentInChildren<Text>().text = "Join a server";
            joinButton.OnClick = new MainMenuButton.ButtonEvent();
            Action joinMenu = delegate {  };
            joinButton.OnClick.AddListener(joinMenu);
            joinObject.SetActive(true);
            
            var networkObject = Object.Instantiate(templateObject);
            RectTransform networkTransform =  networkObject.GetComponent<RectTransform>();
            MainMenuButton networkButton = networkObject.GetComponent<MainMenuButton>();

            networkTransform.parent = parent;
            networkTransform.parentInternal = templateObject.GetComponent<RectTransform>().parentInternal;
            networkButton.Y = 9;
            networkButton.OnMouseHover = templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            section.buttons[9] = networkButton;

            networkTransform.anchoredPosition = new Vector2(-5, -38);
            networkTransform.sizeDelta = new Vector2(288, 55);
            networkButton.GetComponentInChildren<Text>().text = "Network type";
            networkButton.OnClick = new MainMenuButton.ButtonEvent();
            networkButton.isDisabled = true;
            networkButton.DoStateTransition(SelectionState.Disabled, true);
            Action networkChange = delegate {  };
            networkButton.OnClick.AddListener(networkChange);
            networkObject.SetActive(true);
            
            var settingObject = Object.Instantiate(templateObject);
            RectTransform settingTransform =  settingObject.GetComponent<RectTransform>();
            MainMenuButton settingButton = settingObject.GetComponent<MainMenuButton>();

            settingTransform.parent = parent;
            settingTransform.parentInternal = templateObject.GetComponent<RectTransform>().parentInternal;
            settingButton.Y = 10;
            settingButton.OnMouseHover = templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            section.buttons[10] = settingButton;

            settingTransform.anchoredPosition = new Vector2(-5, -152);
            settingTransform.sizeDelta = new Vector2(288, 55);
            settingButton.GetComponentInChildren<Text>().text = "Mod settings";
            settingButton.OnClick = new MainMenuButton.ButtonEvent();
            settingButton.isDisabled = true;
            settingButton.DoStateTransition(SelectionState.Disabled, true);
            Action settingChange = delegate {  };
            settingButton.OnClick.AddListener(settingChange);
            settingObject.SetActive(true);
            
            var backObject = Object.Instantiate(templateObject);
            RectTransform backTransform =  backObject.GetComponent<RectTransform>();
            MainMenuButton backButton = backObject.GetComponent<MainMenuButton>();

            backTransform.parent = parent;
            backTransform.parentInternal = templateObject.GetComponent<RectTransform>().parentInternal;
            backButton.Y = 11;
            backButton.OnMouseHover = templateObject.GetComponent<MainMenuButton>().OnMouseHover;
            section.buttons[11] = backButton;

            backTransform.anchoredPosition = new Vector2(-5, -200);
            backTransform.sizeDelta = new Vector2(288, 55);
            backButton.GetComponentInChildren<Text>().text = "Back to menu";
            backButton.OnClick = new MainMenuButton.ButtonEvent();
            Action backtoMenu = delegate { EnableMainMenu(); };
            backButton.OnClick.AddListener(backtoMenu);
            backObject.SetActive(true);
            
            
            networkButton.DoStateTransition(SelectionState.Disabled, true);
            settingButton.DoStateTransition(SelectionState.Disabled, true);

            isSet = true;
        }

        public static void EnableMultiplayerMenu()
        {
            for (int i = 0; i < section.buttons.Length; i++)
            {
                if (i > 6 && i < 12)
                {
                    if(section.buttons[i] != null)
                        section.buttons[i].gameObject.SetActive(true);
                }
                else
                {
                    if(section.buttons[i] != null)
                        section.buttons[i].gameObject.SetActive(false);
                }
            }
            section.buttons[9].isDisabled = true;
            section.buttons[9].DoStateTransition(SelectionState.Disabled, true);
            section.buttons[10].isDisabled = true;
            section.buttons[10].DoStateTransition(SelectionState.Disabled, true);
        }

        public static void EnableMainMenu()
        {
            for (int i = 0; i < section.buttons.Length; i++)
            {
                if (i < 7)
                {
                    if(section.buttons[i] != null)
                        section.buttons[i].gameObject.SetActive(true);
                }
                else
                {
                    if(section.buttons[i] != null)
                        section.buttons[i].gameObject.SetActive(false);
                }
            }
        }
        

        [HarmonyPatch(typeof(MainSection), "OnMouseHover")]
        [HarmonyPrefix]
        private static bool MainSection_OnMouseHoverFix(int y, MainSection __instance)
        {
            if (y >= 6)
            {
                if (__instance.buttons.Length >= y)
                {
                    __instance.buttons[y].Select();
                    return false;
                }
            }
            return true;
        }
    }
}