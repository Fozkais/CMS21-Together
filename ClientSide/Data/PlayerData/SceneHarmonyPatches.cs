using System;
using CMS21Together.ClientSide.Handle;
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
            }
        }
    }
}