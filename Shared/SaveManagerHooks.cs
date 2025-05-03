using System.IO;
using CMS.ContainersSave;
using CMS.Platforms.Base;
using CMS.Platforms.Steam;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Text;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using Exception = System.Exception;
using Object = Il2CppSystem.Object;

namespace CMS21Together.Shared;

[HarmonyPatch]
public static class SaveManagerHooks
{
	[HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.ReloadProfiles))]
	[HarmonyPrefix]
	public static bool ReloadProfilesHook(SaveData[] saveData, GameDataManager __instance)
	{
		if (saveData == null)
			return false;
		for (int i = 0; i < saveData.Length; i++)
		{
			if (i >= saveData.Length)
			{
				__instance.ProfileData[i].Init();
			}
			else
			{
				SaveData saveData2 = saveData[i];
				if (!saveData2.HasData || saveData2.Data == null || saveData2.Data.Length == 0)
				{
					__instance.ProfileData[i].Init();
				}
				else if (saveData2.Format == 1)
				{
					Il2CppStructArray<byte> dataRef = saveData[i].Data;
					__instance.ProfileData[i].DeserializeFromBytes(ref dataRef);
				}
				else
				{
					string @string = Encoding.UTF8.GetString(saveData2.Data);
					if (!string.IsNullOrEmpty(@string))
					{
						try
						{
							__instance.ProfileData[i] = JsonUtility.FromJson<ProfileData>(@string);
						}
						catch (Exception ex)
						{
							MelonLogger.Warning(string.Format("[SaveManagerHooks] -> ReloadProfiles() Error while loading profile '{0}'. Error: {1}", i, ex.Message));
						}
					}
				}
			}
		}
		__instance.ProfilesUpdated = true;
		return false;
	}
	
	[HarmonyPatch(typeof(ProfileManager), "Save")]
	[HarmonyPrefix]
	public static void SavePatch(ProfileManager __instance)
	{
		if (!Client.Instance.isConnected) return;

		//MelonLogger.Msg("Save GameProfile");
		//MelonLogger.Msg("ProfileManager Save Index: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
		SavesManager.SaveModSave(__instance.selectedProfile);
	}

	[HarmonyPatch(typeof(GarageLoader), nameof(GarageLoader.Save))]
	[HarmonyPrefix]
	public static bool SaveHook(bool showInfoIfSaveInProgress = false)
	{
		if (!Client.Instance.isConnected) return true;

		//MelonLogger.Msg("Save Game");
		//MelonLogger.Msg("ProfileManager Save Index: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
		GameData.Instance.orderGenerator?.Save();
		SavesManager.SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
		return true;
	}
}