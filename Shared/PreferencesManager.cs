using System;
using System.Collections.Generic;
using System.IO;
using CMS21Together.ClientSide;
using CMS21Together.Shared.Data;
using Il2Cpp;
using Il2CppCMS.ContainersSave;
using Il2CppCMS.Platforms.Steam;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Newtonsoft.Json;

namespace CMS21Together.Shared
{
    public static class PreferencesManager // TODO:Clean This!
    {
        private const string ModFolderPath = @"Mods\togetherMod\";
        private const string PreferencesFilePath = ModFolderPath + "preferences.cms21mp";
        private const string SaveFolderPath = ModFolderPath + "saves";
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
                    Client.Instance.ip = preferences.IpAddress;
                    Client.Instance.username = preferences.Username;
                }
            }

            MelonLogger.Msg("Loaded Preferences Successfully!");
        }
        else
        {
            // Utiliser les valeurs par défaut
            Client.Instance.ip = DefaultIPAdress;
            Client.Instance.username = DefaultUsername;
        }
    }

    public static void SavePreferences()
    {
        Preferences preferences = new Preferences
        {
            IpAddress = Client.Instance.ip,
            Username = Client.Instance.username
        };

        string serializedPreferences = JsonConvert.SerializeObject(preferences);
        if (!Directory.Exists(ModFolderPath))
        {
            Directory.CreateDirectory(ModFolderPath);
        }
        
        File.WriteAllText(PreferencesFilePath, serializedPreferences);
    }
    
    public static void SaveMelonLog()
    {
         string LogFilePath = @"MelonLoader\Latest.log";
         string NewLogDestination = @"Mods\togetherMod\log\";
        // Répertoire de destination

        // Créer le répertoire de destination s'il n'existe pas
        if (!Directory.Exists(NewLogDestination))
        {
            Directory.CreateDirectory(NewLogDestination);
        }

        // Obtenez la date et l'heure actuelles pour inclure dans le nouveau nom de fichier
        string dateHeureCourante = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Obtenez le nom du fichier à partir du chemin du fichier source
        string nomFichier = Path.GetFileName(LogFilePath);

        // Construisez le nouveau chemin du fichier avec la date et l'heure
        string nouveauCheminFichier = Path.Combine(NewLogDestination, $"{dateHeureCourante}_{nomFichier}.txt");

        try
        {
            // Copier le fichier
            File.Copy(LogFilePath, nouveauCheminFichier);
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"Erreur while saving mod log : {ex.Message}");
        }
    }
}

    public class Preferences
    {
        public string IpAddress { get; set; }
        public string Username { get; set; }
    }
}