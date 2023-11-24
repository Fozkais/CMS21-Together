using CMS21MP.ClientSide.Data;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using Il2Cpp;
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

        private static void ResyncInventory()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ModInventory.handledItem.Clear();
                ModInventory.handledGroupItem.Clear();
                GameData.localInventory.DeleteAllInventory();

                ClientSend.SendInventoryItem(new ModItem(), false, true);
                ClientSend.SendInventoryGroupItem(new ModGroupItem(), false, true);
            }
        }
    }
}