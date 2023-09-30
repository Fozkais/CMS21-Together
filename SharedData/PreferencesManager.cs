using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using MelonLoader;
using System.IO;
using System.Threading.Tasks;
using Il2CppCMS.ContainersSave;
using Il2CppCMS.Platforms.Base;
using Il2CppCMS.Platforms.Steam;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Newtonsoft.Json;
using UnityEngine;

namespace CMS21MP.SharedData
{

public static class PreferencesManager
{
    private const string PreferencesFilePath = @"Mods\togetherMod\preferences.cms21mp";
    private const string SaveFolderPath = @"Mods\togetherMod\saves";
    private const string DefaultIPAdress = "127.0.0.1";
    private const string DefaultUsername = "player";

    public static void LoadPreferences()
    {
        if (File.Exists(PreferencesFilePath))
        {
            // Lire le contenu du fichier
            string serializedPreferences = File.ReadAllText(PreferencesFilePath);

            if (serializedPreferences.Length > 0)
            {
                Preferences preferences = JsonConvert.DeserializeObject<Preferences>(serializedPreferences);

                if (preferences != null)
                {
                    ModUI.Instance.ipAddress = preferences.IpAddress;
                    ModUI.Instance.username = preferences.Username;
                }
            }

            MelonLogger.Msg("Loaded Preferences Successfully!");
        }
        else
        {
            // Utiliser les valeurs par défaut
            ModUI.Instance.ipAddress = DefaultIPAdress;
            ModUI.Instance.username = DefaultUsername;
        }
    }

    public static void SavePreferences()
    {
        Preferences preferences = new Preferences
        {
            IpAddress = ModUI.Instance.ipAddress,
            Username = ModUI.Instance.username
        };

        string serializedPreferences = JsonConvert.SerializeObject(preferences);
        File.WriteAllText(PreferencesFilePath, serializedPreferences);
    }

    public static void SaveAllModSaves()
    {
        if (!Directory.Exists(SaveFolderPath))
        {
            Directory.CreateDirectory(SaveFolderPath);
        }

        foreach (KeyValuePair<int,ModSaveData> saveData in SaveSystem.ModSaves)
        {
            if (saveData.Value.alreadyLoaded)
            {
                string saveFileName = Path.Combine(SaveFolderPath, $"save_{saveData.Value.saveIndex}.cms21mp");
                string serializedSave = JsonConvert.SerializeObject(saveData);
                File.WriteAllText(saveFileName, serializedSave);
            }
        }
    }

    public static void LoadAllModSaves()
    {
        SteamSave save = new SteamSave();
        byte format = 1;
        bool parameter = true;
        
        if (Directory.Exists(SaveFolderPath))
        {
            DirectoryInfo saveFolder = new DirectoryInfo(SaveFolderPath);
            FileInfo[] saveFiles = saveFolder.GetFiles("save_*.cms21mp");
            
            Il2CppReferenceArray<ProfileData> tempProfileData = new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);

            for (int i = 0; i <= 3; i++)
            {
                tempProfileData[i] = SaveSystem.profileData[i];
            }
            for (int i = 0; i < saveFiles.Length; i++)
            {
                var saveFile = saveFiles[i];
                string serializedSave = File.ReadAllText(saveFile.ToString());
                ModSaveData modSaveData = JsonConvert.DeserializeObject<ModSaveData>(serializedSave);
                if (modSaveData.saveIndex < SaveSystem.ModSaves.Count)
                {
                    SaveSystem.ModSaves[modSaveData.saveIndex] = modSaveData;
                    if (modSaveData.alreadyLoaded)
                    {
                        SaveData newsaveData = UpdateSave(save, modSaveData.saveIndex, format, parameter);
                        SaveSystem.saveData[3] = newsaveData; // Use One of the vanilla save slots to load the save
                        
                        Singleton<GameManager>.Instance.GameDataManager.ReloadProfiles(SaveSystem.saveData);
                        ProfileData data = Singleton<GameManager>.Instance.GameDataManager.ProfileData[3];
                        
                        ProfileData copiedData = DeepCopy.Copy(data);
                        
                        tempProfileData[modSaveData.saveIndex] = copiedData;
                      //  MelonLogger.Msg("Added to tempProfileData:" + tempProfileData[modSaveData.saveIndex].carsInGarage[0].carToLoad);
                    }
                }
            }
            int index2 = 0;
            foreach (var car in  tempProfileData)
            {
                if(car != null)
                    SaveSystem.profileData[index2] = car;
                
                index2++;
            }
            Singleton<GameManager>.Instance.GameDataManager.ProfileData = SaveSystem.profileData;
        }
    }
    private static SaveData UpdateSave(SteamSave save, int saveIndex, byte format, bool parameter)
    {
        Il2CppStructArray<byte> bytes = save.LoadProfileSave(saveIndex, out format, out parameter);
                        
        SaveData saveData = new SaveData();
        saveData.Data = bytes;
        saveData.Format = format;
        saveData.HasData = parameter;

        return saveData;
    }

    public static void SaveModSave(int saveIndex)
    {
        if (SaveSystem.ModSaves.ElementAt(saveIndex).Value.Name != "EmptySave")
        {
            ModSaveData modSaveData = SaveSystem.ModSaves[saveIndex];
            string saveFilePath = Path.Combine(SaveFolderPath, $"save_{saveIndex}.cms21mp");

            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }
            
               JsonConvert.SerializeObject(modSaveData);
               
            File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(modSaveData));
            
            MelonLogger.Msg("Saved Successfully!");
        }
    }
}

public class Preferences
{
    public string IpAddress { get; set; }
    public string Username { get; set; }
}





}