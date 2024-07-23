using System.IO;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared.Data;
using Newtonsoft.Json;

namespace CMS21Together.Shared;

public static class TogetherModManager
{
    
    private const string ModFolderPath = @"Mods\togetherMod\";
    private const string userDataPath = ModFolderPath + "userData.ini";
    private static UserData currentUserData;
    
    public static UserData LoadUserData()
    {
        if (currentUserData != null) return currentUserData;
        
        currentUserData = new UserData();
        if (File.Exists(userDataPath))
        {
            string serializedUserData = File.ReadAllText(userDataPath);
            if (serializedUserData.Length > 0)
            {
                currentUserData = JsonConvert.DeserializeObject<UserData>(serializedUserData);
                if (currentUserData != null)
                    return currentUserData;
            }
        }
        else
        {
            string serializedData = JsonConvert.SerializeObject(currentUserData);
            if (!Directory.Exists(ModFolderPath))
            {
                Directory.CreateDirectory(ModFolderPath);
            }
            File.WriteAllText(userDataPath, serializedData);
        }

        return currentUserData;
    }

    public static void SavePreferences()
    {
        string serializedPreferences = JsonConvert.SerializeObject(currentUserData);
        if (!Directory.Exists(ModFolderPath))
        {
            Directory.CreateDirectory(ModFolderPath);
        }
        
        File.WriteAllText(userDataPath, serializedPreferences);
    }
}