using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using CMS21Together.ServerSide;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Shared;

public static class SteamworksUtils
{
	public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int size)
	{
		byte[] byteArray = new byte[size];
		Marshal.Copy(ptr, byteArray, 0, size);
		return byteArray;
	}
        
	public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
	{
		IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);
		Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
		return ptr;
	}
        
	public static void FreeIntPtr(IntPtr ptr)
	{
		if (ptr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(ptr);
		}
	}

	public static ulong ConvertLobbyID(string lobbyCode)
	{
		lobbyCode = lobbyCode.TrimStart('0');
		lobbyCode = lobbyCode.TrimEnd('0');
        
		ulong lobbyID = ulong.Parse(lobbyCode, System.Globalization.NumberStyles.Integer);
		return lobbyID;
	}
	public static ulong StringToUInt64(string steamIDString)
	{
		if (string.IsNullOrWhiteSpace(steamIDString))
			throw new ArgumentException("Input string cannot be null, empty, or whitespace.");

		
		steamIDString = steamIDString.Trim(); // Nettoyer les espaces et les caractères invisibles
		
		if (!steamIDString.All(char.IsDigit))
			throw new FormatException($"Input string '{steamIDString}' contains invalid characters.");

		if (ulong.TryParse(steamIDString, out ulong steamId))
			return steamId;
		
		MelonLogger.Error($"Input string '{steamIDString}' is out of range for a ulong.");
		return 0;
	}

    
	public static string ConvertServerID(SteamId lobbyID)
	{
		string code = lobbyID.Value.ToBase36();
            
		code = code.PadLeft(5, '0');
		code = code.PadRight(6, '0');
            
		return code;
	}

	public static ServerConnection GetClientFromConnection(Connection connection)
	{
		ServerConnection SV_client = Server.Instance.clients.First(s => s.Value.steam.connection.Id == connection.Id).Value;
		if(SV_client == null)
			MelonLogger.Warning($"[SteamworksUtils->GetClientFromConnection] Did not found a valid client.");
		return SV_client;
	}
}