using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.ContainersSave;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using BinaryWriter = Il2CppSystem.IO.BinaryWriter;

namespace CMS21MP.SharedData
{
    [HarmonyPatch]
    public static class SaveSystem
    {
        
        public static Dictionary<int, ModSaveData> ModSaves = new Dictionary<int, ModSaveData> (); // contain all "mod" saves
        public static Il2CppReferenceArray<ProfileData> profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1); // unneded? 
        public static  Il2CppReferenceArray<SaveData> saveData = new Il2CppReferenceArray<SaveData>(4); // unneded?

        public static void GetVanillaSaves() // Run on game startup
        {
            for (int i = 0; i <= 3; i++) 
                profileData[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i]; // Add Vanilla save to profileData 
            
            for (int i = 4; i < MainMod.MAX_SAVE_COUNT + 1; i++)
            {
                ModSaves.Add(i, new ModSaveData("EmptySave", i, false)); // Add empty save to ModSaves
            }

            Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Define GameDataManager.ProfileData to 16Array instead of 4Array
        }

        public static int LoadSave(int index, string saveName, bool clientSave = false)
        {
            
            int r = 0;
            for (int i = 0; i < profileData.Count-1; i++)
            {
                r++;
            }
            
            if (clientSave)
            {
                index = r;
                saveName = "ClientSave";
            }
            
            MelonLogger.Msg("-------------------Load Save---------------------");
            MelonLogger.Msg("Index : " + index);
            MelonLogger.Msg("SaveName : " + saveName);
            MelonLogger.Msg("Currently Selected Profile : " +  Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            MelonLogger.Msg("---[Set by player Pref]---");
            PlayerPrefs.SetInt("selectedProfile", index);
            MelonLogger.Msg("Selected Profile : " +  Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            MelonLogger.Msg("PlayerPrefsValue : " + PlayerPrefs.GetInt("selectedProfile"));
            MelonLogger.Msg("---[Set by self]---");
            Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index;
            MelonLogger.Msg("Selected Profile : " +  Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            
            MelonLogger.Msg("---[END OF DEFAULT LOAD INFO]---");
            

            int validIndex = -1;
            
            if (ModSaves[index].alreadyLoaded)
            {
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
                BinaryWriter writer = new BinaryWriter();
                var newSave = new ProfileData();
                
                newSave.Init();
                newSave.WriteSaveHeader(writer);
                newSave.WriteSaveVersion(writer);

                if (!clientSave)
                {
                    for (int i = 4; i <  MainMod.MAX_SAVE_COUNT + 1; i++)
                    {
                        if (String.IsNullOrEmpty(profileData[i].Name))
                        {
                            validIndex = i;
                            MelonLogger.Msg("Current Index : " + i);
                            MelonLogger.Msg("Valid Index Founded : " + validIndex);
                            break;
                        }
                    }
                }
                else
                {
                    validIndex = r;
                }


                if (validIndex != -1)
                {
                    profileData[validIndex] = newSave;
                    Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(saveName);
                    Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox);
                    MelonLogger.Msg("Selected Profile Name : " +  Singleton<GameManager>.Instance.ProfileManager.GetSelectedProfileName()); 
                    MelonLogger.Msg("Selected Profile Difficulty : " +  Singleton<GameManager>.Instance.ProfileManager.GetSelectedProfileDifficulty()); 
                    MelonLogger.Msg("Selected Profile : " +  Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                    Singleton<GameManager>.Instance.ProfileManager.Load();
                    
                    SaveSystem.ModSaves[index].Name =  saveName;
                    SaveSystem.ModSaves[index].saveIndex = index;

                    if (!clientSave)
                    {
                       MelonLogger.Msg("SaveSystem Save Index :" + index); 
                        PreferencesManager.SaveModSave(index);
                    }
                }
            }
            
            if(clientSave)
                StartGame(MainMod.MAX_SAVE_COUNT+1);
            
            
            MelonLogger.Msg("-------------------------------------------------");
            if(clientSave)
                return index;
            return validIndex;
        }

        public static void RemoveSave(int index)
        {

            ModSaves[index] = new ModSaveData("EmptySave", index, false);
            // TODO: Implement save deletion
            
            PreferencesManager.SaveModSave(index);
        }

        public static void StartGame(int index)
        {
            Application.runInBackground = true;
            
           // Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set back new ProfileData
            
            Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index; // <- needed

            Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
            Singleton<GameManager>.Instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(true));
            
            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, false));
        }

        
        [HarmonyPatch(typeof(ProfileManager), "Save")]
        [HarmonyPostfix]
        public static void Savepatch()
        {
            MelonLogger.Msg(" ProfileManager Save Index:" + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            PreferencesManager.SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
        }
        
        
        /*[HarmonyPatch(typeof(GameDataManager), "Save")]
        [HarmonyPostfix]
        public static void Savepatch2(bool forcedUpdated)
        {
            MelonLogger.Msg(" GameDataManager Save Index:" + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            PreferencesManager.SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
        }*/
    }

    [Serializable]
    public class ModSaveData
    {
        public string Name;
        public int saveIndex;
        public bool alreadyLoaded;

        public ModSaveData(string saveName, int index, bool loaded)
        {
            Name = saveName;
            saveIndex = index;
            alreadyLoaded = loaded;
        }

        public ModSaveData() { }
    }
}