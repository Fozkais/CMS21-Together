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
	        bool hasModData = false;

	        // 2. Extraction du Payload
	        if (currentSave != null && currentSave.HasData)
	        {
	            try
	            {
	                byte[] managedData = currentSave.Data.ToArray();
	                using var ms = new MemoryStream(managedData);
	                using var reader = new BinaryReader(ms);

	                if (TryFindModData(reader, out var payload))
	                {
	                    Extensions[i] = ModProfileExtension.FromBytes(payload);
	                    hasModData = true;
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
	        else if (hasModData)
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
		MelonLogger.Msg($"Trying to load save index:'{index}'.");
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
		MelonLogger.Msg($"Trying to start save index:'{index}'.");
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
			byte[] magicBytes = Encoding.UTF8.GetBytes(MAGIC_WORD);
			long streamLength = reader.BaseStream.Length;
			int magicLength = magicBytes.Length;

			// On scanne le flux octet par octet pour trouver le MAGIC_WORD
			while (reader.BaseStream.Position < streamLength - magicLength)
			{
				bool found = true;
				long startPosition = reader.BaseStream.Position;

				for (int i = 0; i < magicLength; i++)
				{
					if (reader.ReadByte() != magicBytes[i])
					{
						found = false;
						break;
					}
				}

				if (found)
				{
					// On a trouvé le marqueur !
					// On lit maintenant la taille du payload (Int32)
					int payloadLength = reader.ReadInt32();
                
					// Sécurité : on vérifie que la taille n'est pas aberrante
					if (payloadLength > 0 && reader.BaseStream.Position + payloadLength <= streamLength)
					{
						payload = reader.ReadBytes(payloadLength);
						return true;
					}
				}
				else
				{
					// Si on n'a pas trouvé, on revient juste après le premier octet testé
					reader.BaseStream.Position = startPosition + 1;
				}
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"[SavesManager] Error while scanning for mod data: {ex.Message}");
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