using System;
using System.Collections.Generic;
using CMS21Together.ClientSide;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.ContainersSave;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
namespace CMS21Together.Shared
{
    [HarmonyPatch]
    public static class SavesManager
    {
        public static Dictionary<int, ModSaveData> ModSaves = new Dictionary<int, ModSaveData> ();
        public static Il2CppReferenceArray<ProfileData> profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);
        public static ProfileData currentSave;
        
        private const string MOD_FOLDER_PATH = @"Mods\togetherMod\";
        private const string SAVE_FOLDER_PATH = MOD_FOLDER_PATH + "saves";

        public static void Initialize()
        {
            for (int i = 0; i < 4; i++)
            {
                profileData[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i];
            }

            LoadExistingModSaves();
            
            for (int i = 0; i < MainMod.MAX_SAVE_COUNT + 1; i++)
            {
                if(!ModSaves.ContainsKey(i))
                    ModSaves.Add(i, new ModSaveData("EmptySave", i, false)); // Add empty save to ModSaves 
            }
            
            Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set new array size
        }

        private static void LoadExistingModSaves()
        {
            if (Directory.Exists(SAVE_FOLDER_PATH))
            {
                DirectoryInfo saveFolder = new DirectoryInfo(SAVE_FOLDER_PATH);
                FileInfo[] saveFiles = saveFolder.GetFiles("save_*.cms21mp");// get all saves files.

                Il2CppReferenceArray<SaveData> vanillaSaveArray = new Il2CppReferenceArray<SaveData>(4);
                for (int i = 0; i < 4; i++)
                {
                    vanillaSaveArray[i] =  GetSave(i);
                }
                
                for (int i = 0; i < saveFiles.Length; i++)
                {
                    var saveFile = saveFiles[i];
                    string serializedSave = File.ReadAllText(saveFile.ToString());
                    ModSaveData modSave = JsonConvert.DeserializeObject<ModSaveData>(serializedSave);
                    
                    ModSaves[modSave.saveIndex] = modSave;
                    if (modSave.alreadyLoaded)
                    {
                        Il2CppReferenceArray<SaveData> tempSaveArray = new Il2CppReferenceArray<SaveData>(4);
                        tempSaveArray[3] = GetSave(modSave.saveIndex);
                        
                         Singleton<GameManager>.Instance.GameDataManager.ReloadProfiles(tempSaveArray);
                         ProfileData copiedData = DataHelper.Copy(Singleton<GameManager>.Instance.GameDataManager.ProfileData[3]);

                         profileData[modSave.saveIndex] = copiedData;
                    }
                }
                Singleton<GameManager>.Instance.GameDataManager.ReloadProfiles(vanillaSaveArray);
            }
            
        }

        public static ProfileData GetProfile(int saveIndex)
        {
            return Singleton<GameManager>.Instance.GameDataManager.ProfileData[saveIndex];
        }
        
        private static SaveData GetSave(int saveIndex)
        {
            Il2CppStructArray<byte> bytes = LoadProfileSave(saveIndex, out var format, out var parameter);
                        
            SaveData saveData = new SaveData();
            saveData.Data = bytes;
            saveData.Format = format;
            saveData.HasData = parameter;

            return saveData;
        }
        
