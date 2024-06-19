using System;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    [HarmonyPatch]
    public class SceneHarmonyPatches
    {
        [HarmonyPatch(typeof(NotificationCenter), "SelectSceneToLoad", 
            new Type[]{ typeof(string), typeof(SceneType), typeof(bool), typeof(bool)})]
        [HarmonyPrefix]
        public static void SceneChangePatch( string newSceneName, SceneType sceneType, bool useFader, bool saveGame)
        {
            if (Client.Instance.isConnected || ServerData.isRunning)
            {
                MelonLogger.Msg("Going to : " + newSceneName);
                if (newSceneName == "Menu")
                {
                    Client.Instance.Disconnect();
                    if(ServerData.isRunning)
                        Server.Stop();
                    Application.runInBackground = false;
                }

                if (newSceneName != "garage")
                {
                    if (ClientData.Instance != null)
                    {
                        ClientData.Instance.engineStand.needToResync = true;
                        ClientData.Instance.engineStand.engine = null;
                        ClientData.Instance.engineStand.Groupengine = null;
                        ClientData.Instance.engineStand.isReferenced = false;
                        ClientData.Instance.engineStand.isHandled = false;
                        ClientData.Instance.engineStand.engineStandParts.Clear();
                        ClientData.Instance.engineStand.engineStandPartsReferences.Clear();
                    }
                    
                    
                    ModInventory.handledGroupItem.Clear();
                    ModInventory.handledItem.Clear();
                }
            }
        }
    }
}