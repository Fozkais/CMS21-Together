using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CMS.ContainersSave;
using CMS.Platforms;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla.Cars;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using UnhollowerBaseLib;
using UnityEngine;
using BinaryWriter = Il2CppSystem.IO.BinaryWriter;

namespace CMS21Together.Shared;

[HarmonyPatch]
public static class SavesManager
{
	private const string MOD_FOLDER_PATH = @"Mods\togetherMod\";
	private const string SAVE_FOLDER_PATH = MOD_FOLDER_PATH + "saves";

	private static readonly string GAME_SAVE_FOLDER = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\LocalLow\Red Dot Games\Car Mechanic Simulator 2021\Save");
	public static Dictionary<int, ModSaveData> ModSaves = new();
	public static Il2CppReferenceArray<ProfileData> profileData;
	public static ProfileData currentSave;
	public static int currentSaveIndex;

	/*public static void Initialize() NEW TO FIX
	{
		profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);
		for (var i = 0; i < 4; i++) profileData[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i];
		
		if (Directory.Exists(SAVE_FOLDER_PATH))
		{
			var savesFiles = new DirectoryInfo(SAVE_FOLDER_PATH).GetFiles("save_*.cms21mp");
			for (int i = 0; i < savesFiles.Length; i++)
			{
				string serializedSave = File.ReadAllText(savesFiles[i].ToString());
				ModSaveData save = JsonConvert.DeserializeObject<ModSaveData>(serializedSave);
				ModSaves[save.saveIndex] = save;
				if (save.alreadyLoaded)
					profileData[save.saveIndex] = LoadProfile(save.saveIndex, GetSave(save.saveIndex));
			}
		}
		
		for (var i = 0; i < MainMod.MAX_SAVE_COUNT + 1; i++)
			if (!ModSaves.ContainsKey(i))
				ModSaves.Add(i, new ModSaveData("EmptySave", i, false)); // Add empty save to ModSaves 

		Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set new array size
	}*/
	
	public static void Initialize()
	{
		profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);
		for (var i = 0; i < 4; i++) profileData[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i];

		LoadExistingModSaves();

