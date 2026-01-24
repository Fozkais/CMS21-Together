using System.Collections.Generic;
using System.IO;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network.Packets;

namespace CMS21_Together_Server.Data
{
	public static class ServerGameState
	{
		public static ModGameState CurrentState { get; private set; }

		public static void TryLoadSession(string path)
		{
			if (path != null && File.Exists(path))
			{
				//TODO: Load ModGameState from file
				return;
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
			Logger.Info("New game created.");
		}

		public static void LoadSession(ModGameState saveData)
		{
			CurrentState = saveData;
			Logger.Info("Game loaded from save.");
		}
	}
}