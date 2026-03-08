using System;
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
using Il2CppSystem.IO;
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
    
    [HarmonyPatch(typeof(ProfileData), nameof(ProfileData.SerializeToBytes))]
    [HarmonyPostfix]
    public static void SerializePostfix(ProfileData __instance, ref Il2CppStructArray<byte> __result)
    {
        if (__result == null || !Server.Instance.isRunning) return;

        int index = Singleton<GameManager>.Instance.ProfileManager.selectedProfile;
        UpdateExtensionData(index);
        byte[] modData = SaveSystem.Extensions[index].ToBytes();
        
        byte[] originalBytes = new byte[__result.Length];
        for (int i = 0; i < __result.Length; i++) originalBytes[i] = __result[i];

        MemoryStream ms = new MemoryStream();
        ms.Write(originalBytes, 0, originalBytes.Length);
        BinaryWriter writer = new BinaryWriter(ms);
        writer.Write(SaveSystem.MAGIC_WORD);
        writer.Write(modData.Length);
        writer.Write(modData);
        byte[] finalArray = ms.ToArray();
        Il2CppStructArray<byte> il2CppArray = new Il2CppStructArray<byte>(finalArray.Length);
        for (int i = 0; i < finalArray.Length; i++) il2CppArray[i] = finalArray[i];
        __result = il2CppArray;
        
        ms.Dispose();
        writer.Dispose();
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