		for (var i = 0; i < MainMod.MAX_SAVE_COUNT + 1; i++)
			if (!ModSaves.ContainsKey(i))
				ModSaves.Add(i, new ModSaveData("EmptySave", i, false)); // Add empty save to ModSaves 

		Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set new array size
	}

	private static void LoadExistingModSaves() // TODO:Replace Outdated system by new (see above)
	{
		if (Directory.Exists(SAVE_FOLDER_PATH))
		{
			var saveFolder = new DirectoryInfo(SAVE_FOLDER_PATH);
			var saveFiles = saveFolder.GetFiles("save_*.cms21mp"); // get all saves files.

			var vanillaSaveArray = new Il2CppReferenceArray<SaveData>(4);
			for (var i = 0; i < 4; i++) vanillaSaveArray[i] = GetSave(i);

			for (var i = 0; i < saveFiles.Length; i++)
			{
				var saveFile = saveFiles[i];
				var serializedSave = File.ReadAllText(saveFile.ToString());
				ModSaveData modSave = JsonConvert.DeserializeObject<ModSaveData>(serializedSave);

				ModSaves[modSave.saveIndex] = modSave;
				if (modSave.alreadyLoaded)
				{
					var tempSaveArray = new Il2CppReferenceArray<SaveData>(4);
					tempSaveArray[3] = GetSave(modSave.saveIndex);

					Singleton<GameManager>.Instance.GameDataManager.ReloadProfiles(tempSaveArray);
					var copiedData = DataHelper.Copy(Singleton<GameManager>.Instance.GameDataManager.ProfileData[3]);

					profileData[modSave.saveIndex] = copiedData;
				}
			}

			Singleton<GameManager>.Instance.GameDataManager.ReloadProfiles(vanillaSaveArray);
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
	private static ProfileData LoadProfile(int saveIndex,SaveData saveData2)
	{
		ProfileData data = new ProfileData();
		if (!saveData2.HasData || saveData2.Data == null || saveData2.Data.Length == 0)
		{
			data.Init();
		}
		else if (saveData2.Format == 1)
		{
			Il2CppStructArray<byte> dataRef = saveData2.Data;
			data.DeserializeFromBytes(ref dataRef);
		}
		else
		{
			string @string = Encoding.UTF8.GetString(saveData2.Data);
			if (!string.IsNullOrEmpty(@string))
			{
				try
				{
					data = JsonUtility.FromJson<ProfileData>(@string);
				}
				catch (Exception ex)
				{
					MelonLogger.Warning(string.Format("[SaveManagerHooks] -> LoadProfile() Error while loading profile '{0}'. Error: {1}", saveIndex, ex.Message));
				}
			}
		}

		return data;
	}

	public static void LoadSave(ModSaveData saveData, Dictionary<int, ModNewCarData> carOnPark=null, bool clientSave = false)
	{
		var gameManager = Singleton<GameManager>.Instance;
		int index;
		string name;

		if (clientSave)
		{
			index = MainMod.MAX_SAVE_COUNT;
			name = "ClientSave";
		}
		else
		{
			index = saveData.saveIndex;
			name = saveData.Name;
		}

		currentSaveIndex = index;
		DifficultyLevel difficulty = GetDifficultyFromGamemode(saveData.selectedGamemode);
		gameManager.ProfileManager.selectedProfile = index;
		gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
		
		if (!clientSave)
		{
			if (saveData.alreadyLoaded)
			{
				Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(difficulty);
				gameManager.ProfileManager.Load();
				currentSave = gameManager.ProfileManager.GetSelectedProfileData();
				SaveModSave(index);
				return;
			}

			var writer = new BinaryWriter();
			var save = new ProfileData();

			save.Init();
			save.WriteSaveHeader(writer);
			save.WriteSaveVersion(writer);

			profileData[index] = save;
			Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(name);
			Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(difficulty);
			Singleton<GameManager>.Instance.ProfileManager.Load();
			ModSaves[index].Name = name;
			ModSaves[index].saveIndex = index;
		}
		else
		{
			var writer = new BinaryWriter();
			var save = new ProfileData();

			save.Init();
			save.WriteSaveHeader(writer);
			save.WriteSaveVersion(writer);

			profileData[index] = save;
			gameManager.ProfileManager.selectedProfile = index;
			gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
			Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(name);
			Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(difficulty);
			gameManager.ProfileManager.Load();
			
			currentSave = gameManager.ProfileManager.GetSelectedProfileData();
			if (carOnPark != null)
			{
				for (int i = 0; i < carOnPark.Count; i++)
				{
					var car = carOnPark[i];
					currentSave.carsOnParking[i] = car.ToGame();
				}
			}

		}
		currentSave = gameManager.ProfileManager.GetSelectedProfileData();
		if (!clientSave) SaveModSave(index);
		if (clientSave) StartGame(MainMod.MAX_SAVE_COUNT);
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
	
	public static void SaveModSave(int saveIndex)
	{
		if (!Server.Instance.isRunning) return;
		
		if (ServerData.Instance.engineStand2 != null && ServerData.Instance.engineStand2.engineGroupItem != null)
			ModSaves[saveIndex].additionnalStand = ServerData.Instance.engineStand2;
		foreach (var id in Server.Instance.clients.Keys)
		{
			if (!ServerData.Instance.connectedClients.TryGetValue(id, out var client)) break;
			
			string playerGuid = client.playerGUID;
			PlayerInfo info = ModSaves[currentSaveIndex].playerInfos.FirstOrDefault(p => playerGuid == p.id);
			
			Vector3Serializable pos = client.position;
			QuaternionSerializable rot = client.rotation;
			int lvl = client.playerLevel;
			int exp = client.playerExp;
			int points = client.playerSkillPoints;
			
			info?.UpdateStats(pos, rot, exp , lvl, points);
		}
		
		var saveFilePath = Path.Combine(SAVE_FOLDER_PATH, $"save_{saveIndex}.cms21mp");

		if (!Directory.Exists(SAVE_FOLDER_PATH)) Directory.CreateDirectory(SAVE_FOLDER_PATH);
		
		File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(ModSaves[saveIndex]));
		MelonLogger.Msg("Saved Successfully!");
	}
	
	public static void RemoveModSave(int index)
	{
		ModSaves[index] = new ModSaveData("EmptySave", index, false);
		var modSaveFilePath = Path.Combine(SAVE_FOLDER_PATH, $"save_{index}.cms21mp");
		var saveFilePath = Path.Combine(GAME_SAVE_FOLDER, $"profile{index}.cms21b");
		MelonLogger.Msg($"SavePath:{saveFilePath}");
		if (File.Exists(modSaveFilePath))
		{
			File.Delete(modSaveFilePath);
			MelonLogger.Msg($"Mod save file at {modSaveFilePath} deleted");
		}
		else
		{
			MelonLogger.Error("Error deleting  mod save file ");
		}

		if (File.Exists(saveFilePath))
		{
			var name = Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Name;
			Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Init();
			Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Name = name;
			Singleton<GameManager>.Instance.GameDataManager.ClearData();
			File.Delete(saveFilePath);
			MelonLogger.Msg($"Save file {saveFilePath} deleted");
		}
		else
		{
			MelonLogger.Error("Error deleting save file ");
		}
	}

	public static void StartGame(int index)
	{
		Application.runInBackground = true;

		Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index;
		Singleton<GameManager>.Instance.RDGPlayerPrefs.SetInt("selectedProfile", index);
		Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
		Singleton<GameManager>.Instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(true));

		NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, true));
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

	public static Gamemode GetGamemodeFromDifficulty(DifficultyLevel difficultyLevel) => (Gamemode)difficultyLevel;
}