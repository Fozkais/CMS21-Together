using System.IO;
using CMS21_Together_Core.Data;
using CMS21Together.ClientSide.Data;
using Newtonsoft.Json;

namespace CMS21Together.ClientSide;

public static class TogetherModManager
{
	private const string ModFolderPath = @"Mods\togetherMod\";
	private const string userDataPath = ModFolderPath + "userData.ini";

	public static UserData LoadUserData()
	{
		if (ClientData.UserData != null) return ClientData.UserData;

		ClientData.UserData = new UserData();
		if (File.Exists(userDataPath))
		{
			var serializedUserData = File.ReadAllText(userDataPath);
			if (serializedUserData.Length > 0)
			{
				ClientData.UserData = JsonConvert.DeserializeObject<UserData>(serializedUserData);
				if (!ApiCalls.useSteam)
					ClientData.UserData.selectedNetworkType = NetworkType.TCP;
				if (ClientData.UserData != null)
					return ClientData.UserData;
				return new UserData();
			}
		}
		else
		{
			var serializedData = JsonConvert.SerializeObject(ClientData.UserData);
			if (!Directory.Exists(ModFolderPath)) Directory.CreateDirectory(ModFolderPath);
			File.WriteAllText(userDataPath, serializedData);
		}

		return ClientData.UserData;
	}

	public static void SavePreferences()
	{
		var serializedPreferences = JsonConvert.SerializeObject(ClientData.UserData);
		if (!Directory.Exists(ModFolderPath)) Directory.CreateDirectory(ModFolderPath);

		File.WriteAllText(userDataPath, serializedPreferences);
	}
}