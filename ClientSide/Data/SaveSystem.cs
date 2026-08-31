using System;
using System.IO;
using System.Linq;
using System.Text;
using CMS.ContainersSave;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using Newtonsoft.Json;
using UnhollowerBaseLib;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public static class SaveSystem
{
	private static readonly string GAME_SAVE_FOLDER = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\LocalLow\Red Dot Games\Car Mechanic Simulator 2021\Save");
	public const string MAGIC_WORD = "CMS21-TOGETHER";
	public static ModProfileExtension[] Extensions = new ModProfileExtension[MainMod.MAX_SAVE_COUNT];
	public static ProfileData selectedSave;
	public static int selectedSaveIndex;

	public static void Initialize()
	{
	    var gm = Singleton<GameManager>.Instance;
	    gm.RDGPlayerPrefs.SetInt("selectedProfile", 0);
	    gm.ProfileManager.Load();

	    var gdm = gm.GameDataManager;
	    var expandedProfiles = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT);
	    
	    var vanillaSaveArray = new Il2CppReferenceArray<SaveData>(4);
	    for (var i = 0; i < 4; i++) vanillaSaveArray[i] = GetSave(i);
	    gdm.ReloadProfiles(vanillaSaveArray);

	    if (Extensions == null) Extensions = new ModProfileExtension[MainMod.MAX_SAVE_COUNT];

	    for (int i = 0; i < MainMod.MAX_SAVE_COUNT; i++)
	    {
	        SaveData currentSave = GetSave(i);
	        bool hasSaveFile = currentSave != null && currentSave.HasData;

	        // 2. Extraction du Payload (moderne, ou legacy en fallback en lecture seule)
	        if (hasSaveFile)
	        {
	            try
	            {
	                byte[] managedData = currentSave.Data.ToArray();
	                MelonLogger.Msg($"Slot {i} data size: {managedData.Length}");
	                using var ms = new MemoryStream(managedData);
	                using var reader = new BinaryReader(ms);

	                if (TryFindModData(reader, out var payload))
	                {
	                    Extensions[i] = ModProfileExtension.FromBytes(payload);
	                }
	                else if (SaveMigration.TryLoadLegacyExtension(i, out var legacyExtension))
	                {
	                    Extensions[i] = legacyExtension;
	                    MelonLogger.Msg($"[SaveSystem] Slot {i} recovered from legacy (pre-0.4.17) save data.");
	                }
	            }
	            catch (Exception e)
	            {
	                MelonLogger.Error($"[SaveSystem] Error reading slot {i}: {e.Message}");
	            }
	        }

	        if (i < gdm.ProfileData.Length)
	        {
	            expandedProfiles[i] = gdm.ProfileData[i];
	        }
	        else if (hasSaveFile)
	        {
	            var tempArray = new Il2CppReferenceArray<SaveData>(1);
	            tempArray[0] = currentSave;
	            gdm.ReloadProfiles(tempArray);
	            expandedProfiles[i] = DataHelper.Copy(gdm.ProfileData[0]);
	        }
	        else
	        {
	            var empty = new ProfileData();
	            empty.Init();
	            expandedProfiles[i] = empty;
	        }
	        if (Extensions[i] == null) Extensions[i] = new ModProfileExtension();
	    }
	    
	    gdm.ProfileData = expandedProfiles;
	    gdm.ReloadProfiles(vanillaSaveArray);
	    MelonLogger.Msg($"[SaveSystem] Sync complete. Total slots: {expandedProfiles.Length}");
	}

	public static void LoadGame(ModProfileExtension data, int index)
	{
		Singleton<GameManager>.Instance.RDGPlayerPrefs.SetInt("selectedProfile", index);
		Singleton<GameManager>.Instance.ProfileManager.Load();
		
		ProfileData profileData = Singleton<GameManager>.Instance.ProfileManager.GetSelectedProfileData();
		selectedSave = profileData;
		selectedSaveIndex = index;
		
		DifficultyLevel level = GetDifficultyFromGamemode(data.SelectedGamemode);
		Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(level);
		Il2CppSystem.IO.BinaryWriter writer = new Il2CppSystem.IO.BinaryWriter();
	

		if (index == MainMod.MAX_SAVE_COUNT - 1)
		{
			profileData.WriteSaveHeader(writer);
			profileData.WriteSaveVersion(writer);
			Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile("NO_SAVE");
			StartGame(index);
		}
		else if (Singleton<GameManager>.Instance.ProfileManager.GetProfilePlayTime(index) == 0U)
		{
			profileData.WriteSaveHeader(writer);
			profileData.WriteSaveVersion(writer);
			Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(data.Name);
		}
	}
	
	public static void StartGame(int index)
	{
		Application.runInBackground = true;
		
		Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index;
		Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
		Singleton<GameManager>.Instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(true));
		NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, true));
	}
	
	public static void DeleteSave(int index)
	{
		var saveFilePath = Path.Combine(GAME_SAVE_FOLDER, $"profile{index}.cms21b");
		if (File.Exists(saveFilePath))
		{
			if (Singleton<GameManager>.Instance.GameDataManager.ProfileData[index] != null)
			{
				var name = Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Name;
				Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Init();
				Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Name = name;
				Singleton<GameManager>.Instance.GameDataManager.ClearData();
			}
			File.Delete(saveFilePath);
			MelonLogger.Msg($"Save file {saveFilePath} deleted");
		}
		else
		{
			MelonLogger.Error("Error deleting save file ");
		}
	}
	
	private static SaveData GetSave(int saveIndex)
	{
		Il2CppStructArray<byte> bytes = LoadProfileSave(saveIndex, out var format, out var parameter);
		
		var saveData = new SaveData();
		saveData.Data = bytes;
		saveData.Format = format;
		saveData.HasData = parameter;

		return saveData;
	}

	private static byte[] LoadProfileSave(int profileIndex, out byte format, out bool hasData)
	{
		var path = string.Format("{0}/profile{1}{2}b", GlobalStrings.SaveDirectory, profileIndex, ".cms21");
		if (File.Exists(path))
		{
			format = 1;
			hasData = true;
			return File.ReadAllBytes(path);
		}

		path = string.Format("{0}/profile{1}{2}", GlobalStrings.SaveDirectory, profileIndex, ".cms21");
		if (File.Exists(path))
		{
			format = 0;
			hasData = true;
			return File.ReadAllBytes(path);
		}

		format = 0;
		hasData = false;
		return Array.Empty<byte>();
	}
	
	private static bool TryFindModData(BinaryReader reader, out byte[] payload)
	{
		payload = null;
		try
		{
			long fileLength = reader.BaseStream.Length;
			byte[] magicBytes = Encoding.UTF8.GetBytes(MAGIC_WORD);
			int magicLength = magicBytes.Length;

			// Sécurité minimale : le fichier doit au moins contenir Magic + Length (int)
			if (fileLength < magicLength + 4) return false;

			// 1. Lire la taille du payload (les 4 derniers octets du fichier)
			reader.BaseStream.Seek(-4, SeekOrigin.End);
			int payloadLength = reader.ReadInt32();

			// 2. Vérifier le Magic Word juste avant la taille
			// Position = Fin - 4 (length) - magicLength
			reader.BaseStream.Seek(-(4 + magicLength), SeekOrigin.End);
			byte[] foundMagic = reader.ReadBytes(magicLength);

			// Comparaison des Magic Bytes
			for (int i = 0; i < magicLength; i++)
			{
				if (foundMagic[i] != magicBytes[i]) return false;
			}

			// 3. Lire le payload
			// Il se trouve juste avant le Magic Word
			long payloadPos = fileLength - 4 - magicLength - payloadLength;
			if (payloadPos < 0) return false;

			reader.BaseStream.Seek(payloadPos, SeekOrigin.End); // On peut aussi calculer depuis le début
			reader.BaseStream.Position = payloadPos; 
			payload = reader.ReadBytes(payloadLength);

			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"[SaveSystem] Load Error: {ex.Message}");
		}
		return false;
	}
	
	private static DifficultyLevel GetDifficultyFromGamemode(Gamemode saveDataSelectedGamemode)
	{
		if (saveDataSelectedGamemode == Gamemode.Normal)
			return DifficultyLevel.Normal;
		if  (saveDataSelectedGamemode == Gamemode.Easy)
			return DifficultyLevel.Easy;
		if  (saveDataSelectedGamemode == Gamemode.Expert)
			return DifficultyLevel.Expert;
		return DifficultyLevel.Sandbox;
	}
	
	public static Gamemode GetGamemodeFromInt(int selectedGamemode)
	{
		if (selectedGamemode == 1)
			return Gamemode.Normal;
		if (selectedGamemode == 0)
			return Gamemode.Easy;
		if (selectedGamemode == 2)
			return Gamemode.Expert;
		return Gamemode.Sandbox;
	}

	public static Gamemode GetGamemodeFromDifficulty(DifficultyLevel difficultyLevel)
	{
		if (difficultyLevel == DifficultyLevel.Sandbox)
			return Gamemode.Sandbox;
		if (difficultyLevel == DifficultyLevel.Easy)
			return Gamemode.Easy;
		if (difficultyLevel == DifficultyLevel.Expert)
			return Gamemode.Expert;
		return Gamemode.Normal;
	}
}