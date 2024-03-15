using System;
using System.Collections.Generic;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.ContainersSave;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Shared
{
    public static class SavesManager
    {
        public static Dictionary<int, ModSaveData> ModSaves = new Dictionary<int, ModSaveData> ();
        public static Il2CppReferenceArray<ProfileData> profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);
        public static  Il2CppReferenceArray<SaveData> saveData = new Il2CppReferenceArray<SaveData>(4);

        public static string currentSaveName;

        public static void GetVanillaSaves() // Run on game startup
        {
            for (int i = 0; i < 4; i++)
            {
                profileData[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i];
            }
            Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Change array size
        }

        public static void InitializeModdedSaves()
        {
            // Initialize modded saves (+1 is clientSave)
            for (int i = 3; i < MainMod.MAX_SAVE_COUNT + 1; i++)
            {
                ModSaves.Add(i, new ModSaveData("EmptySave", i, false)); // Add empty save to ModSaves
            }
        }

        public static int LoadSave(int index, string saveName, bool clientSave = false)
        {
            int firstEmptySaveIndex = 0;
            for (int i = 0; i < profileData.Count-1; i++)
            {
                firstEmptySaveIndex++;
            }

            if (clientSave) { index = firstEmptySaveIndex; saveName = "ClientSave"; }
            MelonLogger.Msg("-------------------Load Save---------------------");
            MelonLogger.Msg("Index : " + index);
            MelonLogger.Msg("SaveName : " + saveName);
            Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index;
            MelonLogger.Msg("Selected Profile : " +  Singleton<GameManager>.Instance.ProfileManager.selectedProfile);

            Singleton<GameManager>.Instance.RDGPlayerPrefs.SetInt("selectedProfile", index);
            
            currentSaveName = saveName;

            if (ModSaves[index].alreadyLoaded)
            {
                Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index;
                Singleton<GameManager>.Instance.RDGPlayerPrefs.SetInt("selectedProfile", index);
                MelonLogger.Msg("Save already Exist, Loading...");
                Singleton<GameManager>.Instance.ProfileManager.Load();
                MelonLogger.Msg("Selected Profile Name : " +  Singleton<GameManager>.Instance.ProfileManager.GetSelectedProfileName()); 
                MelonLogger.Msg("Selected Profile Difficulty : " +  Singleton<GameManager>.Instance.ProfileManager.GetSelectedProfileDifficulty()); 
                MelonLogger.Msg("Selected Profile : " +  Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                MelonLogger.Msg("-------------------------------------------------");
                return index;
            }
            else
            {
                MelonLogger.Msg("-------------------------------------------------");
            }

            BinaryWriter writer = new BinaryWriter();
            ProfileData save = new ProfileData();
            
            save.Init();
            save.WriteSaveHeader(writer);
            save.WriteSaveVersion(writer);


            int validIndex = firstEmptySaveIndex;
            
            if (!clientSave)
            {
                for (int i = 4; i < MainMod.MAX_SAVE_COUNT + 1; i++)
                {
                    if (String.IsNullOrEmpty(profileData[i].Name))
                    {
                        validIndex = i;
                        break;
                    }
                }
            }

            if (validIndex != -1)
            {
                profileData[validIndex] = save;
                Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(saveName);
                Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
                Singleton<GameManager>.Instance.ProfileManager.Load();
                
                ModSaves[index].Name =  saveName;
                ModSaves[index].saveIndex = index;
                
                if (!clientSave){ PreferencesManager.SaveModSave(index); }
            }

            if (clientSave)
            {
                StartGame(MainMod.MAX_SAVE_COUNT+1);
                return index;
            }

            return validIndex;
        }

        public static void RemoveSave(int index)
        {
            ModSaves[index] = new ModSaveData("EmptySave", index, false);
            PreferencesManager.RemoveModSave(index);
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
            PreferencesManager.SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
        }
    }
}