        private static byte[] LoadProfileSave(int profileIndex, out byte format, out bool hasData)
        {
            string path = string.Format("{0}/profile{1}{2}b", GlobalStrings.SaveDirectory, profileIndex, ".cms21");
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


        public static void LoadSave(ModSaveData saveData, bool clientSave = false)
        {
            GameManager gameManager = Singleton<GameManager>.Instance;
            int index;
            string name;
            
            if(clientSave) { index = MainMod.MAX_SAVE_COUNT; name = "ClientSave"; }
            else { index = saveData.saveIndex; name = saveData.Name; }
            
            gameManager.ProfileManager.selectedProfile = index;
            gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
            
            MelonLogger.Msg("-------------------Load Save---------------------");
            MelonLogger.Msg("Index : " + index);
            MelonLogger.Msg("Name : " + name);
            if(!clientSave)
            {
                MelonLogger.Msg("Already Loaded : " + saveData.alreadyLoaded);
                if(!saveData.alreadyLoaded) { MelonLogger.Msg("-------------------------------------------------"); }
                
                if (saveData.alreadyLoaded)
                {
                    gameManager.ProfileManager.selectedProfile = index;
                    gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
                    gameManager.ProfileManager.Load();
                    Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
                    MelonLogger.Msg("-------------------Save Info---------------------");
                    MelonLogger.Msg("Selected Profile Name : " +  gameManager.ProfileManager.GetSelectedProfileName()); 
                    MelonLogger.Msg("Selected Profile Difficulty : " +  gameManager.ProfileManager.GetSelectedProfileDifficulty()); 
                    MelonLogger.Msg("Selected Profile : " +  gameManager.ProfileManager.selectedProfile);
                    MelonLogger.Msg("-------------------------------------------------");
                    currentSave = gameManager.ProfileManager.GetSelectedProfileData();
                    SaveModSave(index);
                    return;
                }
                else
                {
                    BinaryWriter writer = new BinaryWriter();
                    ProfileData save = new ProfileData();
            
                    save.Init();
                    save.WriteSaveHeader(writer);
                    save.WriteSaveVersion(writer);

                    profileData[index] = save;
                    Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(name);
                    Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
                    Singleton<GameManager>.Instance.ProfileManager.Load();
                
                    ModSaves[index].Name =  name;
                    ModSaves[index].saveIndex = index;
                }
            }
            else
            {
                BinaryWriter writer = new BinaryWriter();
                ProfileData save = new ProfileData();
            
                save.Init();
                save.WriteSaveHeader(writer);
                save.WriteSaveVersion(writer);

                profileData[index] = save;
                gameManager.ProfileManager.selectedProfile = index;
                gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
                Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(name);
                Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
                gameManager.ProfileManager.Load();
                
                MelonLogger.Msg("-------------------Save Info---------------------");
                MelonLogger.Msg("Selected Profile Name : " +  gameManager.ProfileManager.GetSelectedProfileName()); 
                MelonLogger.Msg("Selected Profile Difficulty : " +  gameManager.ProfileManager.GetSelectedProfileDifficulty()); 
                MelonLogger.Msg("Selected Profile : " +  gameManager.ProfileManager.selectedProfile);
                MelonLogger.Msg("-------------------------------------------------");
            }
            Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
            currentSave = gameManager.ProfileManager.GetSelectedProfileData();
                
            if (!clientSave){ SaveModSave(index); }
            if (clientSave) { StartGame(MainMod.MAX_SAVE_COUNT+1); }
        }
        
        
        public static void SaveModSave(int saveIndex)
        {
            ModSaveData modSaveData = SavesManager.ModSaves[saveIndex];
            string saveFilePath = Path.Combine(SAVE_FOLDER_PATH, $"save_{saveIndex}.cms21mp");

            if (!Directory.Exists(SAVE_FOLDER_PATH))
            {
                Directory.CreateDirectory(SAVE_FOLDER_PATH);
            }
        
            JsonConvert.SerializeObject(modSaveData);
            File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(modSaveData));
        
            MelonLogger.Msg("Saved Successfully!");
        }

        
        public static void RemoveModSave(int index) 
        {
            ModSaves[index] = new ModSaveData("EmptySave", index, false);
            string saveFilePath = Path.Combine(SAVE_FOLDER_PATH, $"save_{index}.cms21mp");

            if (File.Exists(saveFilePath)) 
            {
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
            Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index; // <- needed
          //  Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
            Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
           // Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
            Singleton<GameManager>.Instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(true));
            
            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, true));
        }

        
        [HarmonyPatch(typeof(ProfileManager), "Save")]
        [HarmonyPrefix]
        public static void SavePatch(ProfileManager __instance)
        {
            if(!Client.Instance.isConnected) return;
            
            MelonLogger.Msg("Save GameProfile");
            MelonLogger.Msg("ProfileManager Save Index: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            SaveModSave( __instance.selectedProfile);
        }
        
        [HarmonyPatch(typeof(GameDataManager), "Save")]
        [HarmonyPrefix]
        public static void SavePatch2(int profileID)
        {
            if(!Client.Instance.isConnected) return;
            
            MelonLogger.Msg("Save GameData");
            MelonLogger.Msg("ProfileManager Save Index: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
        }
        
        public static void SaveAllModSaves()
        {
            if (!Directory.Exists(SAVE_FOLDER_PATH))
            {
                Directory.CreateDirectory(SAVE_FOLDER_PATH);
            }

            foreach (KeyValuePair<int,ModSaveData> saveData in SavesManager.ModSaves)
            {
                if (saveData.Value.alreadyLoaded)
                {
                    string saveFileName = Path.Combine(SAVE_FOLDER_PATH, $"save_{saveData.Value.saveIndex}.cms21mp");
                    string serializedSave = JsonConvert.SerializeObject(saveData);
                    File.WriteAllText(saveFileName, serializedSave);
                }
            }
        }
    }
}
