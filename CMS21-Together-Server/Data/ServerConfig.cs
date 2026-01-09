using System;
using System.IO;

namespace CMS21_Together_Server.Data
{
	public class ServerConfig
	{
		private const string ConfigFileName = "server_config.ini";
        
        public int MaxPlayers { get; }
        public bool UseSteam { get; }
        public string GsltToken { get; }
        public int LogLevel { get; }
        
        private ServerConfig(int maxPlayers, bool useSteam, string gsltToken, int logLevel)
        {
            MaxPlayers = maxPlayers;
            UseSteam = useSteam;
            GsltToken = gsltToken;
            LogLevel = logLevel;
        }

        public static ServerConfig LoadOrCreate()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

            if (!File.Exists(filePath))
            {
                Logger.Warn($"Configuration file '{ConfigFileName}' not found.");
                CreateDefaultConfig(filePath);
                
                // Return default values
                return new ServerConfig(4,true, string.Empty, 0);
            }

            Logger.Info($"Loading configuration from '{ConfigFileName}'...");
            return ParseConfig(filePath);
        }

        private static void CreateDefaultConfig(string path)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("# Maximum number of players allowed (mod is designed with 4 players in mind, higher might cause issue)");
                    sw.WriteLine("max_players = 4");
                    sw.WriteLine("");
                    sw.WriteLine("# Enable Steam Transport (True/False)");
                    sw.WriteLine("use_steam = True");
                    sw.WriteLine("");
                    sw.WriteLine("# Game Server Login Token (GSLT)");
                    sw.WriteLine("# Required for persistent ServerID. Leave empty \"\" for anonymous login.");
                    sw.WriteLine("# Generate one here: https://steamcommunity.com/dev/managegameservers");
                    sw.WriteLine("GSLT_Token = \"\"");
                    sw.WriteLine("");
                    sw.WriteLine("# Log Level Configuration");
                    sw.WriteLine("# 0 = Base (Info, Warn, Error, Success)");
                    sw.WriteLine("# 1 = Debug (Show all internal messages)");
                    sw.WriteLine("log_level = 0");
                }
                Logger.Info($"Created default configuration file at: {path}");
                Logger.Warn("Please edit the config file to add your GSLT token if needed.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create config file: {ex.Message}");
            }
        }

        private static ServerConfig ParseConfig(string path)
        {
            int maxPlayers = 4;
            bool useSteam = true;
            string gsltToken = string.Empty;
            int logLevel = 0;

            try
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#")) 
                    {
                        continue; 
                    }
                    
                    if (trimmedLine.Contains("#"))
                    {
                        trimmedLine = trimmedLine.Split('#')[0].Trim();
                    }

                    string[] parts = trimmedLine.Split('=');
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (key.Equals("use_steam", StringComparison.OrdinalIgnoreCase))
                    {
                        bool.TryParse(value, out useSteam);
                    }
                    else if (key.Equals("max_players", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(value, out maxPlayers);
                    }
                    else if (key.Equals("GSLT_Token", StringComparison.OrdinalIgnoreCase))
                    {
                        gsltToken = value.Replace("\"", "");
                    }
                    else if (key.Equals("log_level", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(value, out logLevel);
                    }
                }

                Logger.Success("Configuration loaded successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading configuration file: {ex.Message}. Using defaults.");
            }

            return new ServerConfig(maxPlayers, useSteam, gsltToken, logLevel);
        }
	}
}