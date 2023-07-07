using System.IO;
using MelonLoader;

namespace CMS21MP.SharedData
{
    public static class PreferencesManager
    {
        private const string PreferencesFileName = @"Mods\togheterMod\preferences.txt";
        private const string DefaultIPAdress = "127.0.0.1";
        private const string DefaultUsername = "player";

        public static void LoadPreferences()
        {
            if (File.Exists(PreferencesFileName))
            {
                // Le fichier de préférences existe

                // Lire le contenu du fichier
                string[] preferences = File.ReadAllLines(PreferencesFileName);

                // Vérifier le nombre de lignes lues
                if (preferences.Length >= 2)
                {
                    // La première ligne correspond à l'adresse IP
                    string ipAdress = preferences[0];
                    if (!string.IsNullOrEmpty(ipAdress))
                    {
                        // Utiliser la valeur lue comme adresse IP
                        ModUI.Instance.ipAddress = ipAdress;
                    }

                    // La deuxième ligne correspond au nom d'utilisateur
                    string username = preferences[1];
                    if (!string.IsNullOrEmpty(username))
                    {
                        // Utiliser la valeur lue comme nom d'utilisateur
                        ModUI.Instance.username = username;
                    }
                }
                MelonLogger.Msg("Loaded Preferences Succesfully !");
            }
            else
            {
                // Le fichier de préférences n'existe pas

                // Utiliser les valeurs par défaut
                ModUI.Instance.ipAddress = DefaultIPAdress;
                ModUI.Instance.username = DefaultUsername;
            }
        }

        public static void SavePreferences()
        {
            // Créer le contenu du fichier de préférences
            string[] preferences = new string[]
            {
                ModUI.Instance.ipAddress,
                ModUI.Instance.username
            };

            // Écrire le contenu dans le fichier
            File.WriteAllLines(PreferencesFileName, preferences);
        }
    }
}