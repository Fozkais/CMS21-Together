using System;
using System.Collections.Generic;
using System.IO;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network.Packets;
using Newtonsoft.Json;

namespace CMS21_Together_Server.Data
{
	public static class GameDataManager
	{
		private static readonly string SaveFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
		private static readonly string DefaultSavePath = Path.Combine(SaveFolderPath, "server_save.json");
		private const int MaxBackups = 3;
		
		public static float lastAutoSaveTime;
		public const float AutoSaveInterval = 300f;
		
		public static ModGameState CurrentState { get; private set; }

		public static void TryLoadSession(string path=null)
		{
			string targetPath = path ?? DefaultSavePath;

			if (File.Exists(targetPath))
			{
				try
				{
					string json = File.ReadAllText(targetPath);
					var saveData = JsonConvert.DeserializeObject<ModGameState>(json);

					if (saveData != null)
					{
						LoadSession(saveData);
						return;
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Error loading save file: {ex.Message}. Creating new session instead.");
				}
			}
			CreateNewSession();
		}
		
		public static void CreateNewSession()
		{
			CurrentState = new ModGameState();

			CurrentState.WorldState.Gamemode = Gamemode.Normal;
			CurrentState.WorldState.Money = 12500;
			CurrentState.WorldState.Level = 8;
			CurrentState.WorldState.Exp = 480;
			
			foreach (var id in GameDatabase.GarageUpgrades.Keys)
				CurrentState.GarageState.GarageUpgradeLevels[id] = 0;
			foreach (var upg in GameDatabase.PlayerUpgrades.MoneyUpgrades)
				CurrentState.GarageState.PlayerUpgradeLevels[upg.ID] = 0;
			foreach (var upg in GameDatabase.PlayerUpgrades.PointUpgrades)
				CurrentState.GarageState.PlayerUpgradeLevels[upg.ID] = 0;
			SaveSession();
		}

		public static void SaveSession()
		{
			if (CurrentState == null) return;

			try
			{
				if (!Directory.Exists(SaveFolderPath))
					Directory.CreateDirectory(SaveFolderPath);
				
				RotateBackups();

				string json = JsonConvert.SerializeObject(CurrentState, Formatting.Indented);
				File.WriteAllText(DefaultSavePath, json);

				Logger.Info($"Session successfully saved to: {DefaultSavePath}");
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to save session: {ex.Message}");
			}
		}

		public static void LoadSession(ModGameState saveData)
		{
			CurrentState = saveData;
			Logger.Info("Game session loaded successfully from file.");
		}
		
		private static void RotateBackups()
		{
			if (!File.Exists(DefaultSavePath)) return;
            
			for (int i = MaxBackups - 1; i >= 1; i--)
			{
				string oldPath = GetBackupPath(i);
				string newPath = GetBackupPath(i + 1);

				if (File.Exists(oldPath))
				{
					if (File.Exists(newPath)) File.Delete(newPath);
					File.Move(oldPath, newPath);
				}
			}
			
			string firstBackup = GetBackupPath(1);
			if (File.Exists(firstBackup)) File.Delete(firstBackup);
			File.Move(DefaultSavePath, firstBackup);
		}
		
		private static string GetBackupPath(int index)
		{
			return Path.Combine(SaveFolderPath, $"server_save_bak{index}.json");
		}
	}
}