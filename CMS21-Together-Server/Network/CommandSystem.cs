using System;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;
using CMS21_Together_Server.Network;

namespace CMS21_Together_Server.Network
{
	public static class CommandSystem
	{
		public static void Execute(string commandLine)
		{
			if (string.IsNullOrWhiteSpace(commandLine)) return;
			
			if (!commandLine.StartsWith("/"))
			{
				Logger.Warn("Commands must start with '/' (e.g. '/help').");
				return;
			}
			
			commandLine = commandLine.Substring(1).Trim();
			if (string.IsNullOrWhiteSpace(commandLine)) return;
			
			string[] args = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string cmd = args[0].ToLower();

			switch (cmd)
			{
				case "help":
					Logger.Info("Available commands:");
					Logger.Info("  help              - Show this help message");
					Logger.Info("  exit / stop       - Stop the server");
					Logger.Info("  kick <id>         - Kick a player by ID");
					Logger.Info("  money add <val>   - Add money");
					Logger.Info("  money set <val>   - Set money");
					Logger.Info("  level set <val>   - Set player level");
					Logger.Info("  exp set <val>     - Set player exp");
					break;

				case "exit":
				case "stop":
					Environment.Exit(0);
					break;

				case "kick":
					if (args.Length > 1 && int.TryParse(args[1], out int playerId))
					{
						if (Server.Clients.ContainsKey(playerId) && Server.Clients[playerId].IsConnected)
						{
							Logger.Info($"Kicking player {playerId}...");
							Server.SendToClient(new DisconnectPacket() { message = "You have been kicked by the server.", playerID = playerId }, playerId);
							Server.Clients[playerId].Disconnect();
						}
						else
						{
							Logger.Warn($"Player ID {playerId} not found or not connected.");
						}
					}
					else
					{
						Logger.Warn("Usage: kick <id>");
					}
					break;

				case "money":
					if (args.Length > 2 && int.TryParse(args[2], out int moneyVal))
					{
						var ws = GameDataManager.CurrentState?.WorldState;
						if (ws == null) { Logger.Warn("World State is not loaded yet."); break; }

						if (args[1].ToLower() == "add") ws.Money += moneyVal;
						else if (args[1].ToLower() == "set") ws.Money = moneyVal;
						else { Logger.Warn("Usage: money add <val> OR money set <val>"); break; }
						
						Logger.Success($"Money is now {ws.Money}$");
						BroadcastWorldState();
					}
					else
					{
						Logger.Warn("Usage: money add <val> OR money set <val>");
					}
					break;

				case "level":
					if (args.Length > 2 && args[1].ToLower() == "set" && int.TryParse(args[2], out int levelVal))
					{
						var ws = GameDataManager.CurrentState?.WorldState;
						if (ws == null) { Logger.Warn("World State is not loaded yet."); break; }

						ws.Level = levelVal;
						Logger.Success($"Level is now {ws.Level}");
						BroadcastWorldState();
					}
					else
					{
						Logger.Warn("Usage: level set <val>");
					}
					break;

				case "exp":
					if (args.Length > 2 && args[1].ToLower() == "set" && int.TryParse(args[2], out int expVal))
					{
						var ws = GameDataManager.CurrentState?.WorldState;
						if (ws == null) { Logger.Warn("World State is not loaded yet."); break; }

						ws.Exp = expVal;
						Logger.Success($"Exp is now {ws.Exp}");
						BroadcastWorldState();
					}
					else
					{
						Logger.Warn("Usage: exp set <val>");
					}
					break;

				default:
					Logger.Warn($"Unknown command: {cmd}. Type 'help' for a list of commands.");
					break;
			}
		}

		private static void BroadcastWorldState()
		{
			if (GameDataManager.CurrentState != null && GameDataManager.CurrentState.WorldState != null)
			{
				Server.SendToClients(GameDataManager.CurrentState.WorldState);
			}
		}
	}
}
