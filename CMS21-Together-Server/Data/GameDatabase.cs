using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Server.Log;
using Newtonsoft.Json;

namespace CMS21_Together_Server.Data
{
	public static class GameDatabase
	{
		public static Dictionary<string, PartProperty> ItemsDatabase;
		public static Dictionary<string, Dictionary<int, UpgradeData>> GarageUpgrades;
		public static PlayerUpgrades PlayerUpgrades;
		
		public static bool isInitialized { get; private set; }
		
		public static void Initialize()
		{
			ItemsDatabase = LoadItemDataBase();
			if (ItemsDatabase == null)
				return;
				
			var moddedItems = LoadModdedItemDataBase();
			if (moddedItems != null)
			{
				foreach (var kvp in moddedItems)
				{
					if (!ItemsDatabase.ContainsKey(kvp.Key))
					{
						ItemsDatabase.Add(kvp.Key, kvp.Value);
					}
				}
			}
				
			GarageUpgrades = LoadGarageUpgradeDatabase();
			if (GarageUpgrades == null)
				return;
			PlayerUpgrades = LoadPlayerUpgradeDatabase();
			if (PlayerUpgrades == null)
				return;
			
			isInitialized = true;
		}

		private static Dictionary<string, PartProperty> LoadItemDataBase()
		{
			Dictionary<string, PartProperty> dictionary = new Dictionary<string, PartProperty>();
			
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database/item_database.json");
			if (!File.Exists(filePath))
			{
				Logger.Error("Item Database Loading failed.");
				return null;
			}

			string json = File.ReadAllText(filePath);
			
			dictionary = JsonConvert.DeserializeObject<Dictionary<string, PartProperty>>(json);
        
			Logger.Success($"Database loaded with {dictionary.Count} items.");
			return (dictionary);
		}
		
		private static Dictionary<string, PartProperty> LoadModdedItemDataBase()
		{
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database/modded_item_database.json");
			if (!File.Exists(filePath))
			{
				return new Dictionary<string, PartProperty>();
			}

			try
			{
				string json = File.ReadAllText(filePath);
				var dict = JsonConvert.DeserializeObject<Dictionary<string, PartProperty>>(json);
				Logger.Info($"Loaded {dict.Count} previously registered mod items.");
				return dict;
			}
			catch (Exception ex)
			{
				Logger.Error($"Error loading modded items: {ex.Message}");
				return new Dictionary<string, PartProperty>();
			}
		}

		public static void SaveModdedItems()
		{
			try
			{
				// Only save items that are flagged as IsMod
				var moddedItems = ItemsDatabase.Where(x => x.Value.IsMod).ToDictionary(x => x.Key, x => x.Value);
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database/modded_item_database.json");
				string json = JsonConvert.SerializeObject(moddedItems, Formatting.Indented);
				File.WriteAllText(filePath, json);
			}
			catch (Exception ex)
			{
				Logger.Error($"Error saving modded items: {ex.Message}");
			}
		}
		
		private static Dictionary<string, Dictionary<int, UpgradeData>> LoadGarageUpgradeDatabase()
		{
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database/garage_upgrade_database.json");
			if (!File.Exists(filePath)) 
			{
				Logger.Error("Garage upgrade database not found.");
				return null;
			}

			try
			{
				string json = File.ReadAllText(filePath);
				var list = JsonConvert.DeserializeObject<List<UpgradeData>>(json);
				
				var dictionary = list
					.GroupBy(x => x.upgradeID)
					.ToDictionary(
						group => group.Key,
						group => group.ToDictionary(x => x.upgradeLevel, x => x)
					);

				Logger.Success($"Garage Database loaded with {dictionary.Count} unique upgrade types.");
				return dictionary;
			}
			catch (Exception ex)
			{
				Logger.Error($"Error loading Garage Upgrade DB: {ex.Message}");
				return null;
			}
		}
		
		private static PlayerUpgrades LoadPlayerUpgradeDatabase()
		{
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database/player_upgrade_database.json");
			if (!File.Exists(filePath))
			{
				Logger.Warn("Player upgrade database not found.");
				return null;
			}

			try
			{
				string json = File.ReadAllText(filePath);
				var database = JsonConvert.DeserializeObject<PlayerUpgrades>(json);

				Logger.Success($"Player Database loaded: {database.MoneyUpgrades.Count} money-based and {database.PointUpgrades.Count} point-based upgrades.");
				return database;
			}
			catch (Exception ex)
			{
				Logger.Error($"Error loading Player Upgrade DB: {ex.Message}");
				return null;
			}
		}
	}
}