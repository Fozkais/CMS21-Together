using System;
using CMS21MP.ClientSide.Data;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public static class ClientDebug
    {


        public static void HandleDebug()
        {
            UnlockCamCursor();
            SaveLogsOnAltF4();
            ResyncInventory();
            ResyncAllCar();
            ForceDisableFade();
        }

        private static void UnlockCamCursor()
        {
            if (Input.GetKeyDown(KeyCode.RightControl)) //Debug Mounting part simulteanously
            {
                Cursor3D.Get().BlockCursor(false);
            }
        }

        private static void SaveLogsOnAltF4()
        {
            if (Input.GetKeyDown(KeyCode.F4) && Input.GetKeyDown(KeyCode.LeftAlt))
            {
                PreferencesManager.SaveMelonLog();
            }
        }

        private static void ForceDisableFade()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MelonLogger.Msg("Remove Fade!!");
                ScreenFader.Get().NormalFadeOut();
            }
        }

        private static void ResyncInventory()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MelonLogger.Msg("Resync inventory!!");
                ModInventory.handledItem.Clear();
                ModInventory.handledGroupItem.Clear();
                GameData.localInventory.DeleteAllInventory();

                ClientSend.SendInventoryItem(new ModItem(), false, true);
                ClientSend.SendInventoryGroupItem(new ModGroupItem(), false, true);
            }
        }

        private static void ResyncAllCar()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MelonLogger.Msg("ResyncAllCar!!");
                ClientData.carOnScene.Clear();

                foreach (CarLoader loader in GameData.carLoaders)
                {
                    if (!String.IsNullOrEmpty(loader.carToLoad))
                        loader.DeleteCar();
                }
                
                ClientSend.SendCarInfo(new ModCar(), true);
            }
        }
    }
}