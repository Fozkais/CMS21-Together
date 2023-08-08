using System;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.SharedData;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.ContainersSave;
using Il2CppCMS.Platforms.Base;
using Il2CppCMS.Platforms.Steam;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using BinaryWriter = Il2CppSystem.IO.BinaryWriter;
using SystemWriter =  System.IO.BinaryWriter;

namespace CMS21MP
{
    public static class SaveSystem
    {
        
        public static Dictionary<int, ModSaveData> ModSaves = new Dictionary<int, ModSaveData> ();
        public static Il2CppReferenceArray<ProfileData> profileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);
        public static  Il2CppReferenceArray<SaveData> saveData = new Il2CppReferenceArray<SaveData>(4);

        public static void InitializeSave()
        {
            for (int i = 0; i <= 3; i++) // Add the 4 vanillaSaves
                profileData[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i];
            
            for (int i = 4; i < MainMod.MAX_SAVE_COUNT + 1; i++)
            {
                ModSaves.Add(i, new ModSaveData("EmptySave", i, false));
            }

            Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set back new ProfileData
        }

        public static int LoadSave(int index, string saveName, bool clientSave = false)
        {
            if (!clientSave)
            {
                if (ModSaves[index].alreadyLoaded)
                {
                    Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index; // <- needed
                    Singleton<GameManager>.Instance.ProfileManager.Load(); // <- needed 
                    
                    Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index; // <- needed
                }
                else
                {
                    BinaryWriter writer = new BinaryWriter();
                    var save = new ProfileData();
                    
                    save.Init(); // <- not sure if needed 
                    save.WriteSaveHeader(writer); // <- needed 

                    profileData[profileData.Count - 1] = save; // set the new save after all "mod" save and vanilla save
                    Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Update ProfileData
                    
                    Singleton<GameManager>.Instance.ProfileManager.selectedProfile =  Singleton<GameManager>.Instance.GameDataManager.ProfileData.Count - 1; // <- needed
                    Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox); // <- not sure if needed 
                    Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(saveName); // -> here keys is all key from save dict converted to a string array to set saveName
                    Singleton<GameManager>.Instance.ProfileManager.Load(); // <- needed 

                    
                    // mettre a jour la nouvelle save dans la liste
                    SaveSystem.ModSaves[index].Name =  saveName;
                    SaveSystem.ModSaves[index].saveIndex = index;
                   // SaveSystem.ModSaves[index].saveData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.ToString();

                    PreferencesManager.SaveModSave(index);
                    
                   // MelonLogger.Msg("SaveIndex: " + ModSaves[index].saveIndex + " | ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                   // MelonLogger.Msg("Index : " + index);
                    MelonLogger.Msg(Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.Name);
                    
                    
                }
                MelonLogger.Msg("SaveIndex: " + ModSaves[index].saveIndex + " | ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                MelonLogger.Msg("Index : " + index);
                MelonLogger.Msg("SaveName : " + ModSaves[index].Name + " | ProfileManager: " + Singleton<GameManager>.Instance.GameDataManager.ProfileData[index].Name);
                MelonLogger.Msg("ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                MelonLogger.Msg(Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.Name);
                return index;
            }
            else
            {
                MelonLogger.Msg("LoadAClientSave");
                BinaryWriter writer = new BinaryWriter();
                var save = new ProfileData();
                    
                save.Init(); // <- not sure if needed 
                save.WriteSaveHeader(writer); // <- needed 
                save.WriteSaveVersion(writer); // <- needed 

                profileData[MainMod.MAX_SAVE_COUNT] = save; // set the new save after all "mod" save and vanilla save
                Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Update ProfileData
                    
                Singleton<GameManager>.Instance.ProfileManager.selectedProfile = MainMod.MAX_SAVE_COUNT; // <- needed
                Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox); // <- not sure if needed 
                Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile("Client"); // -> here keys is all key from save dict converted to a string array to set saveName
                Singleton<GameManager>.Instance.ProfileManager.Load(); // <- needed 

                    
                // mettre a jour la nouvelle save dans la liste
                SaveSystem.ModSaves[MainMod.MAX_SAVE_COUNT].Name =  "Client";
                SaveSystem.ModSaves[MainMod.MAX_SAVE_COUNT].saveIndex = MainMod.MAX_SAVE_COUNT;
                // SaveSystem.ModSaves[index].saveData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.ToString();

               // PreferencesManager.SaveModSave(ModSaves.Count - 1);
                    
                // MelonLogger.Msg("SaveIndex: " + ModSaves[index].saveIndex + " | ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                // MelonLogger.Msg("Index : " + index);
                MelonLogger.Msg(Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.Name);
                
                MelonLogger.Msg("SaveIndex: " +  MainMod.MAX_SAVE_COUNT + " | ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                MelonLogger.Msg("Index : " +  MainMod.MAX_SAVE_COUNT);
                MelonLogger.Msg("SaveName : " + ModSaves[MainMod.MAX_SAVE_COUNT].Name + " | ProfileManager: " + Singleton<GameManager>.Instance.GameDataManager.ProfileData[MainMod.MAX_SAVE_COUNT].Name);
                MelonLogger.Msg("ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
                MelonLogger.Msg(Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.Name);
                
                StartGame(MainMod.MAX_SAVE_COUNT);
                
                return  MainMod.MAX_SAVE_COUNT;
            }
        }

        public static void LoadClientSave()
        {
            BinaryWriter writer = new BinaryWriter();
            var save = new ProfileData();
                
            save.Init(); // <- not sure if needed 
            save.WriteSaveHeader(writer); // <- needed 
            save.WriteSaveVersion(writer); // <- needed 

            profileData[profileData.Count - 1] = save; // set the new save after all "mod" save and vanilla save
                
            Singleton<GameManager>.Instance.ProfileManager.selectedProfile = profileData.Count - 1; // <- needed
            
            Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Sandbox); // <- not sure if needed 
            Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile("ClientSave"); // -> here keys is all key from save dict converted to a string array to set saveName
            Singleton<GameManager>.Instance.ProfileManager.Load(); // <- needed 

            Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set back new ProfileData
            
            SaveSystem.ModSaves[ModSaves.Count - 1].Name =  "ClientSave";
            SaveSystem.ModSaves[ModSaves.Count - 1].saveIndex = profileData.Count - 1;

            MelonLogger.Msg("SaveIndex: " + ModSaves[ModSaves.Count - 1].saveIndex + " | ProfileManager: " + Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
            MelonLogger.Msg("Index : " + (ModSaves.Count - 1));
            MelonLogger.Msg(Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.Name);
            
            StartGame(profileData.Count - 1);
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
            
            Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData; // Set back new ProfileData
            
            Singleton<GameManager>.Instance.ProfileManager.selectedProfile = index; // <- needed

            Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
            Singleton<GameManager>.Instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(true));
            
            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, false));
        }

        
        [HarmonyPatch(typeof(ProfileManager), "Save")]
        [HarmonyPostfix]
        public static void Savepatch()
        {
            PreferencesManager.SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
        }
        
        
        [HarmonyPatch(typeof(GameDataManager), "Save")]
        [HarmonyPostfix]
        public static void Savepatch2(bool forcedUpdated)
        {
            PreferencesManager.SaveModSave(Singleton<GameManager>.Instance.ProfileManager.selectedProfile);
        }
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