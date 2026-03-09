using System;
using System.IO;
using System.Linq;
using System.Text;
using CMS.ContainersSave;
using CMS.Platforms;
using CMS.Platforms.Steam;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using UnhollowerBaseLib;
using UnityEngine;

// ReSharper disable EmptyGeneralCatchClause

namespace CMS21Together.ClientSide.Data;

[HarmonyPatch]
public static class SaveSystemHooks
{
    [HarmonyPatch(typeof(ProfileData), nameof(ProfileData.SerializeToBytes))]
    [HarmonyPrefix]
    public static bool SerializePrefix(ProfileData __instance, ref Il2CppStructArray<byte> __result)
    {
        if (Client.Instance.isConnected && !Server.Instance.isRunning) return false;
        return true;
    }
    
    [HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.Save))]
    [HarmonyPrefix]
    public static bool SaveFix(GameDataManager __instance, int profileID)
    {
        if (profileID <= 3) return true;
    
        try
        {
            __instance.ProfileData[profileID].LastUID = UIDManager.GetDataForSave();
            __instance.ProfileData[profileID].BuildVersion = GameSettings.BuildVersion;
            byte[] originalData = __instance.ProfileData[profileID].SerializeToBytes();

            byte[] finalArray;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(originalData);
                    
                    UpdateExtensionData(profileID);
                    byte[] modData = SaveSystem.Extensions[profileID].ToBytes();
                    writer.Write(modData);
                    
                    byte[] magicBytes = Encoding.UTF8.GetBytes(SaveSystem.MAGIC_WORD);
                    writer.Write(magicBytes); 
                    writer.Write(modData.Length);
                }
                finalArray = ms.ToArray();
            }

            MelonLogger.Msg($"[SaveFix] Saving slot {profileID}. Total size: {finalArray.Length} bytes.");
            string fileName = $"profile{profileID}.cms21b";
            Singleton<GameManager>.Instance.PlatformManager.SendSave(fileName, (Il2CppStructArray<byte>)finalArray);
        }
        catch (Exception ex) { MelonLogger.Error($"[SaveFix] Error: {ex}"); }
        return false;
    }
    
    private static void UpdateExtensionData(int index)
    {
        if (Server.Instance.isRunning)
        {
            var ext = SaveSystem.Extensions[index];
            if (ServerData.Instance.engineStand2?.engineGroupItem != null)
                ext.AdditionnalStand = ServerData.Instance.engineStand2;

            foreach (var client in ServerData.Instance.connectedClients.Values)
            {
                var pInfo = ext.PlayerInfos.Find(p => p.id == client.playerGUID);
                if (pInfo == null)
                    continue;
                pInfo.UpdateStats(client.position, client.rotation, client.playerExp, client.playerLevel, client.playerSkillPoints);
            }
        }
    }
}