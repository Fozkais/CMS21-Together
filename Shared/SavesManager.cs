using System;
using System.Collections.Generic;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.ContainersSave;
using Il2CppCMS.Platforms.Steam;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace CMS21Together.Shared
{
    public static class SavesManager
    {
        public static Dictionary<int, ModSaveData> ModSaves = new Dictionary<int, ModSaveData> ();
        public static Il2CppReferenceArray<ProfileData> profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);
        public static string currentSaveName;
        
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

                for (int i = 0; i < saveFiles.Length; i++)
                {
                    var saveFile = saveFiles[i];
                    string serializedSave = File.ReadAllText(saveFile.ToString());
                    ModSaveData modSave = JsonConvert.DeserializeObject<ModSaveData>(serializedSave);
                    
                    ModSaves[modSave.saveIndex] = modSave;
                    if (modSave.alreadyLoaded)
                    {
                        Il2CppReferenceArray<SaveData> tempSaveArray = new Il2CppReferenceArray<SaveData>(4);
                        tempSaveArray[3] = GetSave(new SteamSave(), modSave.saveIndex);
                        
                         Singleton<GameManager>.Instance.GameDataManager.ReloadProfiles(tempSaveArray);
                         ProfileData copiedData = DataHelper.Copy(Singleton<GameManager>.Instance.GameDataManager.ProfileData[3]);

                         profileData[modSave.saveIndex] = copiedData;
                    }
                }
            }
            
        }
        
        private static SaveData GetSave(SteamSave save, int saveIndex)
        {
            Il2CppStructArray<byte> bytes = save.LoadProfileSave(saveIndex, out var format, out var parameter);
                        
            SaveData saveData = new SaveData();
            saveData.Data = bytes;
            saveData.Format = format;
            saveData.HasData = parameter;

            return saveData;
        }


        public static void LoadSave(ModSaveData saveData, bool clientSave = false)
        {
            GameManager gameManager = Singleton<GameManager>.Instance;
            int index;
            string name;
            
            if(clientSave) { index = MainMod.MAX_SAVE_COUNT + 1; name = "ClientSave"; }
            else { index = saveData.saveIndex; name = saveData.Name; }
            
            gameManager.ProfileManager.selectedProfile = index;
            gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
            
            MelonLogger.Msg("-------------------Load Save---------------------");
            MelonLogger.Msg("Index : " + index);
            MelonLogger.Msg("Name : " + name);
            if(!clientSave) { MelonLogger.Msg("Already Loaded : " + saveData.alreadyLoaded); }
            if(!saveData.alreadyLoaded) { MelonLogger.Msg("-------------------------------------------------"); }
            
            if (saveData.alreadyLoaded)
            {
                gameManager.ProfileManager.selectedProfile = index;
                gameManager.RDGPlayerPrefs.SetInt("selectedProfile", index);
                gameManager.ProfileManager.Load();
                
                MelonLogger.Msg("-------------------Save Info-------------------");
                MelonLogger.Msg("Selected Profile Name : " +  gameManager.ProfileManager.GetSelectedProfileName()); 
                MelonLogger.Msg("Selected Profile Difficulty : " +  gameManager.ProfileManager.GetSelectedProfileDifficulty()); 
                MelonLogger.Msg("Selected Profile : " +  gameManager.ProfileManager.selectedProfile);
                MelonLogger.Msg("-------------------------------------------------");
                currentSaveName = name;
                return;
            }
            
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
            currentSaveName = name;
                
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

            Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
            Singleton<GameManager>.Instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(true));
            
            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, true));
        }

        
        [HarmonyPatch(typeof(ProfileManager), "Save")]
        [HarmonyPostfix]
        public static void Savepatch()
        {
            MelonLogger.Msg(" ProfileManager Save Index:" + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
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