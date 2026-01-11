using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using CMS21_Together_Core.Data.GameType;
using Newtonsoft.Json;

namespace CMS21_Together_Server.Data
{
	public static class GameDataManager
	{
		public static Dictionary<string, PartProperty> itemsDatabase;
		public static bool isInitialized { get; private set; }
		
		
		public static void Initialize()
		{
			itemsDatabase = LoadItemDataBase();
			if (itemsDatabase == null)
			{
				Logger.Error("Item Database Loading failed.");
				return;
			}
			isInitialized = true;
		}

		private static Dictionary<string, PartProperty> LoadItemDataBase()
		{
			Dictionary<string, PartProperty> dictionary = new Dictionary<string, PartProperty>();
			
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/item_database.json");
			if (!File.Exists(filePath)) return null;

			string json = File.ReadAllText(filePath);
			
			dictionary = JsonConvert.DeserializeObject<Dictionary<string, PartProperty>>(json);
        
			Logger.Success($"Database loaded with {dictionary.Count} items.");
			return (dictionary);
		}
	}
}